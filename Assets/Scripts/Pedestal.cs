using UnityEngine;

public class Pedestal : MonoBehaviour
{
    public string requiredArtifactId;
    public Transform snapPoint;

    private bool completed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (completed) return;

        Artifact artifact = other.GetComponent<Artifact>();
        if (artifact == null) return;

        if (artifact.artifactId == requiredArtifactId)
        {
            completed = true;
            artifact.ReturnToPedestal(snapPoint);
            Debug.Log("Returned: " + artifact.artifactId);
        }
    }
}