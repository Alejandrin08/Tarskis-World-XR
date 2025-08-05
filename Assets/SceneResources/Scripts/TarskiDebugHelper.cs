using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TarskiSetupReport
{
    public bool hasErrors;
    public bool hasWarnings;
    public List<string> errors = new List<string>();
    public List<string> warnings = new List<string>();
    public List<string> suggestions = new List<string>();
}

public class TarskiDebugHelper : MonoBehaviour
{
    [Header("Auto-Fix Options")]
    public bool autoAddMissingComponents = true;
    public bool autoAssignReferences = true;
    
    [Header("Debug Output")]
    public bool verboseLogging = true;
    
    [System.NonSerialized]
    public TarskiSetupReport lastReport;

    [ContextMenu("Run Full Diagnosis")]
    public void RunFullDiagnosis()
    {
        lastReport = new TarskiSetupReport();
        
        Debug.Log("=== TARSKI LOGIC DIAGNOSIS ===");
        
        TarskiLogic tarskiLogic = FindObjectOfType<TarskiLogic>();
        if (tarskiLogic == null)
        {
            lastReport.errors.Add("No TarskiLogic component found in scene!");
            Debug.LogError("CRITICAL: No TarskiLogic component found in scene!");
            return;
        }
        
        DiagnoseFigures(tarskiLogic);
        DiagnoseUIPredicates(tarskiLogic);
        DiagnoseReferences(tarskiLogic);
        
        // Print summary
        PrintDiagnosisReport();
        
        // Auto-fix if enabled
        if (autoAddMissingComponents || autoAssignReferences)
        {
            Debug.Log("=== ATTEMPTING AUTO-FIXES ===");
            ApplyAutoFixes(tarskiLogic);
        }
    }
    
    private void DiagnoseFigures(TarskiLogic tarskiLogic)
    {
        Debug.Log("--- Diagnosing Figures ---");
        
        if (tarskiLogic.figures == null || tarskiLogic.figures.Count == 0)
        {
            lastReport.errors.Add("No figures assigned to TarskiLogic!");
            
            // Try to auto-find figures
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            List<GameObject> potentialFigures = new List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("piramide") || 
                    obj.name.ToLower().Contains("cubo") || 
                    obj.name.ToLower().Contains("prisma") ||
                    obj.name.ToLower().Contains("cilindro") ||
                    obj.GetComponent<OVRGrabbable>() != null)
                {
                    potentialFigures.Add(obj);
                }
            }
            
            if (potentialFigures.Count > 0)
            {
                lastReport.suggestions.Add($"Found {potentialFigures.Count} potential figures that could be assigned");
                if (verboseLogging)
                {
                    foreach (GameObject fig in potentialFigures)
                    {
                        Debug.Log($"  Potential figure: {fig.name}");
                    }
                }
            }
            return;
        }
        
        for (int i = 0; i < tarskiLogic.figures.Count; i++)
        {
            GameObject figure = tarskiLogic.figures[i];
            
            if (figure == null)
            {
                lastReport.errors.Add($"Figure at index {i} is null!");
                continue;
            }
            
            // Check FigureTracker
            FigureTracker tracker = figure.GetComponent<FigureTracker>();
            if (tracker == null)
            {
                lastReport.warnings.Add($"Figure '{figure.name}' missing FigureTracker component");
            }
            
            // Check OVRGrabbable
            OVRGrabbable grabbable = figure.GetComponent<OVRGrabbable>();
            if (grabbable == null)
            {
                lastReport.warnings.Add($"Figure '{figure.name}' missing OVRGrabbable component");
            }
            
            // Check Renderer
            Renderer renderer = figure.GetComponent<Renderer>();
            if (renderer == null)
            {
                lastReport.warnings.Add($"Figure '{figure.name}' missing Renderer component");
            }
            
            // Check Collider
            Collider collider = figure.GetComponent<Collider>();
            if (collider == null)
            {
                lastReport.warnings.Add($"Figure '{figure.name}' missing Collider component");
            }
            
            if (verboseLogging)
            {
                Debug.Log($"Figure {i} ({figure.name}): " +
                         $"Tracker={tracker != null}, " +
                         $"Grabbable={grabbable != null}, " +
                         $"Renderer={renderer != null}, " +
                         $"Collider={collider != null}");
            }
        }
    }
    
    private void DiagnoseUIPredicates(TarskiLogic tarskiLogic)
    {
        Debug.Log("--- Diagnosing UI Predicates ---");
        
        if (tarskiLogic.uiPredicates == null || tarskiLogic.uiPredicates.Count == 0)
        {
            lastReport.errors.Add("No UI predicates assigned to TarskiLogic!");
            
            // Try to find UI predicates in scene
            PredicateUIElement[] foundPredicates = FindObjectsOfType<PredicateUIElement>();
            if (foundPredicates.Length > 0)
            {
                lastReport.suggestions.Add($"Found {foundPredicates.Length} PredicateUIElement components in scene that could be assigned");
            }
            return;
        }
        
        List<string> expectedPredicates = new List<string>()
        {
            "PiramideCuboMismoColor", "PiramideCuboCerca", "CuboPrismaCerca"
        };
        
        foreach (string expected in expectedPredicates)
        {
            bool found = false;
            foreach (PredicateUIElement uiElement in tarskiLogic.uiPredicates)
            {
                if (uiElement != null && uiElement.predicateName == expected)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                lastReport.warnings.Add($"Expected predicate '{expected}' not found in UI predicates list");
            }
        }
        
        for (int i = 0; i < tarskiLogic.uiPredicates.Count; i++)
        {
            PredicateUIElement uiElement = tarskiLogic.uiPredicates[i];
            
            if (uiElement == null)
            {
                lastReport.errors.Add($"UI Predicate at index {i} is null!");
                continue;
            }
            
            if (string.IsNullOrEmpty(uiElement.predicateName))
            {
                lastReport.warnings.Add($"UI Predicate '{uiElement.gameObject.name}' has empty predicate name!");
            }
            
            if (uiElement.background == null)
            {
                lastReport.warnings.Add($"UI Predicate '{uiElement.predicateName}' has no background Image assigned!");
            }
            
            if (verboseLogging)
            {
                Debug.Log($"UI Predicate {i}: '{uiElement.predicateName}' on {uiElement.gameObject.name}");
            }
        }
    }
    
    private void DiagnoseReferences(TarskiLogic tarskiLogic)
    {
        Debug.Log("--- Diagnosing References ---");
        
        // Check if TarskiLogic can find all required components
        if (tarskiLogic.figures != null && tarskiLogic.figures.Count >= 3)
        {
            // Test a simple predicate call
            try
            {
                bool testResult = tarskiLogic.GetCompletionPercentage() >= 0f;
                Debug.Log($"Basic predicate test passed: {testResult}");
            }
            catch (System.Exception e)
            {
                lastReport.errors.Add($"Error calling predicates: {e.Message}");
            }
        }
    }
    
    private void ApplyAutoFixes(TarskiLogic tarskiLogic)
    {
        int fixesApplied = 0;
        
        // Auto-add missing FigureTracker components
        if (autoAddMissingComponents && tarskiLogic.figures != null)
        {
            foreach (GameObject figure in tarskiLogic.figures)
            {
                if (figure != null)
                {
                    if (figure.GetComponent<FigureTracker>() == null)
                    {
                        figure.AddComponent<FigureTracker>();
                        Debug.Log($"Added FigureTracker to {figure.name}");
                        fixesApplied++;
                    }
                }
            }
        }
        
        // Auto-assign UI predicates if none are assigned
        if (autoAssignReferences && (tarskiLogic.uiPredicates == null || tarskiLogic.uiPredicates.Count == 0))
        {
            PredicateUIElement[] foundPredicates = FindObjectsOfType<PredicateUIElement>();
            if (foundPredicates.Length > 0)
            {
                tarskiLogic.uiPredicates = new List<PredicateUIElement>(foundPredicates);
                Debug.Log($"Auto-assigned {foundPredicates.Length} UI predicates");
                fixesApplied++;
            }
        }
        
        // Auto-assign figures if none are assigned
        if (autoAssignReferences && (tarskiLogic.figures == null || tarskiLogic.figures.Count == 0))
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            List<GameObject> foundFigures = new List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.GetComponent<OVRGrabbable>() != null && 
                    obj.GetComponent<Renderer>() != null)
                {
                    foundFigures.Add(obj);
                }
            }
            
            if (foundFigures.Count > 0)
            {
                tarskiLogic.figures = foundFigures;
                Debug.Log($"Auto-assigned {foundFigures.Count} figures");
                fixesApplied++;
            }
        }
        
        Debug.Log($"Applied {fixesApplied} automatic fixes");
        
        if (fixesApplied > 0)
        {
            Debug.Log("Re-running diagnosis after fixes...");
            RunFullDiagnosis();
        }
    }
    
    private void PrintDiagnosisReport()
    {
        Debug.Log("=== DIAGNOSIS REPORT ===");
        
        if (lastReport.errors.Count > 0)
        {
            Debug.LogError($"ERRORS FOUND ({lastReport.errors.Count}):");
            foreach (string error in lastReport.errors)
            {
                Debug.LogError($"  ❌ {error}");
            }
            lastReport.hasErrors = true;
        }
        
        if (lastReport.warnings.Count > 0)
        {
            Debug.LogWarning($"WARNINGS ({lastReport.warnings.Count}):");
            foreach (string warning in lastReport.warnings)
            {
                Debug.LogWarning($"  ⚠️ {warning}");
            }
            lastReport.hasWarnings = true;
        }
        
        if (lastReport.suggestions.Count > 0)
        {
            Debug.Log($"SUGGESTIONS ({lastReport.suggestions.Count}):");
            foreach (string suggestion in lastReport.suggestions)
            {
                Debug.Log($"  💡 {suggestion}");
            }
        }
        
        if (!lastReport.hasErrors && !lastReport.hasWarnings)
        {
            Debug.Log("✅ All checks passed! TarskiLogic should be working correctly.");
        }
    }
    
    [ContextMenu("Force Update TarskiLogic")]
    public void ForceUpdateTarskiLogic()
    {
        TarskiLogic tarskiLogic = FindObjectOfType<TarskiLogic>();
        if (tarskiLogic != null)
        {
            Debug.Log("Forcing TarskiLogic update...");
            tarskiLogic.OnFigureMoved();
        }
        else
        {
            Debug.LogError("No TarskiLogic found!");
        }
    }
    
    [ContextMenu("Test Predicate Functions")]
    public void TestPredicateFunctions()
    {
        TarskiLogic tarskiLogic = FindObjectOfType<TarskiLogic>();
        if (tarskiLogic == null)
        {
            Debug.LogError("No TarskiLogic found!");
            return;
        }
        
        Debug.Log("=== TESTING PREDICATE FUNCTIONS ===");
        Debug.Log($"Completion Percentage: {tarskiLogic.GetCompletionPercentage():P1}");
        Debug.Log($"Level Completed: {tarskiLogic.IsLevelCompleted()}");
        
        // Force a complete update
        tarskiLogic.DebugCurrentState();
    }
}