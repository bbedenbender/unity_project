using UnityEngine;
using UnityEngine.AI;

public class BookMonsterAI : MonoBehaviour
{
    public enum MonsterState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float losePlayerRange = 15f;

    [Header("Movement")]
    public float patrolSpeed = 1.8f;
    public float chaseSpeed = 3.2f;
    public float waitAtPointTime = 1.5f;
    public float patrolStoppingDistance = 0.25f;
    public float chaseStoppingDistance = 1.25f;
    public bool useCrawlWhenChasing = true;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 1;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";
    public string crawlParameter = "Crawl";
    public string attackTrigger = "";
    public float animationSpeedChangeRate = 6f;

    private NavMeshAgent agent;
    private MonsterState currentState = MonsterState.Patrol;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private float animatorSpeed = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("BookMonsterAI requires a NavMeshAgent on BookHeadMonster_ROOT.");
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BookMonsterAI: NavMeshAgent is not on a NavMesh. Move BookHeadMonster_ROOT onto the baked NavMesh.");
            return;
        }

        agent.stoppingDistance = patrolStoppingDistance;

        if (patrolPoints != null && patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            UpdateAnimation(false);
            return;
        }

        float distanceToPlayer = player != null
            ? Vector3.Distance(transform.position, player.position)
            : Mathf.Infinity;

        if (player != null && distanceToPlayer <= attackRange)
        {
            currentState = MonsterState.Attack;
        }
        else if (player != null && distanceToPlayer <= detectionRange)
        {
            currentState = MonsterState.Chase;
        }
        else if (player == null || distanceToPlayer >= losePlayerRange)
        {
            currentState = MonsterState.Patrol;
        }

        switch (currentState)
        {
            case MonsterState.Patrol:
                Patrol();
                break;

            case MonsterState.Chase:
                ChasePlayer();
                break;

            case MonsterState.Attack:
                AttackPlayer();
                break;
        }

        bool shouldAnimateMoving = ShouldAnimateMoving();
        UpdateAnimation(shouldAnimateMoving);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = patrolStoppingDistance;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtPointTime)
            {
                GoToNextPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform nextPoint = patrolPoints[currentPatrolIndex];

        if (nextPoint != null)
        {
            if (NavMesh.SamplePosition(nextPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("BookMonsterAI: Patrol point is not near a NavMesh: " + nextPoint.name);
            }
        }

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            currentState = MonsterState.Patrol;
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStoppingDistance;

        if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (player == null)
        {
            currentState = MonsterState.Patrol;
            return;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        FacePlayer();

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;

            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
                animator.SetTrigger(attackTrigger);

            Debug.Log("Book monster attacked the player for " + attackDamage + " damage.");
        }
    }

    private bool ShouldAnimateMoving()
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (agent.isStopped || currentState == MonsterState.Attack)
            return false;

        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            return false;

        return agent.desiredVelocity.sqrMagnitude > 0.01f || agent.velocity.sqrMagnitude > 0.01f;
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    private void UpdateAnimation(bool moving)
    {
        if (animator == null)
            return;

        float targetSpeed = moving ? 1f : 0f;
        animatorSpeed = Mathf.MoveTowards(
            animatorSpeed,
            targetSpeed,
            animationSpeedChangeRate * Time.deltaTime
        );

        if (!string.IsNullOrEmpty(speedParameter))
            animator.SetFloat(speedParameter, animatorSpeed);

        if (!string.IsNullOrEmpty(crawlParameter))
        {
            bool shouldCrawl = useCrawlWhenChasing && currentState == MonsterState.Chase;
            animator.SetBool(crawlParameter, shouldCrawl);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}