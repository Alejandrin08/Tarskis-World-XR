using UnityEngine;
using UnityEngine.UI;

public class PredicateUIElement : MonoBehaviour
{
    public string predicateName;
    public Image background;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    public void SetStatus(bool isTrue)
    {
        background.color = isTrue ? successColor : failColor;
    }
}