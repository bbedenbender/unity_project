using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleVehicleController : MonoBehaviour
{
    public float acceleration = 900f;
    public float reverseAcceleration = 500f;
    public float turnSpeed = 90f;
    public float maxSpeed = 12f;
    public float brakeForce = 4f;

    public Transform visualModel;
    public float visualLeanAmount = 5f;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }

    private void Update()
    {
        moveInput = 0f;
        turnInput = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveInput = 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveInput = -1f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            turnInput = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            turnInput = 1f;
        }

        AnimateVisual();
    }

    private void FixedUpdate()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float forwardSpeed = localVelocity.z;

        if (Mathf.Abs(forwardSpeed) < maxSpeed)
        {
            float force = moveInput >= 0f ? acceleration : reverseAcceleration;
            rb.AddForce(transform.forward * moveInput * force * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        if (Mathf.Abs(forwardSpeed) > 0.2f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            float direction = Mathf.Sign(forwardSpeed);

            Quaternion turnRotation = Quaternion.Euler(0f, turn * direction, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        if (moveInput == 0f)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                Vector3.zero,
                brakeForce * Time.fixedDeltaTime
            );
        }
    }

    private void AnimateVisual()
    {
        if (visualModel == null) return;

        float lean = -turnInput * visualLeanAmount;

        visualModel.localRotation = Quaternion.Slerp(
            visualModel.localRotation,
            Quaternion.Euler(0f, 0f, lean),
            8f * Time.deltaTime
        );
    }
}