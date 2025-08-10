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
    public List<PredicateUIElement> uiPredicates = new List<PredicateUIElement>();

    [Header("Configuración del Nivel")]
    public DifficultyLevel currentLevel = DifficultyLevel.Facil;

    [Header("Distancias")]
    public float minDistance = 0.05f;       // 5cm - distancia mínima para evitar colisiones
    public float maxDistance = 1.0f;        // 100cm - distancia máxima para predicados
    public float closeDistance = 0.30f;     // 15cm - para "EstanCerca"
    public float farDistance = 0.5f;

    [Header("Posiciones")]
    public float frontThreshold = 0.8f;     // Umbral más estricto para "al frente"
    public float sideThreshold = 0.5f;      // Umbral para "al lado"
    public float heightTolerance = 0.1f;

    [Header("Eventos")]
    public UnityEvent OnLevelCompleted;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private Dictionary<string, System.Func<bool>> allPredicates;
    private Dictionary<DifficultyLevel, List<string>> levelPredicates;
    private List<string> activePredicates;
    private bool hasInitializationErrors = false;

    void Start()
    {
        LogDebug("=== INICIANDO TARSKI LOGIC ===");

        // Validar figuras antes de inicializar
        if (!ValidateFigures())
        {
            hasInitializationErrors = true;
            Debug.LogError("❌ TARSKI LOGIC: Errores en validación de figuras. Sistema deshabilitado.");
            return;
        }

        try
        {
            InitializeAllPredicates();
            InitializeLevelPredicates();
            SetActivePredicatesForLevel(currentLevel);
            UpdateAllPredicatesUI();

            LogDebug("✅ TarskiLogic inicializado correctamente");
        }
        catch (System.Exception ex)
        {
            hasInitializationErrors = true;
            Debug.LogError($"❌ Error al inicializar TarskiLogic: {ex.Message}");
        }
    }

    private bool ValidateFigures()
    {
        LogDebug($"Validando {figures.Count} figuras...");

        bool allValid = true;
        for (int i = 0; i < figures.Count; i++)
        {
            if (figures[i] == null)
            {
                Debug.LogError($"❌ Figura en índice {i} es null");
                allValid = false;
                continue;
            }

            // Validar Renderer
            var renderer = figures[i].GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError($"❌ Figura '{figures[i].name}' (índice {i}) no tiene componente Renderer");
                allValid = false;
            }
            else if (renderer.material == null)
            {
                Debug.LogError($"❌ Figura '{figures[i].name}' (índice {i}) no tiene material asignado");
                allValid = false;
            }
            else
            {
                LogDebug($"✅ Figura '{figures[i].name}' (índice {i}) - OK");
            }
        }

        LogDebug($"Validación completada. Resultado: {(allValid ? "✅ ÉXITO" : "❌ ERRORES ENCONTRADOS")}");
        return allValid;
    }

    public void OnFigureMoved()
    {
        // Si hay errores de inicialización, no procesar
        if (hasInitializationErrors)
        {
            LogDebug("⚠️ OnFigureMoved ignorado debido a errores de inicialización");
            return;
        }

        try
        {
            UpdateAllPredicatesUI();
            CheckLevelCompletion();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error en OnFigureMoved: {ex.Message}");
        }
    }

    private void UpdateAllPredicatesUI()
    {
        if (activePredicates == null)
        {
            LogDebug("⚠️ activePredicates es null");
            return;
        }

        foreach (var uiElement in uiPredicates)
        {
            if (uiElement == null) continue;

            if (activePredicates.Contains(uiElement.predicateName) &&
                allPredicates.TryGetValue(uiElement.predicateName, out var predicate))
            {
                try
                {
                    bool status = predicate.Invoke();
                    uiElement.SetStatus(status, true);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Error evaluando predicado '{uiElement.predicateName}': {ex.Message}");
                    uiElement.SetStatus(false, true); // Marcar como falso en caso de error
                }
            }
        }
    }

    private bool MismoColor(int indexA, int indexB)
    {
        if (!CheckFigureIndices(indexA, indexB))
        {
            LogDebug($"⚠️ MismoColor({indexA}, {indexB}): Índices inválidos");
            return false;
        }

        try
        {
            var rendererA = figures[indexA].GetComponent<Renderer>();
            var rendererB = figures[indexB].GetComponent<Renderer>();

            if (rendererA == null)
            {
                Debug.LogError($"❌ Figura '{figures[indexA].name}' (índice {indexA}) no tiene Renderer");
                return false;
            }

            if (rendererB == null)
            {
                Debug.LogError($"❌ Figura '{figures[indexB].name}' (índice {indexB}) no tiene Renderer");
                return false;
            }

            if (rendererA.material == null || rendererB.material == null)
            {
                Debug.LogError($"❌ Una de las figuras no tiene material asignado");
                return false;
            }

            bool sameColor = rendererA.material.color == rendererB.material.color;
            LogDebug($"MismoColor({indexA}, {indexB}): {sameColor}");
            return sameColor;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error en MismoColor({indexA}, {indexB}): {ex.Message}");
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

        Debug.Log($"Validación de cercanía:\n" +
                 $"Figuras: {figures[indexA].name} y {figures[indexB].name}\n" +
                 $"Distancia: {distance * 100:F2}cm\n" +
                 $"Rango válido: {minDistance * 100:F0}cm a {closeDistance * 100:F0}cm\n" +
                 $"Resultado: {isClose}");

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
            Debug.LogError($"❌ Error en EstanLejos({indexA}, {indexB}): {ex.Message}");
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
            Debug.LogError($"❌ Error en AlFrenteDe({indexA}, {indexB}): {ex.Message}");
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
            Debug.LogError($"❌ Error en EstanAlLado({indexA}, {indexB}): {ex.Message}");
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
            Debug.LogError($"❌ Error en Entre({indexA}, {indexB}, {indexC}): {ex.Message}");
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
            Debug.LogError($"❌ Error en EstanAlineadas: {ex.Message}");
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
            Debug.LogError($"❌ Error en FormanTriangulo({indexA}, {indexB}, {indexC}): {ex.Message}");
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
            Debug.LogError($"❌ Error en EstaEnCentro({indexCentro}, {indexA}, {indexB}): {ex.Message}");
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
            Debug.LogError($"❌ Error en EstanPerpendiculares({indexA}, {indexB}, {indexC}): {ex.Message}");
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
            Debug.LogError($"❌ Error en AlineacionCompleja: {ex.Message}");
            return false;
        }
    }

    private bool CheckFigureIndex(int index)
    {
        bool valid = index >= 0 && index < figures.Count && figures[index] != null;
        if (!valid)
        {
            LogDebug($"⚠️ Índice inválido: {index} (Total figuras: {figures.Count})");
        }
        return valid;
    }

    private bool CheckFigureIndices(params int[] indices)
    {
        foreach (var index in indices)
            if (!CheckFigureIndex(index)) return false;
        return true;
    }

    // Resto de métodos sin cambios
    private void InitializeAllPredicates()
    {
        allPredicates = new Dictionary<string, System.Func<bool>>()
        {
            { "PiramideCubo1MismoColor", () => MismoColor(0, 1) },
            { "Cubo1PrismaMismoColor", () => MismoColor(1, 2) },
            { "PiramidePrismaMismoColor", () => MismoColor(0, 2) },
            { "Cubo1Cubo2MismoColor", () => MismoColor(1, 3) },
            { "PrismaCilindroMismoColor", () => MismoColor(2, 4) },

            { "PiramideCubo1Cerca", () => EstanCerca(0, 1) },
            { "Cubo1PrismaCerca", () => EstanCerca(1, 2) },
            { "PiramidePrismaLejos", () => EstanLejos(0, 2) },
            { "PiramideCilindroLejos", () => EstanLejos(0, 4) },

            { "Cubo1AlFrentePiramide", () => AlFrenteDe(0, 1) },
            { "PrismaAlFrenteCubo1", () => AlFrenteDe(1, 2) },
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
                    "FormacionTriangular",
                    "FigurasFormandoCruz",
                    "CincoFigurasAlineadas"
                }
            }
        };
    }

    public void SetActivePredicatesForLevel(DifficultyLevel level)
    {
        if (hasInitializationErrors) return;

        currentLevel = level;
        if (levelPredicates.TryGetValue(level, out activePredicates))
        {
            foreach (var uiElement in uiPredicates)
            {
                if (uiElement == null) continue;

                bool shouldShow = activePredicates.Contains(uiElement.predicateName);
                uiElement.gameObject.SetActive(shouldShow);
                uiElement.SetStatus(false, shouldShow);
            }
            UpdateAllPredicatesUI();
        }
    }

    private void CheckLevelCompletion()
    {
        if (activePredicates == null) return;

        try
        {
            int completedPredicates = 0;
            foreach (var predicateName in activePredicates)
            {
                if (allPredicates.TryGetValue(predicateName, out var predicate) && predicate.Invoke())
                {
                    completedPredicates++;
                }
            }

            if (completedPredicates == activePredicates.Count && activePredicates.Count > 0)
            {
                LogDebug("🎉 ¡NIVEL COMPLETADO!");
                OnLevelCompleted?.Invoke();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error en CheckLevelCompletion: {ex.Message}");
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
            int completed = 0;
            foreach (var predicateName in activePredicates)
            {
                if (allPredicates.TryGetValue(predicateName, out var predicate) && predicate.Invoke())
                {
                    completed++;
                }
            }

            return (float)completed / activePredicates.Count;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error en GetCompletionPercentage: {ex.Message}");
            return 0f;
        }
    }

    public bool IsLevelCompleted()
    {
        return GetCompletionPercentage() >= 1.0f;
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TARSKI_LOGIC] {message}");
        }
    }
}