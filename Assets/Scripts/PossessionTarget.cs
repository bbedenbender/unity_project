using UnityEngine;

public class PossessionTarget : MonoBehaviour
{
    public string targetId;
    public string displayName;

    public bool isPossessed = false;

    public Renderer[] renderersToTint;
    public Color normalColor = Color.white;
    public Color possessedColor = new Color(0.6f, 0f, 1f);

    public Light possessionLight;

    public void OnOrbArrived()
    {
        Debug.Log("Orb arrived at: " + displayName);

        // For now, just mark as suspicious.
        // Later this can trigger full possession.
        SetSuspiciousVisual();
    }

    public void Possess()
    {
        isPossessed = true;

        if (possessionLight != null)
        {
            possessionLight.enabled = true;
        }

        SetColor(possessedColor);

        Debug.Log(displayName + " is possessed.");
    }

    public void Release()
    {
        isPossessed = false;

        if (possessionLight != null)
        {
            possessionLight.enabled = false;
        }

        SetColor(normalColor);

        Debug.Log(displayName + " released.");
    }

    private void SetSuspiciousVisual()
    {
        if (!isPossessed)
        {
            SetColor(possessedColor * 0.6f);
        }
    }

    private void SetColor(Color color)
    {
        if (renderersToTint == null)
        {
            return;
        }

        foreach (Renderer rend in renderersToTint)
        {
            if (rend == null)
            {
                continue;
            }

            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    mat.color = color;
                }
            }
        }
    }
}