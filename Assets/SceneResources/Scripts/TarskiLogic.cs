using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public enum DifficultyLevel
{
    Facil = 1,
    Medio = 2,
    Dificil = 3
}

public class TarskiLogic : MonoBehaviour
{
    [Header("Referencias")]
    public List<GameObject> figures = new List<GameObject>();
    public PredicateUIElement levelProgressUI; 

    [Header("Configuración del Nivel")]
    public DifficultyLevel currentLevel = DifficultyLevel.Facil;

    [Header("Distancias")]
    public float minDistance = 0.05f;
    public float maxDistance = 1.0f;
    public float closeDistance = 0.30f;
    public float farDistance = 0.5f;

    [Header("Posiciones")]
    public float frontThreshold = 0.8f;
    public float sideThreshold = 0.5f;
    public float heightTolerance = 0.1f;

    [Header("Eventos")]
    public UnityEvent OnLevelCompleted;

    private Dictionary<string, System.Func<bool>> allPredicates;
    private Dictionary<DifficultyLevel, List<string>> levelPredicates;
    private Dictionary<DifficultyLevel, string> levelNames; 
    private List<string> activePredicates;
    private bool hasInitializationErrors = false;

    void Start()
    {
        if (!ValidateFigures())
        {
            hasInitializationErrors = true;
            return;
        }

        try
        {
            InitializeAllPredicates();
            InitializeLevelPredicates();
            InitializeLevelNames(); 
            SetActivePredicatesForLevel(currentLevel);
            UpdateProgressUI(); 
        }
        catch (System.Exception ex)
        {
            hasInitializationErrors = true;
        }
    }

    private void InitializeAllPredicates()
    {
        allPredicates = new Dictionary<string, System.Func<bool>>()
        {
            { "PiramideCubo1MismoColor", () => MismoColor(0, 1) },
            { "Cubo1PrismaMismoColor", () => MismoColor(1, 2) },
            { "PiramidePrismaMismoColor", () => MismoColor(0, 2) },
            { "Cubo1Cubo2MismoColor", () => MismoColor(1, 3) },
            { "PrismaCilindroMismoColor", () => MismoColor(2, 4) },
            { "PiramideCubo2MismoColor", () => MismoColor(0, 3) },

            { "PiramideCubo1Cerca", () => EstanCerca(0, 1) },
            { "Cubo1PrismaCerca", () => EstanCerca(1, 2) },
            { "PiramidePrismaLejos", () => EstanLejos(0, 2) },
            { "PiramideCilindroLejos", () => EstanLejos(0, 4) },

            { "Cubo1AlFrentePiramide", () => AlFrenteDe(0, 1) },
            { "PrismaAlFrenteCubo1", () => AlFrenteDe(1, 2) },
            { "CilindroFrentePiramide", () => AlFrenteDe(0, 4) },
            { "PiramideEntreCubos", () => Entre(0, 1, 3) },
            { "Cubo1EntreOtros", () => Entre(1, 0, 2) },

            { "Cubo1AlLadoPiramide", () => EstanAlLado(0, 1) },
            { "PrismaAlLadoCubo1", () => EstanAlLado(1, 2) },
            { "TresFigurasAlineadas", () => EstanAlineadas(0, 1, 2) },
            { "CincoFigurasAlineadas", () => AlineacionCompleja() },

            { "FormacionTriangular", () => FormanTriangulo(0, 1, 2) },
            { "PiramideCentro", () => EstaEnCentro(0, 1, 2) },
            { "FigurasFormandoCruz", () => EstanPerpendiculares(0, 3, 2) }
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
                    "PiramideCubo1MismoColor",
                    "PiramideCubo1Cerca",
                    "Cubo1PrismaCerca"
                }
            },
            {
                DifficultyLevel.Medio,
                new List<string>()
                {
                    "Cubo1AlFrentePiramide",
                    "PrismaCilindroMismoColor",
                    "PrismaAlLadoCubo1",
                    "PiramideCilindroLejos"
                }
            },
            {
                DifficultyLevel.Dificil,
                new List<string>()
                {
                    "PiramideEntreCubos",
                    "PrismaAlLadoCubo1",
                    "PiramideCubo2MismoColor",
                    "CilindroFrentePiramide"
                }
            }
        };
    }

    private void InitializeLevelNames()
    {
        levelNames = new Dictionary<DifficultyLevel, string>()
        {
            { DifficultyLevel.Facil, "Nivel Fácil" },
            { DifficultyLevel.Medio, "Nivel Medio" },
            { DifficultyLevel.Dificil, "Nivel Difícil" }
        };
    }

    private bool ValidateFigures()
    {
        bool allValid = true;
        for (int i = 0; i < figures.Count; i++)
        {
            if (figures[i] == null)
            {
                allValid = false;
                continue;
            }

            var renderer = figures[i].GetComponent<Renderer>();
            if (renderer == null || renderer.material == null)
            {
                allValid = false;
            }
        }

        return allValid;
    }

    public void OnFigureMoved()
    {
        if (hasInitializationErrors)
        {
            return;
        }

        try
        {
            UpdateProgressUI(); 
            CheckLevelCompletion();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error en OnFigureMoved: {ex.Message}");
        }
    }

    private void UpdateProgressUI()
    {
        if (activePredicates == null || levelProgressUI == null)
        {
            return;
        }

        try
        {
            int completedCount = GetCompletedPredicatesCount();
            int totalCount = activePredicates.Count;
            float progress = totalCount > 0 ? (float)completedCount / totalCount : 0f;

            string levelName = levelNames.TryGetValue(currentLevel, out string name) ? name : currentLevel.ToString();

            levelProgressUI.UpdateProgress(progress, completedCount, totalCount, levelName);

        }
        catch (System.Exception ex)
        {
            if (levelProgressUI != null)
            {
                levelProgressUI.SetStatus(false, true);
            }
        }
    }

    private int GetCompletedPredicatesCount()
    {
        if (activePredicates == null || activePredicates.Count == 0)
            return 0;

        int completedCount = 0;
        foreach (var predicateName in activePredicates)
        {
            if (allPredicates.TryGetValue(predicateName, out var predicate))
            {
                try
                {
                    if (predicate.Invoke())
                    {
                        completedCount++;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error evaluando predicado {predicateName}: {ex.Message}");
                }
            }
        }

        return completedCount;
    }

    private bool MismoColor(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB))
        {
            return false;
        }

        try
        {
            var rendererA = figures[indexA].GetComponent<Renderer>();
            var rendererB = figures[indexB].GetComponent<Renderer>();

            if (rendererA == null || rendererB == null || rendererA.material == null || rendererB.material == null)
            {
                return false;
            }

            bool sameColor = rendererA.material.color == rendererB.material.color;
            return sameColor;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool EstanCerca(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB)) return false;

        float distance = Vector3.Distance(
            figures[indexA].transform.position,
            figures[indexB].transform.position
        );

        bool isClose = distance > minDistance && distance <= closeDistance;

        return isClose;
    }

    private bool EstanLejos(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB)) return false;
        try
        {
            float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
            return distance > farDistance;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool AlFrenteDe(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB) || !EstanCerca(indexA, indexB)) return false;
        try
        {
            Vector3 dir = figures[indexB].transform.position - figures[indexA].transform.position;
            return Vector3.Dot(figures[indexA].transform.forward, dir.normalized) > frontThreshold;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool EstanAlLado(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB)) return false;
        try
        {
            Vector3 posA = figures[indexA].transform.position;
            Vector3 posB = figures[indexB].transform.position;

            Vector3 horizontalDiff = new Vector3(posB.x - posA.x, 0, posB.z - posA.z);
            float horizontalDistance = horizontalDiff.magnitude;
            float heightDiff = Mathf.Abs(posA.y - posB.y);

            return horizontalDistance > minDistance && horizontalDistance < closeDistance &&
                   heightDiff < heightTolerance;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool Entre(int indexA, int indexB, int indexC)
    {
        if (!CheckFigureIndices(indexA, indexB, indexC)) return false;
        try
        {
            Vector3 a = figures[indexA].transform.position;
            Vector3 b = figures[indexB].transform.position;
            Vector3 c = figures[indexC].transform.position;

            Vector3 ba = a - b;
            Vector3 bc = c - b;
            Vector3 ac = a - c;

            float dot1 = Vector3.Dot(ba.normalized, bc.normalized);
            float dot2 = Vector3.Dot(ac.normalized, -bc.normalized);

            return dot1 > 0 && dot2 > 0;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool EstanAlineadas(params int[] indices)
    {
        if (indices.Length < 2) return false;
        foreach (var index in indices) if (!CheckFigureIndex(index)) return false;

        try
        {
            Vector3 firstPos = figures[indices[0]].transform.position;
            Vector3 dir = (figures[indices[1]].transform.position - firstPos).normalized;

            for (int i = 2; i < indices.Length; i++)
            {
                Vector3 currentDir = (figures[indices[i]].transform.position - firstPos).normalized;
                if (Vector3.Cross(dir, currentDir).magnitude > 0.1f) return false;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool FormanTriangulo(int indexA, int indexB, int indexC)
    {
        if (!CheckFigureIndices(indexA, indexB, indexC)) return false;
        try
        {
            float distAB = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
            float distBC = Vector3.Distance(figures[indexB].transform.position, figures[indexC].transform.position);
            float distCA = Vector3.Distance(figures[indexC].transform.position, figures[indexA].transform.position);

            return (distAB + distBC > distCA) && (distBC + distCA > distAB) && (distCA + distAB > distBC) &&
                   distAB > minDistance && distBC > minDistance && distCA > minDistance &&
                   distAB < maxDistance && distBC < maxDistance && distCA < maxDistance;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool EstaEnCentro(int indexCentro, int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexCentro, indexA, indexB)) return false;
        try
        {
            Vector3 centro = figures[indexCentro].transform.position;
            Vector3 a = figures[indexA].transform.position;
            Vector3 b = figures[indexB].transform.position;

            Vector3 puntoMedio = (a + b) / 2f;
            return Vector3.Distance(centro, puntoMedio) < closeDistance * 0.5f;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool EstanPerpendiculares(int indexA, int indexB, int indexC)
    {
        if (!CheckFigureIndices(indexA, indexB, indexC)) return false;
        try
        {
            Vector3 posA = figures[indexA].transform.position;
            Vector3 posB = figures[indexB].transform.position;
            Vector3 posC = figures[indexC].transform.position;

            Vector3 ba = (posA - posB).normalized;
            Vector3 bc = (posC - posB).normalized;

            return Mathf.Abs(Vector3.Dot(ba, bc)) < 0.3f;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool AlineacionCompleja()
    {
        try
        {
            return EstanAlineadas(0, 1, 3) && EstanAlineadas(1, 2, 4);
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    private bool CheckFigureIndex(int index)
    {
        bool valid = index >= 0 && index < figures.Count && figures[index] != null;
        return valid;
    }

    private bool CheckFigureIndices(params int[] indices)
    {
        foreach (var index in indices)
            if (!CheckFigureIndex(index)) return false;
        return true;
    }

    public void SetActivePredicatesForLevel(DifficultyLevel level)
    {
        if (hasInitializationErrors) return;

        currentLevel = level;
        if (levelPredicates.TryGetValue(level, out activePredicates))
        {
            if (levelProgressUI != null)
            {
                levelProgressUI.gameObject.SetActive(true);
                levelProgressUI.ResetProgress();

                levelProgressUI.levelIdentifier = $"Level_{level}";
            }

            UpdateProgressUI();
        }
    }

    private void CheckLevelCompletion()
    {
        if (activePredicates == null) return;

        try
        {
            int completedPredicates = GetCompletedPredicatesCount();

            if (completedPredicates == activePredicates.Count && activePredicates.Count > 0)
            {
                if (levelProgressUI != null)
                {
                    levelProgressUI.SetLevelCompleted();
                }

                OnLevelCompleted?.Invoke();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error en CheckLevelCompletion: {ex.Message}");
        }
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
        if (hasInitializationErrors || activePredicates == null || activePredicates.Count == 0) return 0f;

        try
        {
            int completed = GetCompletedPredicatesCount();
            return (float)completed / activePredicates.Count;
        }
        catch (System.Exception ex)
        {
            return 0f;
        }
    }

    public bool IsLevelCompleted()
    {
        return GetCompletionPercentage() >= 1.0f;
    }

    public int GetCurrentLevelTotalPredicates()
    {
        return activePredicates?.Count ?? 0;
    }

    public int GetCurrentLevelCompletedPredicates()
    {
        return GetCompletedPredicatesCount();
    }

    public string GetCurrentLevelName()
    {
        return levelNames.TryGetValue(currentLevel, out string name) ? name : currentLevel.ToString();
    }
}