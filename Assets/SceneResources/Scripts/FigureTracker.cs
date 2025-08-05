using UnityEngine;

public class FigureTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    public float movementThreshold = 0.01f;
    public float updateFrequency = 0.1f; // Update every 0.1 seconds instead of every frame
    
    [Header("Debug")]
    public bool enableDebugLogs = false;
    
    private Vector3 lastPosition;
    private OVRGrabbable grabbable;
    private TarskiLogic tarskiLogic;
    private float lastUpdateTime;
    private bool wasGrabbed = false;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        lastPosition = transform.position;
        
        // Find TarskiLogic once at start instead of every update
        tarskiLogic = FindObjectOfType<TarskiLogic>();
        
        if (tarskiLogic == null)
        {
            Debug.LogError($"TarskiLogic not found! FigureTracker on {gameObject.name} won't work.");
        }
        
        if (grabbable == null)
        {
            Debug.LogWarning($"OVRGrabbable not found on {gameObject.name}. Movement detection will only work by position.");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"FigureTracker initialized on {gameObject.name}");
        }
    }

    void Update()
    {
        // Only update at specified frequency to improve performance
        if (Time.time - lastUpdateTime < updateFrequency) return;
        
        bool currentlyGrabbed = grabbable != null && grabbable.isGrabbed;
        bool positionChanged = Vector3.Distance(transform.position, lastPosition) > movementThreshold;
        bool grabStateChanged = wasGrabbed != currentlyGrabbed;
        
        // Trigger update if:
        // 1. Object is currently being grabbed
        // 2. Position changed significantly
        // 3. Grab state changed (picked up or released)
        if (currentlyGrabbed || positionChanged || grabStateChanged)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name} moved - Position: {transform.position}, Grabbed: {currentlyGrabbed}");
            }
            
            lastPosition = transform.position;
            lastUpdateTime = Time.time;
            wasGrabbed = currentlyGrabbed;
            
            // Notify TarskiLogic
            if (tarskiLogic != null)
            {
                tarskiLogic.OnFigureMoved();
            }
        }
    }
    
    // Method to manually trigger an update (useful for external scripts)
    public void ForceUpdate()
    {
        if (tarskiLogic != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"Force update triggered on {gameObject.name}");
            }
            tarskiLogic.OnFigureMoved();
        }
    }
    
    // Get current status for debugging
    public bool IsBeingTracked()
    {
        return tarskiLogic != null;
    }
}