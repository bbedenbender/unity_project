using UnityEngine;

public class TestExhibitControls : MonoBehaviour
{
    public ExhibitObject exhibitObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            exhibitObject.StealObject();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            exhibitObject.RestoreObject();
        }
    }
}