using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 6f;
    public float backwardSpeed = 4f;
    public float turnSpeedKeys = 120f;
    public float turnSpeedMouse = 3f;
    public float gravity = -20f;

    [Header("Controls")]
    public bool allowMouseTurn = true;
    public bool allowKeyboardTurn = true;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        TurnCharacter();
        MoveCharacter();
        ApplyGravity();
    }

    private void TurnCharacter()
    {
        float turnAmount = 0f;

        if (allowKeyboardTurn)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                turnAmount -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                turnAmount += 1f;
            }
        }

        if (Mathf.Abs(turnAmount) > 0.01f)
        {
            transform.Rotate(
                0f,
                turnAmount * turnSpeedKeys * Time.deltaTime,
                0f
            );
        }

        if (allowMouseTurn)
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(
                0f,
                mouseX * turnSpeedMouse,
                0f
            );
        }
    }

    private void MoveCharacter()
    {
        float moveAmount = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveAmount += 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveAmount -= 1f;
        }

        Vector3 movement = Vector3.zero;

        if (moveAmount > 0f)
        {
            movement = transform.forward * forwardSpeed;
        }
        else if (moveAmount < 0f)
        {
            movement = -transform.forward * backwardSpeed;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            controller.Move(movement * Time.deltaTime);
        }

        if (animator != null)
        {
            float speedValue = Mathf.Abs(moveAmount);
            animator.SetFloat("Speed", speedValue);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}