using UnityEngine;

public class FlashlightIdleAim : MonoBehaviour
{
    [Header("References")]
    public FlashlightPickup flashlightPickup;
    public Light flashlightLight;
    public Transform playerRoot;
    public Transform cameraTransform;
    public CameraFollow cameraFollow;

    [Header("Aim Rules")]
    public bool aimOnlyWhenIdle = true;
    public bool useCameraPitchInFirstPerson = true;
    public bool requireRightMouseButton = false;

    [Header("Idle Aiming")]
    public float aimSmooth = 18f;

    [Tooltip("Adjusts flashlight aim only in third-person idle view. Use X to tilt up/down.")]
    public Vector3 thirdPersonAimOffset = new Vector3(10f, 0f, 0f);

    [Tooltip("Keeps first-person idle aiming separate from third-person.")]
    public Vector3 firstPersonAimOffset = Vector3.zero;

    [Header("First Person Walk Swing")]
    public bool swingWhenWalkingInFirstPerson = true;
    public float walkSwingSpeed = 10f;
    public float walkPositionAmount = 0.035f;
    public float walkRotationAmount = 4f;
    public float returnSmooth = 14f;

    private Quaternion startingLightLocalRotation;

    private bool cachedHeldTransform = false;
    private Vector3 heldLocalPosition;
    private Quaternion heldLocalRotation;
    private Vector3 heldLocalScale;

    private void Awake()
    {
        if (flashlightPickup == null)
        {
            flashlightPickup = GetComponent<FlashlightPickup>();
        }

        if (flashlightLight == null)
        {
            flashlightLight = GetComponentInChildren<Light>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }

        if (flashlightLight != null)
        {
            startingLightLocalRotation = flashlightLight.transform.localRotation;
        }
    }

    private void LateUpdate()
    {
        if (flashlightPickup == null || !flashlightPickup.IsPickedUp)
        {
            return;
        }

        if (flashlightLight == null)
        {
            return;
        }

        CacheHeldTransformAfterPickup();

        bool moving = MovementInputPressed();
        bool firstPerson = cameraFollow != null && cameraFollow.IsFirstPersonMode;

        if (ShouldAimForward())
        {
            RestoreHeldTransform();
            AimFlashlightForward();
            return;
        }

        if (firstPerson && moving && swingWhenWalkingInFirstPerson)
        {
            ApplyFirstPersonWalkSwing();
            ReturnLightToHeldDirection();
            return;
        }

        RestoreHeldTransform();
        ReturnLightToHeldDirection();
    }

    private void CacheHeldTransformAfterPickup()
    {
        if (cachedHeldTransform)
        {
            return;
        }

        heldLocalPosition = transform.localPosition;
        heldLocalRotation = transform.localRotation;
        heldLocalScale = transform.localScale;

        cachedHeldTransform = true;
    }

    private bool ShouldAimForward()
    {
        if (requireRightMouseButton && !Input.GetMouseButton(1))
        {
            return false;
        }

        if (!aimOnlyWhenIdle)
        {
            return true;
        }

        return !MovementInputPressed();
    }

    private bool MovementInputPressed()
    {
        return
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.RightArrow);
    }

    private void AimFlashlightForward()
    {
        Vector3 aimDirection = GetAimDirection();

        if (aimDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        bool firstPerson =
            cameraFollow != null &&
            cameraFollow.IsFirstPersonMode;

        Vector3 offset = firstPerson ? firstPersonAimOffset : thirdPersonAimOffset;

        Quaternion targetRotation =
            Quaternion.LookRotation(aimDirection, Vector3.up) *
            Quaternion.Euler(offset);

        flashlightLight.transform.rotation = Quaternion.Slerp(
            flashlightLight.transform.rotation,
            targetRotation,
            aimSmooth * Time.deltaTime
        );
    }

    private Vector3 GetAimDirection()
    {
        bool firstPerson =
            cameraFollow != null &&
            cameraFollow.IsFirstPersonMode;

        if (firstPerson && useCameraPitchInFirstPerson && cameraTransform != null)
        {
            return cameraTransform.forward.normalized;
        }

        if (playerRoot != null)
        {
            Vector3 forward = playerRoot.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        return transform.forward;
    }

    private void ApplyFirstPersonWalkSwing()
    {
        float time = Time.time * walkSwingSpeed;

        float side = Mathf.Sin(time) * walkPositionAmount;
        float bob = Mathf.Abs(Mathf.Cos(time)) * walkPositionAmount * 0.6f;

        float rotX = Mathf.Sin(time) * walkRotationAmount;
        float rotY = Mathf.Sin(time * 0.5f) * walkRotationAmount * 0.5f;
        float rotZ = Mathf.Cos(time) * walkRotationAmount;

        Vector3 targetLocalPosition = heldLocalPosition + new Vector3(side, bob, 0f);

        Quaternion targetLocalRotation =
            heldLocalRotation *
            Quaternion.Euler(rotX, rotY, rotZ);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetLocalPosition,
            returnSmooth * Time.deltaTime
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetLocalRotation,
            returnSmooth * Time.deltaTime
        );

        transform.localScale = heldLocalScale;
    }

    private void RestoreHeldTransform()
    {
        if (!cachedHeldTransform)
        {
            return;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            heldLocalPosition,
            returnSmooth * Time.deltaTime
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            heldLocalRotation,
            returnSmooth * Time.deltaTime
        );

        transform.localScale = heldLocalScale;
    }

    private void ReturnLightToHeldDirection()
    {
        flashlightLight.transform.localRotation = Quaternion.Slerp(
            flashlightLight.transform.localRotation,
            startingLightLocalRotation,
            aimSmooth * Time.deltaTime
        );
    }
}