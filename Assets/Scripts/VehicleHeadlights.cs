using UnityEngine;

public class VehicleHeadlights : MonoBehaviour
{
    public Light leftHeadlight;
    public Light rightHeadlight;

    public KeyCode toggleKey = KeyCode.L;
    public bool headlightsOn = false;
    public bool onlyToggleWhileDriving = true;

    private SimpleVehicleController vehicleController;

    private void Awake()
    {
        vehicleController = GetComponent<SimpleVehicleController>();
    }

    private void Start()
    {
        SetHeadlights(headlightsOn);
    }

    private void Update()
    {
        if (onlyToggleWhileDriving && vehicleController != null && !vehicleController.enabled)
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            headlightsOn = !headlightsOn;
            SetHeadlights(headlightsOn);
        }
    }

    public void SetHeadlights(bool on)
    {
        if (leftHeadlight != null)
        {
            leftHeadlight.enabled = on;
        }

        if (rightHeadlight != null)
        {
            rightHeadlight.enabled = on;
        }
    }
}