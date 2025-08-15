using Oculus.Interaction;
using UnityEngine;
using System.Text;

public class FigureTracker : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private float notificationCooldown = 0.5f; 

    private Vector3 lastPosition;
    private Grabbable grabbable;
    private TarskiLogic tarskiLogic;
    private float lastNotificationTime;
    private bool wasBeingGrabbed;
    private int notificationCount = 0;

    void Start()
    {
        grabbable = GetComponent<Grabbable>();
        tarskiLogic = FindObjectOfType<TarskiLogic>();

        if (tarskiLogic == null)
        {
            enabled = false;
            return;
        }

        if (grabbable == null)
        {
            enabled = false;
            return;
        }
        
        lastPosition = transform.position;
        wasBeingGrabbed = false;
    }

    void Update()
    {
        if (tarskiLogic == null || grabbable == null) return;

        bool isBeingGrabbed = grabbable.SelectingPointsCount > 0;
        Vector3 currentPosition = transform.position;
        float distanceMoved = Vector3.Distance(currentPosition, lastPosition);
        bool hasMoved = distanceMoved > movementThreshold;
        bool canNotify = Time.time - lastNotificationTime > notificationCooldown;

        
        bool shouldNotify = false;
        string notificationReason = "";

        if (wasBeingGrabbed && !isBeingGrabbed)
        {
            shouldNotify = true;
            notificationReason = "FIGURA SOLTADA";
        }
        
        else if (hasMoved && canNotify && !isBeingGrabbed)
        {
            shouldNotify = true;
            notificationReason = $"MOVIMIENTO ({distanceMoved:F4}m)";
        }

        if (shouldNotify)
        {
            try
            {
                notificationCount++;
                lastPosition = currentPosition;
                lastNotificationTime = Time.time;

                tarskiLogic.OnFigureMoved();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ERROR al notificar desde {gameObject.name}: {ex.Message}");
            }
        }

        wasBeingGrabbed = isBeingGrabbed;
    }
}