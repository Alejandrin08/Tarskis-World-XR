using UnityEngine;

public class FigureTracker : MonoBehaviour
{
    private Vector3 lastPosition;
    private OVRGrabbable grabbable;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (grabbable.isGrabbed || Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            lastPosition = transform.position;
            FindObjectOfType<TarskiLogic>().OnFigureMoved();
        }
    }
}