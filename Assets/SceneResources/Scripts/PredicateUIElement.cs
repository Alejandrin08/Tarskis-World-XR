using UnityEngine;
using UnityEngine.UI;

public class PredicateUIElement : MonoBehaviour
{
    public string predicateName;
    public Image background;
    public Color successColor = new Color(76f / 255f, 175f / 255f, 80f / 255f, 51f / 255f);
    public Color failColor = new Color(255f / 255f, 107f / 255f, 107f / 255f, 51f / 255f);
    public Color inactiveColor = new Color(28f / 255f, 43f / 255f, 51f / 255f, 255f / 255f);

    private void Start()
    {
        if (background == null)
        {
            background = GetComponent<Image>();
        }

        SetStatus(false, false);
    }

    public void SetStatus(bool isTrue, bool isActive = true)
    {
        if (background == null) return;

        if (!isActive)
        {
            background.color = inactiveColor;
        }
        else
        {
            background.color = isTrue ? successColor : failColor;
        }
    }

    public void SetStatus(bool isTrue)
    {
        SetStatus(isTrue, true);
    }
}