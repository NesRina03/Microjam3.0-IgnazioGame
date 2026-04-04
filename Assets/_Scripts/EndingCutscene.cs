using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingCutscene : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public RawImage videoScreen;
    public Image blackOverlay;
    public TextMeshProUGUI overlayText;
    public AudioSource sfx;

    [Header("Settings")]
    public string nextScene = "MainMenu";
    public float fadeDuration = 1.5f;

    [Header("Text overlay")]
    [TextArea] public string endingText = "It remembers you were here.";
    public float textStartTime = 3f;
    public float textHoldDuration = 4f;

    void Start()
    {
        // Start fully black
        SetOverlayAlpha(1f);
        SetVideoAlpha(0f);
        SetTextAlpha(0f);

        StartCoroutine(PlayEnding());
    }

    IEnumerator PlayEnding()
    {
        // Hold black for 1 second
        yield return new WaitForSeconds(1f);

        // Fade in video
        videoPlayer.Play();
        yield return StartCoroutine(FadeOverlay(1f, 0f));
        yield return StartCoroutine(FadeVideo(0f, 1f));

        // Play sound effect at the right moment
        if (sfx != null)
            sfx.Play();

        // Start text overlay coroutine in parallel
        StartCoroutine(ShowText());

        // Wait for video to finish
        yield return new WaitUntil(() =>
            !videoPlayer.isPlaying);

        // Camera shake
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(ShakeCamera(1.5f, 0.08f));

        yield return new WaitForSeconds(1.5f);

        // Fade everything to black
        yield return StartCoroutine(FadeVideo(1f, 0f));
        yield return StartCoroutine(FadeOverlay(0f, 1f));

        yield return new WaitForSeconds(1.5f);

        // Load next scene
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator ShowText()
    {
        yield return new WaitForSeconds(textStartTime);
        overlayText.text = endingText;
        yield return StartCoroutine(FadeTextIn());
        yield return new WaitForSeconds(textHoldDuration);
        yield return StartCoroutine(FadeTextOut());
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 original = Camera.main.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition =
                new Vector3(original.x + x,
                            original.y + y,
                            original.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = original;
    }

    IEnumerator FadeOverlay(float from, float to)
    {
        float elapsed = 0f;
        Color c = blackOverlay.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            blackOverlay.color = c;
            yield return null;
        }
        c.a = to;
        blackOverlay.color = c;
    }

    IEnumerator FadeVideo(float from, float to)
    {
        float elapsed = 0f;
        Color c = videoScreen.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            videoScreen.color = c;
            yield return null;
        }
    }

    IEnumerator FadeTextIn()
    {
        float elapsed = 0f;
        Color c = overlayText.color;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed);
            overlayText.color = c;
            yield return null;
        }
    }

    IEnumerator FadeTextOut()
    {
        float elapsed = 0f;
        Color c = overlayText.color;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed);
            overlayText.color = c;
            yield return null;
        }
    }

    void SetOverlayAlpha(float a)
    {
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }

    void SetVideoAlpha(float a)
    {
        Color c = videoScreen.color;
        c.a = a;
        videoScreen.color = c;
    }

    void SetTextAlpha(float a)
    {
        Color c = overlayText.color;
        c.a = a;
        overlayText.color = c;
    }
}