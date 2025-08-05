using UnityEngine;
using System.Collections.Generic;
using TMPro;

public enum DifficultyLevel
{
    Facil = 1,
    Medio = 2,
    Dificil = 3
}

public class TarskiLogic : MonoBehaviour
{
    [Header("Referencias")]
    public List<GameObject> figures;
    public List<PredicateUIElement> uiPredicates;

    [Header("Configuración del Nivel")]
    public DifficultyLevel currentLevel = DifficultyLevel.Facil;

    [Header("Distancias")]
    public float minDistance = 0.1f;
    public float maxDistance = 1.5f;
    public float closeDistance = 0.8f;
    public float farDistance = 2.0f;

    [Header("Posiciones")]
    public float frontThreshold = 0.7f;
    public float sideThreshold = 0.3f;
    public float heightTolerance = 0.2f;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool enableDetailedDebug = false;
    public bool autoValidateSetup = true;

    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent OnLevelCompleted;

    private Dictionary<string, System.Func<bool>> allPredicates;
    private Dictionary<DifficultyLevel, List<string>> levelPredicates;
    private List<string> activePredicates;
    private int lastCompletedCount = -1;

    void Start()
    {
        if (autoValidateSetup)
        {
            ValidateSetup();
        }
        
        InitializeAllPredicates();
        InitializeLevelPredicates();
        SetActivePredicatesForLevel(currentLevel);
        UpdateAllPredicatesUI();
        
        // Force an initial update
        Invoke("DelayedInitialUpdate", 0.5f);
    }
    
    void DelayedInitialUpdate()
    {
        if (enableDebugLogs)
        {
            Debug.Log("=== TARSKI LOGIC INITIAL VALIDATION ===");
        }
        OnFigureMoved();
    }
    
    private void ValidateSetup()
    {
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();
        
        // Validate figures
        if (figures == null || figures.Count == 0)
        {
            errors.Add("No figures assigned!");
        }
        else
        {
            for (int i = 0; i < figures.Count; i++)
            {
                if (figures[i] == null)
                {
                    errors.Add($"Figure at index {i} is null!");
                }
                else
                {
                    // Check if figure has FigureTracker
                    FigureTracker tracker = figures[i].GetComponent<FigureTracker>();
                    if (tracker == null)
                    {
                        warnings.Add($"Figure '{figures[i].name}' doesn't have FigureTracker component!");
                    }
                    
                    // Check if figure has Renderer for color detection
                    Renderer renderer = figures[i].GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        warnings.Add($"Figure '{figures[i].name}' doesn't have Renderer component for color detection!");
                    }
                }
            }
        }
        
        // Validate UI predicates
        if (uiPredicates == null || uiPredicates.Count == 0)
        {
            errors.Add("No UI predicates assigned!");
        }
        else
        {
            for (int i = 0; i < uiPredicates.Count; i++)
            {
                if (uiPredicates[i] == null)
                {
                    errors.Add($"UI Predicate at index {i} is null!");
                }
                else if (string.IsNullOrEmpty(uiPredicates[i].predicateName))
                {
                    warnings.Add($"UI Predicate '{uiPredicates[i].gameObject.name}' has empty predicate name!");
                }
            }
        }
        
        // Log results
        if (errors.Count > 0)
        {
            Debug.LogError("TarskiLogic Setup Errors:\n" + string.Join("\n", errors));
        }
        if (warnings.Count > 0)
        {
            Debug.LogWarning("TarskiLogic Setup Warnings:\n" + string.Join("\n", warnings));
        }
        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("TarskiLogic setup validation passed!");
        }
    }

    private void InitializeAllPredicates()
    {
        allPredicates = new Dictionary<string, System.Func<bool>>()
        {
            { "PiramideCuboMismoColor", () => MismoColor(0, 1) },
            { "CuboPrismaMismoColor", () => MismoColor(1, 2) },
            { "PiramidePrismaMismoColor", () => MismoColor(0, 2) },

            { "PiramideCuboCerca", () => EstanCerca(0, 1) },
            { "CuboPrismaCerca", () => EstanCerca(1, 2) },
            { "PiramidePrismaLejos", () => EstanLejos(0, 2) },

            { "CuboAlFrentePiramide", () => AlFrenteDe(0, 1) },
            { "PrismaAlFrenteCubo", () => AlFrenteDe(1, 2) },
            { "PiramideEntreOtros", () => Entre(0, 1, 2) },
            { "CuboEntreOtros", () => Entre(1, 0, 2) },

            { "CuboAlLadoPiramide", () => EstanAlLado(0, 1) },
            { "PrismaAlLadoCubo", () => EstanAlLado(1, 2) },

            { "TresFigurasAlineadas", () => EstanAlineadas(0, 1, 2) },

            { "FormacionTriangular", () => FormanTriangulo(0, 1, 2) },
            { "PiramideCentro", () => EstaEnCentro(0, 1, 2) },

            { "PrismaCilindroMismoColor", () => MismoColor(2, 3) },
            { "PiramidaCilindroLejos", () => EstanLejos(0, 3) },
            { "PiramideEntreCubos", () => Entre(0, 1, 4) },
            { "CincoFigurasAlineadas", () => AlineacionCompleja() },
            { "FigurasFormandoCruz", () => EstanPerpendiculares(0, 4, 2) },
        };
    }

    private void InitializeLevelPredicates()
    {
        levelPredicates = new Dictionary<DifficultyLevel, List<string>>()
    {
        {
            DifficultyLevel.Facil,
            new List<string>()
            {
                "PiramideCuboMismoColor",    
                "PiramideCuboCerca",           
                "CuboPrismaCerca"            
            }
        },
        {
            DifficultyLevel.Medio,
            new List<string>()
            {
                "CuboAlFrentePiramide",      
                "PrismaCilindroMismoColor",  
                "CuboAlLadoPrisma",          
                "PiramidaCilindroLejos"      
            }
        },
        {
            DifficultyLevel.Dificil,
            new List<string>()
            {
                "PiramideEntreCubos",        
                "FormacionTriangular",       
                "FigurasFormandoCruz",       
                "CincoFigurasAlineadas"      
            }
        }
    };
    }

    public void SetActivePredicatesForLevel(DifficultyLevel level)
    {
        currentLevel = level;
        activePredicates = levelPredicates[level];

        if (enableDebugLogs)
        {
            Debug.Log($"Setting active predicates for level {level}: {string.Join(", ", activePredicates)}");
        }

        foreach (var uiElement in uiPredicates)
        {
            bool shouldShow = activePredicates.Contains(uiElement.predicateName);
            uiElement.gameObject.SetActive(shouldShow);
            
            if (enableDetailedDebug)
            {
                Debug.Log($"UI Element '{uiElement.predicateName}' - Show: {shouldShow}");
            }
        }

        UpdateAllPredicatesUI();
    }

    public void OnFigureMoved()
    {
        if (enableDetailedDebug)
        {
            Debug.Log("=== FIGURE MOVED - UPDATING PREDICATES ===");
        }
        
        UpdateAllPredicatesUI();
        CheckLevelCompletion();
    }

    private void UpdateAllPredicatesUI()
    {
        int updatedCount = 0;
        
        foreach (var uiElement in uiPredicates)
        {
            if (activePredicates.Contains(uiElement.predicateName) &&
                allPredicates.ContainsKey(uiElement.predicateName))
            {
                bool status = allPredicates[uiElement.predicateName].Invoke();
                uiElement.SetStatus(status);
                updatedCount++;
                
                if (enableDetailedDebug)
                {
                    Debug.Log($"Predicate '{uiElement.predicateName}': {status}");
                }
            }
        }
        
        if (enableDebugLogs && updatedCount > 0)
        {
            Debug.Log($"Updated {updatedCount} predicate UI elements");
        }
    }

    private void CheckLevelCompletion()
    {
        int completedPredicates = 0;
        int totalPredicates = activePredicates.Count;

        foreach (var predicateName in activePredicates)
        {
            if (allPredicates.ContainsKey(predicateName) &&
                allPredicates[predicateName].Invoke())
            {
                completedPredicates++;
            }
        }

        // Only log if completion count changed
        if (completedPredicates != lastCompletedCount)
        {
            lastCompletedCount = completedPredicates;
            
            if (enableDebugLogs)
            {
                Debug.Log($"Progreso del Nivel {currentLevel}: {completedPredicates}/{totalPredicates} predicados cumplidos");
                
                if (enableDetailedDebug)
                {
                    // Show which predicates are completed
                    foreach (var predicateName in activePredicates)
                    {
                        bool completed = allPredicates.ContainsKey(predicateName) && allPredicates[predicateName].Invoke();
                        Debug.Log($"  - {predicateName}: {(completed ? "✓" : "✗")}");
                    }
                }
            }
        }

        if (completedPredicates == totalPredicates && totalPredicates > 0)
        {
            Debug.Log($"¡NIVEL {currentLevel} COMPLETADO! ({completedPredicates}/{totalPredicates} predicados cumplidos)");
            OnLevelCompleted?.Invoke();
        }
    }

    private bool EstanCerca(int indexA, int indexB)
    {
        if (!ValidateIndices(indexA, indexB)) return false;
        
        float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        bool result = distance > minDistance && distance < closeDistance;
        
        if (enableDetailedDebug)
        {
            Debug.Log($"EstanCerca({indexA},{indexB}): distance={distance:F3}, result={result}");
        }
        
        return result;
    }

    private bool EstanLejos(int indexA, int indexB)
    {
        if (!ValidateIndices(indexA, indexB)) return false;
        
        float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        bool result = distance > farDistance;
        
        if (enableDetailedDebug)
        {
            Debug.Log($"EstanLejos({indexA},{indexB}): distance={distance:F3}, result={result}");
        }
        
        return result;
    }

    private bool MismoColor(int indexA, int indexB)
    {
        if (!ValidateIndices(indexA, indexB)) return false;
        
        Renderer rendererA = figures[indexA].GetComponent<Renderer>();
        Renderer rendererB = figures[indexB].GetComponent<Renderer>();
        
        if (rendererA == null || rendererB == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"MismoColor({indexA},{indexB}): Missing Renderer component");
            }
            return false;
        }
        
        bool result = rendererA.material.color == rendererB.material.color;
        
        if (enableDetailedDebug)
        {
            Debug.Log($"MismoColor({indexA},{indexB}): colorA={rendererA.material.color}, colorB={rendererB.material.color}, result={result}");
        }
        
        return result;
    }
    
    private bool ValidateIndices(params int[] indices)
    {
        foreach (int index in indices)
        {
            if (index < 0 || index >= figures.Count || figures[index] == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogError($"Invalid figure index: {index} (figures.Count: {figures.Count})");
                }
                return false;
            }
        }
        return true;
    }

    private bool AlFrenteDe(int indexA, int indexB)
    {
        if (!ValidateIndices(indexA, indexB)) return false;
        if (!EstanCerca(indexA, indexB)) return false;
        Vector3 dir = figures[indexB].transform.position - figures[indexA].transform.position;
        return Vector3.Dot(figures[indexA].transform.forward, dir.normalized) > frontThreshold;
    }

    private bool EstanAlLado(int indexA, int indexB)
    {
        if (!ValidateIndices(indexA, indexB)) return false;
        
        Vector3 posA = figures[indexA].transform.position;
        Vector3 posB = figures[indexB].transform.position;

        Vector3 horizontalDiff = new Vector3(posB.x - posA.x, 0, posB.z - posA.z);
        float horizontalDistance = horizontalDiff.magnitude;

        float heightDiff = Mathf.Abs(posA.y - posB.y);

        return horizontalDistance > minDistance && horizontalDistance < closeDistance &&
               heightDiff < heightTolerance;
    }

    private bool Entre(int indexA, int indexB, int indexC)
    {
        if (!ValidateIndices(indexA, indexB, indexC)) return false;
        
        Vector3 a = figures[indexA].transform.position;
        Vector3 b = figures[indexB].transform.position;
        Vector3 c = figures[indexC].transform.position;

        Vector3 bc = c - b;
        Vector3 ba = a - b;
        Vector3 ca = a - c;
        Vector3 cb = b - c;

        float dotProduct1 = Vector3.Dot(ba.normalized, bc.normalized);
        float dotProduct2 = Vector3.Dot(ca.normalized, cb.normalized);

        return dotProduct1 > 0 && dotProduct2 > 0;
    }

    private bool EstanAlineadas(int indexA, int indexB, int indexC)
    {
        if (!ValidateIndices(indexA, indexB, indexC)) return false;
        
        Vector3 a = figures[indexA].transform.position;
        Vector3 b = figures[indexB].transform.position;
        Vector3 c = figures[indexC].transform.position;

        Vector3 ab = b - a;
        Vector3 ac = c - a;

        float crossMagnitude = Vector3.Cross(ab, ac).magnitude;
        float threshold = 0.1f;

        return crossMagnitude < threshold;
    }

    private bool FormanTriangulo(int indexA, int indexB, int indexC)
    {
        if (!ValidateIndices(indexA, indexB, indexC)) return false;
        
        float distAB = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        float distBC = Vector3.Distance(figures[indexB].transform.position, figures[indexC].transform.position);
        float distCA = Vector3.Distance(figures[indexC].transform.position, figures[indexA].transform.position);

        return (distAB + distBC > distCA) &&
               (distBC + distCA > distAB) &&
               (distCA + distAB > distBC) &&
               distAB > minDistance && distBC > minDistance && distCA > minDistance &&
               distAB < maxDistance && distBC < maxDistance && distCA < maxDistance;
    }

    private bool EstaEnCentro(int indexCentro, int indexA, int indexB)
    {
        if (!ValidateIndices(indexCentro, indexA, indexB)) return false;
        
        Vector3 centro = figures[indexCentro].transform.position;
        Vector3 a = figures[indexA].transform.position;
        Vector3 b = figures[indexB].transform.position;

        Vector3 puntoMedio = (a + b) / 2f;
        float distanciaAlCentro = Vector3.Distance(centro, puntoMedio);

        return distanciaAlCentro < closeDistance * 0.5f;
    }

    private bool EstanPerpendiculares(int indexA, int indexB, int indexC)
    {
        if (!ValidateIndices(indexA, indexB, indexC)) return false;
        
        Vector3 posA = figures[indexA].transform.position;
        Vector3 posB = figures[indexB].transform.position;
        Vector3 posC = figures[indexC].transform.position;

        if (!EstanCerca(indexA, indexB) || !EstanCerca(indexB, indexC))
            return false;

        Vector3 ba = (posA - posB).normalized;
        Vector3 bc = (posC - posB).normalized;

        float dotProduct = Vector3.Dot(ba, bc);
        return Mathf.Abs(dotProduct) < 0.3f;
    }

    private bool AlineacionCompleja()
    {
        if (figures.Count < 5) return false;
        return EstanAlineadas(0, 1, 4) && EstanAlineadas(1, 2, 3);
    }


    public void SetLevel(int level)
    {
        if (level >= 1 && level <= 3)
        {
            SetActivePredicatesForLevel((DifficultyLevel)level);
        }
    }

    public float GetCompletionPercentage()
    {
        if (activePredicates.Count == 0) return 0f;

        int completed = 0;
        foreach (var predicateName in activePredicates)
        {
            if (allPredicates.ContainsKey(predicateName) &&
                allPredicates[predicateName].Invoke())
            {
                completed++;
            }
        }

        return (float)completed / activePredicates.Count;
    }

    public bool IsLevelCompleted()
    {
        return GetCompletionPercentage() >= 1.0f;
    }
    
    // Manual debug method you can call from Unity Inspector or console
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== TARSKI LOGIC DEBUG STATE ===");
        Debug.Log($"Current Level: {currentLevel}");
        Debug.Log($"Figures Count: {figures.Count}");
        Debug.Log($"Active Predicates: {string.Join(", ", activePredicates)}");
        Debug.Log($"Completion: {GetCompletionPercentage():P0}");
        
        for (int i = 0; i < figures.Count && i < 3; i++)
        {
            if (figures[i] != null)
            {
                Debug.Log($"Figure {i} ({figures[i].name}): Position = {figures[i].transform.position}");
            }
        }
        
        OnFigureMoved(); // Force update
    }
}