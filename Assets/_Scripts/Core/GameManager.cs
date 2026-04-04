using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { MainMenu, Playing, Paused, Win, Lose }
    public GameState currentState;

    [Header("UI Screens")]
    public GameObject mainMenuCanvas;
    public GameObject hudCanvas;
    public GameObject pauseCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;

    private FirstPersonController _fps;
    private HUDController _hud;
    public static HUDController HUD => Instance._hud; // easy global access
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

void Start()
{
    _fps = FindFirstObjectByType<FirstPersonController>(FindObjectsInactive.Include);
    
    // HUDCanvas starts disabled so Awake() never runs on it — find it manually
    var hud = FindFirstObjectByType<HUDController>(FindObjectsInactive.Include);
    if (hud != null) HUDController.Register(hud);
    
    ShowMainMenu();
}

    void Update()
    {
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // Called by PuzzleManager.ClosePuzzle() to restore cursor + FPS after a puzzle closes
    public void RestorePlayState()
    {
        if (currentState != GameState.Playing) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        if (_fps != null) _fps.enabled = true;
    }

    public void ShowMainMenu()
    {
        currentState = GameState.MainMenu;
        SetScreens(mainMenu: true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (_fps != null) _fps.enabled = false;
    }

    public void StartGame()
{
    currentState = GameState.Playing;
    SetScreens(hud: true);
    Time.timeScale   = 1f;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible   = false;
    InstabilityManager.Instance.StartGame();
    if (_fps != null) _fps.enabled = true;
    
    // ← force reset puzzle state on every new game
    if (PuzzleManager.Instance != null)
        PuzzleManager.Instance.ForceReset();
}

    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            SetScreens(hud: true);
            Time.timeScale   = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            if (_fps != null) _fps.enabled = true;
        }
        else
        {
            currentState = GameState.Paused;
            SetScreens(hud: true, pause: true);
            Time.timeScale   = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            if (_fps != null) _fps.enabled = false;
        }
    }

   public void TriggerWin()
{
    currentState = GameState.Win;
    SetScreens(win: true);
    Time.timeScale   = 0f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible   = true;
    if (_fps != null) _fps.enabled = false;

    // disable raycaster too
    var raycaster = FindFirstObjectByType<InteractionRaycaster>();
    if (raycaster != null) raycaster.enabled = false;
}
    public void TriggerGameOver()
    {
        currentState = GameState.Lose;
        SetScreens(lose: true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (_fps != null) _fps.enabled = false;
         var raycaster = FindFirstObjectByType<InteractionRaycaster>();
    if (raycaster != null) raycaster.enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        ShowMainMenu();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetScreens(bool mainMenu = false, bool hud = false,
                    bool pause = false,    bool win = false, bool lose = false)
    {
        if (mainMenuCanvas) mainMenuCanvas.SetActive(mainMenu);
        if (hudCanvas)      hudCanvas.SetActive(hud);
        if (pauseCanvas)    pauseCanvas.SetActive(pause);
        if (winCanvas)      winCanvas.SetActive(win);
        if (loseCanvas)     loseCanvas.SetActive(lose);
    }
}