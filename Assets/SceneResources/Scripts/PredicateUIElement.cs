using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PredicateUIElement : MonoBehaviour
{
    [Header("Configuration")]
    public string predicateName;
    
    [Header("Visual References")]
    public Image background;
    public TextMeshProUGUI textComponent; // Optional text component
    public Image checkmarkIcon; // Optional checkmark icon
    
    [Header("Colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public Color defaultColor = Color.gray;
    
    [Header("Animation")]
    public bool enableAnimation = true;
    public float animationDuration = 0.3f;
    
    [Header("Debug")]
    public bool enableDebugLogs = false;
    
    private bool currentStatus = false;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // Auto-find components if not assigned
        if (background == null)
        {
            background = GetComponent<Image>();
        }
        
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (checkmarkIcon == null)
        {
            // Look for a child with "checkmark" or "check" in the name
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.name.ToLower().Contains("check") || child.name.ToLower().Contains("mark"))
                {
                    checkmarkIcon = child.GetComponent<Image>();
                    break;
                }
            }
        }
        
        // Set initial state
        if (background != null)
        {
            background.color = defaultColor;
        }
        
        if (checkmarkIcon != null)
        {
            checkmarkIcon.gameObject.SetActive(false);
        }
        
        // Update text if available
        if (textComponent != null && !string.IsNullOrEmpty(predicateName))
        {
            textComponent.text = FormatPredicateName(predicateName);
        }
        
        isInitialized = true;
        
        if (enableDebugLogs)
        {
            Debug.Log($"PredicateUIElement '{predicateName}' initialized on {gameObject.name}");
        }
    }

    public void SetStatus(bool isTrue)
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        if (currentStatus != isTrue)
        {
            currentStatus = isTrue;
            
            if (enableDebugLogs)
            {
                Debug.Log($"Predicate '{predicateName}' status changed to: {isTrue}");
            }
            
            UpdateVisuals(isTrue);
        }
    }
    
    private void UpdateVisuals(bool isTrue)
    {
        Color targetColor = isTrue ? successColor : failColor;
        
        if (enableAnimation && Application.isPlaying)
        {
            // Use Unity's built-in animation system
            StartCoroutine(AnimateColorChange(targetColor));
        }
        else
        {
            // Immediate change
            if (background != null)
            {
                background.color = targetColor;
            }
        }
        
        // Update checkmark visibility
        if (checkmarkIcon != null)
        {
            checkmarkIcon.gameObject.SetActive(isTrue);
        }
        
        // Update text color for better contrast
        if (textComponent != null)
        {
            textComponent.color = GetContrastColor(targetColor);
        }
    }
    
    private System.Collections.IEnumerator AnimateColorChange(Color targetColor)
    {
        if (background == null) yield break;
        
        Color startColor = background.color;
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Use smooth step for better animation feel
            t = t * t * (3f - 2f * t);
            
            background.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        background.color = targetColor;
    }
    
    private Color GetContrastColor(Color backgroundColor)
    {
        // Calculate luminance to determine if we should use black or white text
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }
    
    private string FormatPredicateName(string name)
    {
        // Convert camelCase to readable format
        // Example: "PiramideCuboMismoColor" -> "Piramide Cubo Mismo Color"
        string result = "";
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                result += " ";
            }
            result += c;
        }
        return result;
    }
    
    // Public method to get current status
    public bool GetCurrentStatus()
    {
        return currentStatus;
    }
    
    // Method to manually refresh the display
    public void RefreshDisplay()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"Refreshing display for predicate '{predicateName}'");
        }
        UpdateVisuals(currentStatus);
    }
}