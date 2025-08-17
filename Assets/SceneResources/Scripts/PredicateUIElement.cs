using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PredicateUIElement : MonoBehaviour
{
    [Header("Configuración General")]
    public string levelIdentifier; 
    public Image background;

    [Header("Barra de Progreso")]
    public Slider progressBar;
    public Image progressFill;

    [Header("Texto de Progreso")]
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI levelNameText;

    [Header("Colores")]
    public Color successColor = new Color(76f / 255f, 175f / 255f, 80f / 255f, 51f / 255f);
    public Color failColor = new Color(255f / 255f, 107f / 255f, 107f / 255f, 51f / 255f);
    public Color inactiveColor = new Color(28f / 255f, 43f / 255f, 51f / 255f, 255f / 255f);
    public Color progressColor = new Color(33f / 255f, 150f / 255f, 243f / 255f, 255f / 255f);
    public Color completedColor = new Color(76f / 255f, 175f / 255f, 80f / 255f, 255f / 255f);

    private void Start()
    {
        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        SetStatus(false, false);
    }

    public void SetStatus(bool isTrue, bool isActive = true)
    {
        if (background == null) return;

        if (!isActive)
        {
            background.color = inactiveColor;
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
            }
        }
        else
        {
            background.color = isTrue ? successColor : failColor;
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
            }
        }
    }

    public void SetStatus(bool isTrue)
    {
        SetStatus(isTrue, true);
    }

    public void UpdateProgress(float progress, int completedCount, int totalCount, string levelName = "")
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
            progressBar.gameObject.SetActive(true);
        }

        if (progressFill != null)
        {
            if (progress >= 1f)
            {
                progressFill.color = completedColor;
            }
            else
            {
                progressFill.color = progressColor;
            }
        }

        if (progressText != null)
        {
            progressText.text = $"{completedCount}/{totalCount} Completados";
        }

        if (levelNameText != null && !string.IsNullOrEmpty(levelName))
        {
            levelNameText.text = levelName;
        }

        if (background != null)
        {
            if (progress >= 1f)
            {
                background.color = successColor;
            }
            else if (progress > 0f)
            {
                background.color = Color.Lerp(failColor, successColor, progress);
            }
            else
            {
                background.color = failColor;
            }
        }
    }

    public void SetLevelCompleted()
    {
        if (background != null)
        {
            background.color = successColor;
        }

        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        if (progressFill != null)
        {
            progressFill.color = completedColor;
        }
    }

    public void ResetProgress()
    {
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        if (progressText != null)
        {
            progressText.text = "0/0 Completados";
        }

        SetStatus(false, false);
    }
}