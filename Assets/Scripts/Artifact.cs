using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Artifact : MonoBehaviour
{
    public string artifactId;
    public bool isReturned = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ReturnToPedestal(Transform snapPoint)
    {
        isReturned = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;
    }
}