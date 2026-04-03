using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TextCutscene : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneCard
    {
        [TextArea] public string text;
        public float holdDuration = 3f;
    }

    public CutsceneCard[] cards;
    public TextMeshProUGUI displayText;
    public float fadeDuration = 1.2f;
    public string nextScene = "Intro_game";
    public AudioSource ambientAudio;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        foreach (CutsceneCard card in cards)
        {
            if (string.IsNullOrEmpty(card.text))
            {
                yield return new WaitForSeconds(card.holdDuration);
                continue;
            }

            displayText.text = card.text;
            yield return StartCoroutine(FadeText(0f, 1f));
            yield return new WaitForSeconds(card.holdDuration);
            yield return StartCoroutine(FadeText(1f, 0f));
            yield return new WaitForSeconds(0.4f);
        }

        if (ambientAudio != null)
            yield return StartCoroutine(FadeAudio());

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator FadeText(float from, float to)
    {
        float elapsed = 0f;
        Color c = displayText.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            displayText.color = c;
            yield return null;
        }
        c.a = to;
        displayText.color = c;
    }

    IEnumerator FadeAudio()
    {
        float elapsed = 0f;
        float startVolume = ambientAudio.volume;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            ambientAudio.volume = Mathf.Lerp(
                startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }
    }
}