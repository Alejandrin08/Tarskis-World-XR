using UnityEngine;
using System.Collections;

public class HandUIPalmActivation : MonoBehaviour
{
    [Header("Hand Tracking References")]
    public Transform wristBoneTransform;

    [Header("UI Configuration")]
    public GameObject predicateInterface;

    [Header("Detection Settings")]
    [Range(30f, 90f)]
    public float activationAngleThreshold = 45.0f;
    public float activationDelay = 0.1f;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Follow Settings")]
    public float followSpeed = 2.0f;
    public Vector3 offsetFromPalm = new Vector3(0, 0.1f, 0.05f); 
    public bool enableSmoothFollow = true;

    private float lastActivationTime;
    private bool isInterfaceActive = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 targetPosition;
    private Coroutine currentAnimation;

    private void Start()
    {
        if (predicateInterface != null)
        {
            originalScale = predicateInterface.transform.localScale;
            predicateInterface.SetActive(false);
        }
    }

    void Update()
    {
        if (wristBoneTransform == null || predicateInterface == null)
            return;

        bool shouldActivate = IsPalmFacingUp();

        if (shouldActivate && !isInterfaceActive && !isAnimating &&
            Time.time - lastActivationTime > activationDelay)
        {
            ShowInterface();
        }
        else if (!shouldActivate && isInterfaceActive && !isAnimating)
        {
            HideInterface();
        }

        if (isInterfaceActive && enableSmoothFollow)
        {
            UpdateInterfacePosition();
        }
    }

    private bool IsPalmFacingUp()
    {
        Vector3 directionToCamera = (Camera.main.transform.position - wristBoneTransform.position).normalized;
        Vector3 palmNormal = wristBoneTransform.forward;

        float angleToCamera = Vector3.Angle(palmNormal, directionToCamera);
        return angleToCamera <= activationAngleThreshold;
    }

    private void UpdateInterfacePosition()
    {
        Vector3 palmPosition = wristBoneTransform.position;
        Vector3 palmUp = wristBoneTransform.up;
        Vector3 palmForward = wristBoneTransform.forward;
        Vector3 palmRight = wristBoneTransform.right;

        targetPosition = palmPosition +
                        palmRight * offsetFromPalm.x +
                        palmUp * offsetFromPalm.y +
                        palmForward * offsetFromPalm.z;

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

    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offsetFromPalm = newOffset;
    }

    public void EnableSmoothFollow(bool enable)
    {
        enableSmoothFollow = enable;
    }
}