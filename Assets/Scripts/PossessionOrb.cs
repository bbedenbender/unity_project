using UnityEngine;

public class PossessionOrb : MonoBehaviour
{
    [Header("Targets")]
    public Transform[] targets;
    public float waitAtTargetTime = 2f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float arcHeight = 2f;
    public float arriveDistance = 0.15f;

    [Header("Effects")]
    public Light orbLight;
    public ParticleSystem orbParticles;

    private Transform currentTarget;
    private int currentTargetIndex = 0;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyProgress = 0f;
    private float waitTimer = 0f;

    private bool isMoving = false;

    private void Start()
    {
        if (orbLight == null)
        {
            orbLight = GetComponentInChildren<Light>();
        }

        if (orbParticles == null)
        {
            orbParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (targets != null && targets.Length > 0)
        {
            ChooseTarget(0);
        }
    }

    private void Update()
    {
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            WaitThenJump();
        }
    }

    private void ChooseTarget(int index)
    {
        currentTargetIndex = index;
        currentTarget = targets[currentTargetIndex];

        startPosition = transform.position;
        targetPosition = currentTarget.position;
        journeyProgress = 0f;
        isMoving = true;
    }

    private void MoveToTarget()
    {
        if (currentTarget == null)
        {
            ChooseNextTarget();
            return;
        }

        targetPosition = currentTarget.position;

        float distance = Vector3.Distance(startPosition, targetPosition);
        float journeyLength = Mathf.Max(distance, 0.01f);

        journeyProgress += (moveSpeed / journeyLength) * Time.deltaTime;
        journeyProgress = Mathf.Clamp01(journeyProgress);

        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, journeyProgress);

        float arc = Mathf.Sin(journeyProgress * Mathf.PI) * arcHeight;
        Vector3 arcedPosition = flatPosition + Vector3.up * arc;

        transform.position = arcedPosition;

        if (Vector3.Distance(transform.position, targetPosition) <= arriveDistance || journeyProgress >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;
            waitTimer = waitAtTargetTime;

            NotifyTargetReached(currentTarget);
        }
    }

    private void WaitThenJump()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            ChooseNextTarget();
        }
    }

    private void ChooseNextTarget()
    {
        int nextIndex = currentTargetIndex + 1;

        if (nextIndex >= targets.Length)
        {
            nextIndex = 0;
        }

        ChooseTarget(nextIndex);
    }

    private void NotifyTargetReached(Transform target)
    {
        PossessionTarget possessionTarget = target.GetComponentInParent<PossessionTarget>();

        if (possessionTarget != null)
        {
            possessionTarget.OnOrbArrived();
        }
    }
}