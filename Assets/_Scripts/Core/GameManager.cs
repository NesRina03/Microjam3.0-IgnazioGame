using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { MainMenu, Playing, Paused, Win, Lose, Level2 }
    public GameState currentState;
    GameState _stateBeforePause;

    [Header("Level Settings")]
    public bool enableLevel2 = false;

    [Header("UI Screens")]
    public GameObject mainMenuCanvas;
    public GameObject hudCanvas;
    public GameObject pauseCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    public GameObject optionsCanvas;
    public GameObject level2Canvas;

    private FirstPersonController _fps;
    private HUDController _hud;
    private bool _optionsOpen;
    private GameState _stateBeforeOptions;
    public static HUDController HUD => Instance._hud; // easy global access

    bool EnsureOptionsAssigned()
    {
        if (optionsCanvas != null) return true;

        OptionsManager mgr = FindFirstObjectByType<OptionsManager>(FindObjectsInactive.Include);
        if (mgr != null)
            optionsCanvas = mgr.optionsPanel != null ? mgr.optionsPanel : mgr.gameObject;

        if (optionsCanvas == null)
            optionsCanvas = GameObject.Find("OptionsPanel") ?? GameObject.Find("Options");

        if (optionsCanvas == null)
            Debug.LogError("GameManager: optionsCanvas is not assigned and could not be auto-found.");

        return optionsCanvas != null;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

void Start()
{
    AudioSettings.Load();
    _fps = FindFirstObjectByType<FirstPersonController>(FindObjectsInactive.Include);
    
    // HUDCanvas starts disabled so Awake() never runs on it — find it manually
    var hud = FindFirstObjectByType<HUDController>(FindObjectsInactive.Include);
    if (hud != null) HUDController.Register(hud);
    
    ShowMainMenu();
}

    void Update()
    {
        // Allow pausing both in Playing and Level2 states
        if ((currentState == GameState.Playing || currentState == GameState.Level2) && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // Called by PuzzleManager.ClosePuzzle() to restore cursor + FPS after a puzzle closes
    public void RestorePlayState()
    {
        // Restore controls for Playing or Level2
        if (currentState != GameState.Playing && currentState != GameState.Level2) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        if (_fps != null) _fps.enabled = true;
    }

    public void ShowMainMenu()
    {
        currentState = GameState.MainMenu;
        SetScreens(mainMenu: true);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
        _optionsOpen = false;
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (_fps != null) _fps.enabled = false;
    }

    public void StartGame()
{
    currentState = GameState.Playing;
    SetScreens(hud: true);
    if (optionsCanvas != null) optionsCanvas.SetActive(false);
    _optionsOpen = false;
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
        if (_optionsOpen) return;

        if (currentState == GameState.Paused)
        {
            // Unpause: return to whatever state we were in before pause
            currentState = _stateBeforePause;

            // Show correct screens based on restored state
            if (currentState == GameState.Level2)
                SetScreens(hud: true, level2: true);
            else
                SetScreens(hud: true);

            Time.timeScale   = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            if (_fps != null) _fps.enabled = true;
        }
        else
        {
            // If the player pauses while a puzzle UI is open, close it first so it cannot
            // block clicks on the pause menu.
            if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleOpen)
                PuzzleManager.Instance.ClosePuzzle();

            // Remember previous state (Playing or Level2)
            _stateBeforePause = currentState;

            currentState = GameState.Paused;
            SetScreens(hud: true, pause: true);
            Time.timeScale   = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            if (_fps != null) _fps.enabled = false;
        }
    }
    public void TransitionToLevel2()
    {
        if (!enableLevel2)
        {
            TriggerWin();
            return;
        }

        currentState = GameState.Level2;
        SetScreens(hud: true, level2: true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (_fps != null) _fps.enabled = true;
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ForceReset();
            Debug.Log("[GameManager] TransitionToLevel2: ForceReset called on PuzzleManager");
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
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
        _optionsOpen = false;
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

    public void RegisterOptionsPanel(GameObject panel)
    {
        if (panel == null) return;
        optionsCanvas = panel;
    }

    public void OpenOptions()
    {
        if (_optionsOpen) return;
        if (!EnsureOptionsAssigned()) return;
        if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleOpen) return;

        _stateBeforeOptions = currentState;
        _optionsOpen = true;
        optionsCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_fps != null) _fps.enabled = false;
    }

    public void CloseOptions()
    {
        if (!EnsureOptionsAssigned()) return;
        if (!_optionsOpen && !optionsCanvas.activeSelf) return;

        optionsCanvas.SetActive(false);
        _optionsOpen = false;

        if (_stateBeforeOptions == GameState.Playing && currentState == GameState.Playing)
        {
            RestorePlayState();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (_fps != null) _fps.enabled = false;
        }
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
                    bool pause = false,    bool win = false, bool lose = false, bool level2 = false)
    {
        if (mainMenuCanvas) mainMenuCanvas.SetActive(mainMenu);
        if (hudCanvas)      hudCanvas.SetActive(hud);
        if (pauseCanvas)    pauseCanvas.SetActive(pause);
        if (winCanvas)      winCanvas.SetActive(win);
        if (loseCanvas)     loseCanvas.SetActive(lose);
        if (level2Canvas)   level2Canvas.SetActive(level2);
    }
}