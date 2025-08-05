using UnityEngine;

public class FigureTracker : MonoBehaviour
{
    private Vector3 lastPosition;
    private OVRGrabbable grabbable;
    private TarskiLogic tarskiLogic;
    private bool hasValidTarskiLogic = false;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        lastPosition = transform.position;
        
        // Buscar TarskiLogic una sola vez al inicio
        tarskiLogic = FindObjectOfType<TarskiLogic>();
        hasValidTarskiLogic = tarskiLogic != null;
        
        if (!hasValidTarskiLogic)
        {
            Debug.LogError($"FigureTracker en {gameObject.name}: No se encontró TarskiLogic en la escena!");
        }
        else
        {
            Debug.Log($"FigureTracker en {gameObject.name}: TarskiLogic encontrado correctamente");
        }
    }

    void Update()
    {
        // Solo verificar movimiento si tenemos una referencia válida a TarskiLogic
        if (!hasValidTarskiLogic) return;
        
        bool figureMoved = false;
        
        // Verificar si la figura está siendo agarrada
        if (grabbable != null && grabbable.isGrabbed)
        {
            figureMoved = true;
        }
        // O si la posición ha cambiado significativamente
        else if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            figureMoved = true;
        }
        
        if (figureMoved)
        {
            lastPosition = transform.position;
            Debug.Log($"Figura {gameObject.name} movida. Notificando a TarskiLogic...");
            tarskiLogic.OnFigureMoved();
        }
    }
    
    // Método para forzar una actualización manual (útil para debugging)
    public void ForceUpdate()
    {
        if (hasValidTarskiLogic)
        {
            Debug.Log($"Forzando actualización desde {gameObject.name}");
            tarskiLogic.OnFigureMoved();
        }
    }
}