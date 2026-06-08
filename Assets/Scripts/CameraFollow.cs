using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;
    public Transform firstPersonTarget;
    public Transform playerRoot;

    [Header("Mode")]
    public KeyCode switchCameraKey = KeyCode.C;
    public bool firstPersonMode = false;

    public bool IsFirstPersonMode => firstPersonMode;

    [Header("Third Person")]
    public float distance = 3f;
    public float height = 1.4f;
    public float minDistance = 0.6f;

    [Header("First Person")]
    public Vector3 firstPersonOffset = new Vector3(0f, 0f, 0.2f);

    [Header("Mouse Pitch")]
    public float pitchSensitivity = 3f;
    public float pitchMin = -35f;
    public float pitchMax = 70f;

    [Header("Camera Smoothing")]
    public float positionSmooth = 12f;
    public float rotationSmooth = 12f;

    [Header("Wall Collision")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.3f;
    public float collisionOffset = 0.25f;

    private float pitch = 20f;

    private void Start()
    {
        pitch = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SnapCameraToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Input.GetKeyDown(switchCameraKey))
        {
            firstPersonMode = !firstPersonMode;
            SnapCameraToTarget();
        }

        UpdatePitch();

        if (firstPersonMode)
        {
            PositionFirstPerson();
        }
        else
        {
            PositionThirdPersonBehindCharacter();
        }
    }

    private void UpdatePitch()
    {
        float mouseY = Input.GetAxis("Mouse Y") * pitchSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    private void PositionThirdPersonBehindCharacter()
    {
        Transform root = playerRoot != null ? playerRoot : target;

        Vector3 flatForward = root.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.01f)
        {
            flatForward = Vector3.forward;
        }

        flatForward.Normalize();

        float yaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;

        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPoint = target.position + Vector3.up * height;
        Vector3 desiredDirection = desiredRotation * Vector3.back;

        float finalDistance = distance;

        if (Physics.SphereCast(
                targetPoint,
                collisionRadius,
                desiredDirection,
                out RaycastHit hit,
                distance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
        {
            finalDistance = Mathf.Clamp(
                hit.distance - collisionOffset,
                minDistance,
                distance
            );
        }

        Vector3 desiredPosition = targetPoint + desiredDirection * finalDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.deltaTime
        );

        Quaternion lookRotation = Quaternion.LookRotation(targetPoint - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    private void PositionFirstPerson()
    {
        Transform root = playerRoot != null ? playerRoot : target;
        Transform fpTarget = firstPersonTarget != null ? firstPersonTarget : target;

        Vector3 flatForward = root.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.01f)
        {
            flatForward = Vector3.forward;
        }

        flatForward.Normalize();

        float yaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;

        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = fpTarget.position + desiredRotation * firstPersonOffset;

        transform.position = desiredPosition;
        transform.rotation = desiredRotation;
    }

    public void SetTarget(
        Transform newTarget,
        Transform newFirstPersonTarget = null,
        Transform newPlayerRoot = null)
    {
        target = newTarget;

        if (newFirstPersonTarget != null)
        {
            firstPersonTarget = newFirstPersonTarget;
        }

        if (newPlayerRoot != null)
        {
            playerRoot = newPlayerRoot;
        }

        SnapCameraToTarget();
    }

    public void SnapCameraToTarget()
    {
        if (target == null)
        {
            return;
        }

        if (firstPersonMode)
        {
            SnapFirstPerson();
        }
        else
        {
            SnapThirdPerson();
        }
    }

    private void SnapThirdPerson()
    {
        Transform root = playerRoot != null ? playerRoot : target;

        Vector3 flatForward = root.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.01f)
        {
            flatForward = Vector3.forward;
        }

        flatForward.Normalize();

        float yaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPoint = target.position + Vector3.up * height;
        Vector3 desiredDirection = desiredRotation * Vector3.back;

        float finalDistance = distance;

        if (Physics.SphereCast(
                targetPoint,
                collisionRadius,
                desiredDirection,
                out RaycastHit hit,
                distance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
        {
            finalDistance = Mathf.Clamp(
                hit.distance - collisionOffset,
                minDistance,
                distance
            );
        }

        transform.position = targetPoint + desiredDirection * finalDistance;
        transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
    }

    private void SnapFirstPerson()
    {
        Transform root = playerRoot != null ? playerRoot : target;
        Transform fpTarget = firstPersonTarget != null ? firstPersonTarget : target;

        Vector3 flatForward = root.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.01f)
        {
            flatForward = Vector3.forward;
        }

        flatForward.Normalize();

        float yaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;

        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = fpTarget.position + desiredRotation * firstPersonOffset;

        transform.position = desiredPosition;
        transform.rotation = desiredRotation;
    }
}