using UnityEngine;
using UnityEngine.UI;

public class PredicateUIElement : MonoBehaviour
{
    public string predicateName;
    public Image background;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private void Start()
    {
        // Verificar que tenemos una referencia válida al componente Image
        if (background == null)
        {
            background = GetComponent<Image>();
            if (background == null)
            {
                Debug.LogError($"PredicateUIElement '{predicateName}': No se encontró componente Image!");
                return;
            }
        }
        
        // Establecer color inicial
        SetStatus(false);
        Debug.Log($"PredicateUIElement '{predicateName}' inicializado correctamente");
    }

    public void SetStatus(bool isTrue)
    {
        if (background == null)
        {
            Debug.LogError($"PredicateUIElement '{predicateName}': background es null!");
            return;
        }
        
        Color newColor = isTrue ? successColor : failColor;
        background.color = newColor;
        
        Debug.Log($"PredicateUIElement '{predicateName}': Cambiando color a {(isTrue ? "SUCCESS" : "FAIL")} ({newColor})");
    }
}