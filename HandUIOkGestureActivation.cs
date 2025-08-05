using UnityEngine;
using System.Collections;
using Oculus.Interaction.Input;
using Oculus.Interaction;

public class HandUIOkGestureActivation : MonoBehaviour
{
    [Header("Hand Tracking References")]
    public OVRHand ovrHand; // Fixed missing semicolon
    public Hand leftHand; // Added missing leftHand reference

    [Header("UI Configuration")]
    public GameObject predicateInterface;

    [Header("Gesture Detection Settings")]
    [Range(0.1f, 0.8f)]
    public float thumbIndexDistanceThreshold = 0.03f;
    [Range(0.5f, 1.0f)]
    public float otherFingersExtensionThreshold = 0.7f;
    public float gestureConfidenceThreshold = 0.8f; 
    public float activationDelay = 0.1f;
    
    [Header("Microgesture Settings")]
    public bool useOVRMicrogestures = true; // Toggle between OVR microgestures and custom OK detection
    public OVRHand.MicrogestureType activationGesture = OVRHand.MicrogestureType.ThumbTap;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Follow Settings")]
    public float followSpeed = 2.0f;
    public Vector3 offsetFromHand = new Vector3(0, 0.1f, 0.1f);
    public bool enableSmoothFollow = true;

    private float lastActivationTime;
    private bool isInterfaceActive = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 targetPosition;
    private Coroutine currentAnimation;
    private OVRHand.MicrogestureType lastMicrogesture = OVRHand.MicrogestureType.NoGesture;

    private void Start()
    {
        if (predicateInterface != null)
        {
            originalScale = predicateInterface.transform.localScale;
            predicateInterface.SetActive(false);
        }

        // Auto-find leftHand if not assigned
        if (leftHand == null)
        {
            Hand[] hands = FindObjectsOfType<Hand>();
            foreach (Hand hand in hands)
            {
                if (hand.Handedness == Handedness.Left)
                {
                    leftHand = hand;
                    break;
                }
            }
        }

        // Auto-find OVRHand if not assigned
        if (ovrHand == null)
        {
            ovrHand = FindObjectOfType<OVRHand>();
        }
    }

    void Update()
    {
        if (useOVRMicrogestures && ovrHand != null)
        {
            HandleMicrogestures();
        }
        else if (leftHand != null)
        {
            HandleCustomOkGesture();
        }

        // Update interface position if it's active and following is enabled
        if (isInterfaceActive && enableSmoothFollow)
        {
            UpdateInterfacePosition();
        }
    }

    private void HandleMicrogestures()
    {
        OVRHand.MicrogestureType currentMicrogesture = ovrHand.GetMicrogestureType();

        // Check if the gesture changed
        if (currentMicrogesture != lastMicrogesture)
        {
            switch (currentMicrogesture)
            {
                case OVRHand.MicrogestureType.ThumbTap:
                    OnGestureDetected();
                    break;
                    
                case OVRHand.MicrogestureType.NoGesture:
                    OnGestureReleased();
                    break;
                    
                // You can add more gesture types here if needed
                default:
                    // Handle other gestures or do nothing
                    break;
            }
            
            lastMicrogesture = currentMicrogesture;
        }
    }

    private void HandleCustomOkGesture()
    {
        bool currentlyDetectingOK = IsOkGesture();
        
        if (currentlyDetectingOK && !isInterfaceActive && !isAnimating)
        {
            if (Time.time - lastActivationTime >= activationDelay)
            {
                OnGestureDetected();
            }
        }
        else if (!currentlyDetectingOK && isInterfaceActive && !isAnimating)
        {
            OnGestureReleased();
        }
    }

    private void OnGestureDetected()
    {
        if (!isInterfaceActive && !isAnimating)
        {
            ShowInterface();
        }
    }

    private void OnGestureReleased()
    {
        if (isInterfaceActive && !isAnimating)
        {
            HideInterface();
        }
    }

    private bool IsOkGesture()
    {
        if (!leftHand.IsTrackedDataValid)
        {
            return false;
        }

        if (!leftHand.GetFingerIsHighConfidence(HandFinger.Index))
        {
            return false;
        }

        if (!leftHand.GetJointPose(HandJointId.HandThumbTip, out Pose thumbTip) ||
            !leftHand.GetJointPose(HandJointId.HandIndexTip, out Pose indexTip))
        {
            return false;
        }

        float thumbIndexDistance = Vector3.Distance(thumbTip.position, indexTip.position);
        bool fingersClose = thumbIndexDistance <= thumbIndexDistanceThreshold;

        bool middleExtended = IsFingerExtended(HandFinger.Middle);
        bool ringExtended = IsFingerExtended(HandFinger.Ring);
        bool pinkyExtended = IsFingerExtended(HandFinger.Pinky);

        bool otherFingersExtended = middleExtended && ringExtended && pinkyExtended;

        return fingersClose && otherFingersExtended;
    }

    private bool IsFingerExtended(HandFinger finger)
    {
        float fingerStrength = leftHand.GetFingerPinchStrength(finger);
        return (1.0f - fingerStrength) >= otherFingersExtensionThreshold;
    }

    private void UpdateInterfacePosition()
    {
        if (leftHand == null) return;

        if (!leftHand.GetJointPose(HandJointId.HandWristRoot, out Pose wristPose))
        {
            return;
        }

        Vector3 handPosition = wristPose.position;
        Vector3 handUp = wristPose.rotation * Vector3.up;
        Vector3 handForward = wristPose.rotation * Vector3.forward;
        Vector3 handRight = wristPose.rotation * Vector3.right;

        targetPosition = handPosition +
                        handRight * offsetFromHand.x +
                        handUp * offsetFromHand.y +
                        handForward * offsetFromHand.z;

        predicateInterface.transform.position = Vector3.Lerp(
            predicateInterface.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookDirection = Camera.main.transform.position - predicateInterface.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        predicateInterface.transform.rotation = Quaternion.Lerp(
            predicateInterface.transform.rotation,
            targetRotation,
            followSpeed * Time.deltaTime
        );
    }

    private void ShowInterface()
    {
        isInterfaceActive = true;
        lastActivationTime = Time.time;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateShow());
    }

    private void HideInterface()
    {
        isInterfaceActive = false;
        lastActivationTime = Time.time;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateHide());
    }

    private IEnumerator AnimateShow()
    {
        isAnimating = true;

        predicateInterface.SetActive(true);

        UpdateInterfacePosition();

        predicateInterface.transform.localScale = Vector3.zero;

        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;

            float scaleValue = showCurve.Evaluate(normalizedTime);
            predicateInterface.transform.localScale = originalScale * scaleValue;

            yield return null;
        }

        predicateInterface.transform.localScale = originalScale;
        isAnimating = false;
    }

    private IEnumerator AnimateHide()
    {
        isAnimating = true;

        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;

            float scaleValue = hideCurve.Evaluate(normalizedTime);
            predicateInterface.transform.localScale = originalScale * scaleValue;

            yield return null;
        }

        predicateInterface.SetActive(false);
        isAnimating = false;
    }
}