using UnityEngine;

public class OpeningRagdollKnockback : MonoBehaviour
{
    [Header("Main Character")]
    public Animator animator;
    public CharacterController characterController;
    public Collider mainCollider;
    public Rigidbody mainRigidbody;

    [Header("Ragdoll Parts")]
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;

    [Header("Knockback")]
    public Transform knockbackDirectionSource;
    public float backwardForce = 700f;
    public float upwardForce = 250f;
    public ForceMode forceMode = ForceMode.Impulse;

    private bool ragdolled;

    private void Awake()
    {
        DisableRagdoll();
    }

    public void TriggerRagdollBackward()
    {
        if (ragdolled)
        {
            return;
        }

        ragdolled = true;

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Vector3 backwardDirection = knockbackDirectionSource != null
            ? -knockbackDirectionSource.forward
            : -transform.forward;

        Vector3 force = backwardDirection.normalized * backwardForce + Vector3.up * upwardForce;

        Rigidbody targetBody = FindBestRagdollBody();

        if (targetBody != null)
        {
            targetBody.AddForce(force, forceMode);
        }
    }

    private void DisableRagdoll()
    {
        ragdolled = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }
    }

    private Rigidbody FindBestRagdollBody()
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null && rb.name.ToLower().Contains("hips"))
            {
                return rb;
            }
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null && rb.name.ToLower().Contains("pelvis"))
            {
                return rb;
            }
        }

        if (ragdollRigidbodies != null && ragdollRigidbodies.Length > 0)
        {
            return ragdollRigidbodies[0];
        }

        return null;
    }
}