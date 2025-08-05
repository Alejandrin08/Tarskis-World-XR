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

    [Header("Configuraci�n del Nivel")]
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

    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent OnLevelCompleted;

    private Dictionary<string, System.Func<bool>> allPredicates;
    private Dictionary<DifficultyLevel, List<string>> levelPredicates;
    private List<string> activePredicates;

    void Start()
    {
        Debug.Log("TarskiLogic: Iniciando...");
        
        // Verificar que tenemos figuras asignadas
        if (figures == null || figures.Count == 0)
        {
            Debug.LogError("TarskiLogic: No hay figuras asignadas!");
            return;
        }
        
        // Verificar que tenemos predicados UI asignados
        if (uiPredicates == null || uiPredicates.Count == 0)
        {
            Debug.LogError("TarskiLogic: No hay predicados UI asignados!");
            return;
        }
        
        Debug.Log($"TarskiLogic: {figures.Count} figuras y {uiPredicates.Count} predicados UI encontrados");
        
        InitializeAllPredicates();
        InitializeLevelPredicates();
        SetActivePredicatesForLevel(currentLevel);
        UpdateAllPredicatesUI();
        
        Debug.Log($"TarskiLogic: Inicialización completa para nivel {currentLevel}");
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

        foreach (var uiElement in uiPredicates)
        {
            bool shouldShow = activePredicates.Contains(uiElement.predicateName);
            uiElement.gameObject.SetActive(shouldShow);
        }

        UpdateAllPredicatesUI();
    }

    public void OnFigureMoved()
    {
        Debug.Log("TarskiLogic: OnFigureMoved() llamado - Actualizando predicados...");
        UpdateAllPredicatesUI();
        CheckLevelCompletion();
    }

    private void UpdateAllPredicatesUI()
    {
        Debug.Log($"TarskiLogic: Actualizando UI para {activePredicates.Count} predicados activos del nivel {currentLevel}");
        
        foreach (var uiElement in uiPredicates)
        {
            if (activePredicates.Contains(uiElement.predicateName) &&
                allPredicates.ContainsKey(uiElement.predicateName))
            {
                bool status = allPredicates[uiElement.predicateName].Invoke();
                uiElement.SetStatus(status);
                Debug.Log($"Predicado '{uiElement.predicateName}': {status}");
            }
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

        if (completedPredicates == totalPredicates && totalPredicates > 0)
        {
            Debug.Log($"�Nivel {currentLevel} completado! ({completedPredicates}/{totalPredicates} predicados cumplidos)");
            OnLevelCompleted?.Invoke();
        }
        else
        {
            Debug.Log($"Progreso: {completedPredicates}/{totalPredicates} predicados cumplidos");
        }
    }

    private bool EstanCerca(int indexA, int indexB)
    {
        float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        return distance > minDistance && distance < closeDistance;
    }

    private bool EstanLejos(int indexA, int indexB)
    {
        float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        return distance > farDistance;
    }

    private bool MismoColor(int indexA, int indexB)
    {
        return figures[indexA].GetComponent<Renderer>().material.color ==
               figures[indexB].GetComponent<Renderer>().material.color;
    }

    private bool AlFrenteDe(int indexA, int indexB)
    {
        if (!EstanCerca(indexA, indexB)) return false;
        Vector3 dir = figures[indexB].transform.position - figures[indexA].transform.position;
        return Vector3.Dot(figures[indexA].transform.forward, dir.normalized) > frontThreshold;
    }

    private bool EstanAlLado(int indexA, int indexB)
    {
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
        Vector3 centro = figures[indexCentro].transform.position;
        Vector3 a = figures[indexA].transform.position;
        Vector3 b = figures[indexB].transform.position;

        Vector3 puntoMedio = (a + b) / 2f;
        float distanciaAlCentro = Vector3.Distance(centro, puntoMedio);

        return distanciaAlCentro < closeDistance * 0.5f;
    }

    private bool EstanPerpendiculares(int indexA, int indexB, int indexC)
    {
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
    
    // Método para debugging - llamar manualmente para forzar actualización
    [ContextMenu("Force Update Predicates")]
    public void ForceUpdatePredicates()
    {
        Debug.Log("=== FORZANDO ACTUALIZACIÓN DE PREDICADOS ===");
        OnFigureMoved();
    }
    
    // Método para verificar el estado actual de todas las figuras
    [ContextMenu("Debug Figure Positions")]
    public void DebugFigurePositions()
    {
        Debug.Log("=== POSICIONES DE FIGURAS ===");
        for (int i = 0; i < figures.Count; i++)
        {
            if (figures[i] != null)
            {
                Debug.Log($"Figura {i} ({figures[i].name}): {figures[i].transform.position}");
            }
            else
            {
                Debug.LogError($"Figura {i} es null!");
            }
        }
    }
}