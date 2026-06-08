using UnityEngine;

public class ExhibitObject : MonoBehaviour
{
    public string objectId;
    public string displayName;

    [TextArea]
    public string description;

    public Transform displayPoint;
    public Transform recoveryPoint;

    public bool isStolen = false;
    public bool isTargeted = false;

    public void MarkTargeted(bool targeted)
    {
        isTargeted = targeted;
    }

    public void StealObject()
    {
        isStolen = true;
        isTargeted = false;

        if (recoveryPoint != null)
        {
            transform.position = recoveryPoint.position;
            transform.rotation = recoveryPoint.rotation;
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log(displayName + " was stolen!");
    }

    public void RestoreObject()
    {
        isStolen = false;
        isTargeted = false;

        gameObject.SetActive(true);

        if (displayPoint != null)
        {
            transform.position = displayPoint.position;
            transform.rotation = displayPoint.rotation;
        }

        Debug.Log(displayName + " restored.");
    }
}