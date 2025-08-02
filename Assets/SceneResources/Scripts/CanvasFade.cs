using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CanvasFade : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup canvasGroup; 
    public float displayTime = 10f; 

    [Header("Animaciones")]
    public float fadeInDuration = 1f; 
    public float fadeOutDuration = 1f; 

    private void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("No se encontró CanvasGroup en el GameObject.");
                return;
            }
        }

        StartCoroutine(FadeInOut());
    }

    private IEnumerator FadeInOut()
    {
        yield return StartCoroutine(FadeCanvas(canvasGroup, 0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(displayTime);

        yield return StartCoroutine(FadeCanvas(canvasGroup, 1f, 0f, fadeOutDuration));

        gameObject.SetActive(false);
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