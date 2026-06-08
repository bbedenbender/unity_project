using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform holdPoint;

    [Tooltip("Main flashlight light. Keep this assigned for compatibility.")]
    public Light flashlightLight;

    [Tooltip("Optional extra lights, such as near/far beams.")]
    public Light[] additionalLights;

    [Header("Pickup")]
    public float pickupRange = 4f;
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode toggleKey = KeyCode.F;

    [Header("Held Transform")]
    public Vector3 heldLocalPosition = Vector3.zero;
    public Vector3 heldLocalRotation = new Vector3(0f, 90f, 0f);
    public Vector3 heldLocalScale = Vector3.one;

    [Header("Light")]
    public bool startOn = true;
    public bool turnOnWhenPickedUp = true;

    private bool isPickedUp = false;
    private bool lightIsOn = true;

    public bool IsPickedUp => isPickedUp;

    private void Start()
    {
        SetAllLights(startOn);
    }

    private void Update()
    {
        if (!isPickedUp)
        {
            TryPickup();
        }
        else
        {
            ToggleLight();
        }
    }

    private void TryPickup()
    {
        if (player == null || holdPoint == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRange && Input.GetKeyDown(pickupKey))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        isPickedUp = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalRotation);
        transform.localScale = heldLocalScale;

        SetAllLights(turnOnWhenPickedUp);

        Debug.Log("Flashlight picked up.");
    }

    private void ToggleLight()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetAllLights(!lightIsOn);
        }
    }

    private void SetAllLights(bool on)
    {
        lightIsOn = on;

        if (flashlightLight != null)
        {
            flashlightLight.enabled = on;
        }

        if (additionalLights != null)
        {
            foreach (Light light in additionalLights)
            {
                if (light != null)
                {
                    light.enabled = on;
                }
            }
        }
    }
}