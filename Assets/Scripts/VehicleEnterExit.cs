using UnityEngine;

public class VehicleEnterExit : MonoBehaviour
{
    [Header("Player References")]
    public GameObject player;
    public GameObject playerVisual;
    public PlayerController playerController;
    public CharacterController characterController;

    [Header("Vehicle References")]
    public SimpleVehicleController vehicleController;
    public Rigidbody vehicleRigidbody;
    public Transform exitPoint;
    public VehicleHeadlights vehicleHeadlights;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public Transform playerCameraTarget;
    public Transform playerFirstPersonTarget;
    public Transform vehicleCameraTarget;

    [Header("Interaction")]
    public float enterDistance = 5f;
    public KeyCode interactKey = KeyCode.E;

    private bool isDriving = false;

    private void Start()
    {
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
            vehicleRigidbody.linearVelocity = Vector3.zero;
            vehicleRigidbody.angularVelocity = Vector3.zero;
        }

        if (vehicleHeadlights != null)
        {
            vehicleHeadlights.SetHeadlights(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (isDriving)
            {
                ExitVehicle();
            }
            else if (PlayerIsCloseEnough())
            {
                EnterVehicle();
            }
        }
    }

    private bool PlayerIsCloseEnough()
    {
        if (player == null)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= enterDistance)
        {
            Debug.Log("Player close enough to enter vehicle.");
            return true;
        }

        return false;
    }

    private void EnterVehicle()
    {
        isDriving = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (playerVisual != null)
        {
            playerVisual.SetActive(false);
        }

        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = false;
        }

        if (vehicleController != null)
        {
            vehicleController.enabled = true;
        }

        if (vehicleHeadlights != null)
        {
            vehicleHeadlights.SetHeadlights(true);
        }

        if (cameraFollow != null && vehicleCameraTarget != null)
        {
            cameraFollow.SetTarget(vehicleCameraTarget, vehicleCameraTarget, transform);
            Debug.Log("Camera switched to vehicle.");
        }
        else
        {
            Debug.LogWarning("Vehicle camera switch failed. Check CameraFollow or VehicleCameraTarget assignment.");
        }

        Debug.Log("Entered vehicle.");
    }

    private void ExitVehicle()
    {
        isDriving = false;

        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.linearVelocity = Vector3.zero;
            vehicleRigidbody.angularVelocity = Vector3.zero;
            vehicleRigidbody.isKinematic = true;
        }

        if (player != null && exitPoint != null)
        {
            player.transform.position = exitPoint.position;
            player.transform.rotation = exitPoint.rotation;
        }

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (vehicleHeadlights != null)
        {
            vehicleHeadlights.SetHeadlights(false);
        }

        if (cameraFollow != null && playerCameraTarget != null)
        {
            cameraFollow.SetTarget(playerCameraTarget, playerFirstPersonTarget, player.transform);
            Debug.Log("Camera switched back to player.");
        }

        Debug.Log("Exited vehicle.");
    }
}