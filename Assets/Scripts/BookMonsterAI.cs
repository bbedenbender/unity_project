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
    public bool useCrawlWhenChasing = true;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 1;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";
    public string crawlParameter = "Crawl";
    public string attackTrigger = "Attack";

    private NavMeshAgent agent;
    private MonsterState currentState = MonsterState.Patrol;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("BookMonsterAI requires a NavMeshAgent on the same GameObject as this script.");
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BookMonsterAI: NavMeshAgent is not on a NavMesh. Move BookHeadMonster_ROOT onto the baked blue NavMesh area.");
            UpdateAnimation(0f);
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            UpdateAnimation(0f);
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

        float currentSpeed = agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
        UpdateAnimation(currentSpeed);
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

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
            NavMeshHit hit;

            if (NavMesh.SamplePosition(nextPoint.position, out hit, 2f, NavMesh.AllAreas))
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
        {
            currentPatrolIndex = 0;
        }
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

        NavMeshHit hit;

        if (NavMesh.SamplePosition(player.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(player.position);
        }
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
            {
                animator.SetTrigger(attackTrigger);
            }

            Debug.Log("Book monster attacked the player for " + attackDamage + " damage.");
        }
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

    private void UpdateAnimation(float speed)
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(speedParameter))
        {
            animator.SetFloat(speedParameter, speed);
        }

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