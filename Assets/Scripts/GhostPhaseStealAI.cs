using UnityEngine;

public class GhostPhaseStealAI : MonoBehaviour
{
    public ExhibitObject[] possibleTargets;
    public Transform hidePoint;

    [Header("Ghost Visuals")]
    public GameObject mummyModel;
    public GameObject soldierModel;

    [Tooltip("If true, the vase is always stolen by the mummy.")]
    public bool forceMummyForVase = true;

    [Tooltip("If true, non-vase steals alternate between mummy and soldier.")]
    public bool alternateGhostEachSteal = true;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float stealDistance = 1.5f;
    public float waitBeforeSteal = 1.0f;

    private ExhibitObject currentTarget;
    private float stealTimer;
    private bool useMummyNext = true;

    private enum GhostState
    {
        ChooseTarget,
        MoveToTarget,
        Steal,
        Flee
    }

    private enum GhostVisual
    {
        Mummy,
        Soldier
    }

    private GhostState state = GhostState.ChooseTarget;
    private GhostVisual activeVisual = GhostVisual.Mummy;

    private void Start()
    {
        SetGhostVisual(activeVisual);
    }

    private void Update()
    {
        switch (state)
        {
            case GhostState.ChooseTarget:
                ChooseTarget();
                break;

            case GhostState.MoveToTarget:
                MoveToTarget();
                break;

            case GhostState.Steal:
                StealTarget();
                break;

            case GhostState.Flee:
                Flee();
                break;
        }
    }

    private void ChooseTarget()
    {
        currentTarget = FindAvailableTarget();

        if (currentTarget == null)
        {
            return;
        }

        ChooseVisualForTarget(currentTarget);

        currentTarget.MarkTargeted(true);
        state = GhostState.MoveToTarget;
    }

    private ExhibitObject FindAvailableTarget()
    {
        foreach (ExhibitObject exhibit in possibleTargets)
        {
            if (exhibit != null && !exhibit.isStolen)
            {
                return exhibit;
            }
        }

        return null;
    }

    private void ChooseVisualForTarget(ExhibitObject target)
    {
        if (forceMummyForVase && target.objectId == "vase")
        {
            activeVisual = GhostVisual.Mummy;
        }
        else
        {
            activeVisual = useMummyNext ? GhostVisual.Mummy : GhostVisual.Soldier;

            if (alternateGhostEachSteal)
            {
                useMummyNext = !useMummyNext;
            }
        }

        SetGhostVisual(activeVisual);
    }

    private void MoveToTarget()
    {
        if (currentTarget == null)
        {
            state = GhostState.ChooseTarget;
            return;
        }

        if (currentTarget.isStolen)
        {
            state = GhostState.ChooseTarget;
            return;
        }

        Vector3 targetPosition = currentTarget.transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        RotateToward(targetPosition);

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= stealDistance)
        {
            stealTimer = waitBeforeSteal;
            state = GhostState.Steal;
        }
    }

    private void StealTarget()
    {
        if (currentTarget == null)
        {
            state = GhostState.ChooseTarget;
            return;
        }

        stealTimer -= Time.deltaTime;

        if (stealTimer <= 0f)
        {
            currentTarget.StealObject();
            state = GhostState.Flee;
        }
    }

    private void Flee()
    {
        if (hidePoint == null)
        {
            currentTarget = null;
            state = GhostState.ChooseTarget;
            return;
        }

        Vector3 targetPosition = hidePoint.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        RotateToward(targetPosition);

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= 0.5f)
        {
            currentTarget = null;
            state = GhostState.ChooseTarget;
        }
    }

    private void RotateToward(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void SetGhostVisual(GhostVisual visual)
    {
        if (mummyModel != null)
        {
            mummyModel.SetActive(visual == GhostVisual.Mummy);
        }

        if (soldierModel != null)
        {
            soldierModel.SetActive(visual == GhostVisual.Soldier);
        }
    }
}