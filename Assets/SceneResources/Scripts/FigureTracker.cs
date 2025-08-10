using Oculus.Interaction;
using UnityEngine;
using System.Text;

public class FigureTracker : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private float notificationCooldown = 0.5f; // Aumentado para menos spam

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false; // Desactivado por defecto
    [SerializeField] private bool enableVerboseLogging = false;
    [SerializeField] private bool logOnlyOnRelease = true; // Solo loggear al soltar

    private Vector3 lastPosition;
    private Grabbable grabbable;
    private TarskiLogic tarskiLogic;
    private float lastNotificationTime;
    private bool wasBeingGrabbed;
    private int notificationCount = 0;

    void Start()
    {
        LogDebug($"=== INICIANDO FIGURE TRACKER EN {gameObject.name} ===");

        // Inicializar componentes
        grabbable = GetComponent<Grabbable>();
        tarskiLogic = FindObjectOfType<TarskiLogic>();

        // Verificaciones con logging detallado
        if (tarskiLogic == null)
        {
            LogError($"❌ TarskiLogic no encontrado en la escena. FigureTracker en {gameObject.name} DESHABILITADO.");
            enabled = false;
            return;
        }
        else
        {
            LogDebug($"✅ TarskiLogic encontrado: {tarskiLogic.gameObject.name}");
        }

        if (grabbable == null)
        {
            LogError($"❌ Grabbable no encontrado en {gameObject.name}. FigureTracker DESHABILITADO.");
            enabled = false;
            return;
        }
        else
        {
            LogDebug($"✅ Grabbable encontrado en {gameObject.name}");
        }

        // Estado inicial
        lastPosition = transform.position;
        wasBeingGrabbed = false;

        LogDebug($"✅ FigureTracker INICIALIZADO correctamente");
        LogDebug($"   - Posición inicial: {lastPosition}");
        LogDebug($"   - Movement Threshold: {movementThreshold}");
        LogDebug($"   - Notification Cooldown: {notificationCooldown}s");
    }

    void Update()
    {
        if (tarskiLogic == null || grabbable == null) return;

        // Estado actual
        bool isBeingGrabbed = grabbable.SelectingPointsCount > 0;
        Vector3 currentPosition = transform.position;
        float distanceMoved = Vector3.Distance(currentPosition, lastPosition);
        bool hasMoved = distanceMoved > movementThreshold;
        bool canNotify = Time.time - lastNotificationTime > notificationCooldown;

        // Logging verbose si está habilitado
        if (enableVerboseLogging && enableDebugLogs)
        {
            LogDebug($"[VERBOSE] Grabbed: {isBeingGrabbed}, Moved: {distanceMoved:F4}m, CanNotify: {canNotify}");
        }

        bool shouldNotify = false;
        string notificationReason = "";

        // Detectar cambio de estado (soltado) - PRIORITARIO
        if (wasBeingGrabbed && !isBeingGrabbed)
        {
            shouldNotify = true;
            notificationReason = "FIGURA SOLTADA";

            // Solo log cuando se suelta o si verbose está activado
            if (logOnlyOnRelease || enableDebugLogs)
            {
                LogDebug($"🔥 {gameObject.name} fue SOLTADA en posición {currentPosition:F2}");
            }
        }
        // Detectar movimiento significativo SOLO si no está siendo agarrado
        else if (hasMoved && canNotify && !isBeingGrabbed)
        {
            shouldNotify = true;
            notificationReason = $"MOVIMIENTO ({distanceMoved:F4}m)";

            if (enableVerboseLogging && enableDebugLogs)
            {
                LogDebug($"📍 {gameObject.name} se movió {distanceMoved:F4}m");
            }
        }

        // Ejecutar notificación
        if (shouldNotify)
        {
            try
            {
                notificationCount++;
                lastPosition = currentPosition;
                lastNotificationTime = Time.time;

                // Solo log detallado si está habilitado
                if (enableDebugLogs && !logOnlyOnRelease)
                {
                    LogDebug($"🚨 NOTIFICACIÓN #{notificationCount}: {notificationReason}");
                }

                tarskiLogic.OnFigureMoved();

                // Log de éxito solo si está habilitado
                if (enableDebugLogs && enableVerboseLogging)
                {
                    LogDebug($"✅ Notificación enviada exitosamente a TarskiLogic");
                }
            }
            catch (System.Exception ex)
            {
                // Errores críticos siempre se loggean
                LogError($"❌ ERROR al notificar desde {gameObject.name}: {ex.Message}");
            }
        }

        wasBeingGrabbed = isBeingGrabbed;
    }

    // Sistema de logging mejorado
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[FIGURE_TRACKER] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[FIGURE_TRACKER] {message}");
    }

    // Métodos públicos para debugging
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintDebugInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== DEBUG INFO para {gameObject.name} ===");
        sb.AppendLine($"TarskiLogic: {(tarskiLogic != null ? "✅" : "❌")}");
        sb.AppendLine($"Grabbable: {(grabbable != null ? "✅" : "❌")}");
        sb.AppendLine($"SelectingPoints: {grabbable?.SelectingPointsCount ?? 0}");
        sb.AppendLine($"Posición actual: {transform.position}");
        sb.AppendLine($"Última posición: {lastPosition}");
        sb.AppendLine($"Distancia desde último: {Vector3.Distance(transform.position, lastPosition):F4}m");
        sb.AppendLine($"Notificaciones enviadas: {notificationCount}");
        sb.AppendLine($"Última notificación: {Time.time - lastNotificationTime:F2}s atrás");

        LogDebug(sb.ToString());
    }

    public void ForceNotification()
    {
        if (tarskiLogic != null)
        {
            notificationCount++;
            lastPosition = transform.position;
            LogDebug($"🔧 NOTIFICACIÓN FORZADA #{notificationCount} desde {gameObject.name}");
            tarskiLogic.OnFigureMoved();
        }
    }

    public void ToggleDebugLogs() => enableDebugLogs = !enableDebugLogs;
    public void ToggleVerboseLogging() => enableVerboseLogging = !enableVerboseLogging;
}