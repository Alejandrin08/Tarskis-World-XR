using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;
using UnityEngine;

public class HandUIGestureActivation : MonoBehaviour
{
    [Header("Hand Tracking References")]
    public OVRHand ovrHand;
    public Transform handTransform; 

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
    public bool useOVRMicrogestures = true;
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
    private Coroutine currentAnimation;
    private OVRHand.MicrogestureType lastMicrogesture = OVRHand.MicrogestureType.NoGesture;

    void Start()
    {
        if (predicateInterface != null)
        {
            originalScale = predicateInterface.transform.localScale;
            predicateInterface.SetActive(false);
        }
    }

    void Update()
    {
        if (useOVRMicrogestures && ovrHand != null)
        {
            HandleMicrogestures();
        }

        if (isInterfaceActive && enableSmoothFollow && handTransform != null)
        {
            UpdateInterfacePosition();
        }
    }

    private void HandleMicrogestures()
    {
        OVRHand.MicrogestureType currentMicrogesture = ovrHand.GetMicrogestureType();

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

                default:
                    break;
            }

            lastMicrogesture = currentMicrogesture;
        }
    }

    private void OnGestureDetected()
    {
        if (!isInterfaceActive && !isAnimating && predicateInterface != null)
        {
            ShowInterface();
        }
    }

    private void OnGestureReleased()
    {
        if (isInterfaceActive && !isAnimating && predicateInterface != null)
        {
            HideInterface();
        }
    }

    private void UpdateInterfacePosition()
    {
        if (handTransform == null) return;

        Vector3 targetPosition = handTransform.position +
                                handTransform.forward * offsetFromHand.z +
                                handTransform.up * offsetFromHand.y +
                                handTransform.right * offsetFromHand.x;

        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - Camera.main.transform.position);

        if (enableSmoothFollow)
        {
            predicateInterface.transform.position = Vector3.Lerp(
                predicateInterface.transform.position,
                targetPosition,
                followSpeed * Time.deltaTime);

            predicateInterface.transform.rotation = Quaternion.Slerp(
                predicateInterface.transform.rotation,
                targetRotation,
                followSpeed * Time.deltaTime);
        }
        else
        {
            predicateInterface.transform.position = targetPosition;
            predicateInterface.transform.rotation = targetRotation;
        }
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