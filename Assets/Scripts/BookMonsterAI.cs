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

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 1;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";
    public string crawlParameter = "Crawl";
    public string attackTrigger = "Attack";
    public bool crawlWhenChasing = true;

    private NavMeshAgent agent;
    private MonsterState currentState = MonsterState.Patrol;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("BookMonsterAI needs a NavMeshAgent on BookHeadMonster_ROOT.");
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("BookHeadMonster_ROOT is not on a baked NavMesh. Move it onto the blue NavMesh floor.");
            return;
        }

        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (player == null)
        {
            currentState = MonsterState.Patrol;
            Patrol();
            UpdateAnimation();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = MonsterState.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = MonsterState.Chase;
        }
        else if (distanceToPlayer >= losePlayerRange)
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

        UpdateAnimation();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

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
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform nextPoint = patrolPoints[currentPatrolIndex];

        if (nextPoint != null)
            agent.SetDestination(nextPoint.position);

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;
    }

    private void ChasePlayer()
    {
        if (player == null)
            return;

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (player == null)
            return;

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

    private void FacePlayer()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        if (!string.IsNullOrEmpty(speedParameter))
            animator.SetFloat(speedParameter, agent.velocity.magnitude);

        if (!string.IsNullOrEmpty(crawlParameter))
            animator.SetBool(crawlParameter, crawlWhenChasing && currentState == MonsterState.Chase);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}