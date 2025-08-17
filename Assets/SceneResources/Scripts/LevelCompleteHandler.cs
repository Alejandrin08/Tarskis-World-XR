using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCompleteDisplay : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup completionCanvas;
    public float displayDuration = 5f;

    [Header("Animaciones")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    [Header("Configuración")]
    public string menuSceneName = "Menu";

    [Header("Interfaces")]
    public GameObject menuInterface;

    public void OnLevelCompleted()
    {
        StartCoroutine(ShowCompletionMessage());
    }

    private IEnumerator ShowCompletionMessage()
    {
        if (menuInterface != null)
        {
            menuInterface.SetActive(false);
        }
        completionCanvas.gameObject.SetActive(true);
        completionCanvas.alpha = 0f;

        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(FadeCanvas(completionCanvas, 0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeCanvas(completionCanvas, 1f, 0f, fadeOutDuration));

        SceneManager.LoadScene(menuSceneName);
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}