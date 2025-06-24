using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float visionRange;
    [SerializeField] private float visionAngle;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;   //ensure player cant be seen if hiding
    [SerializeField] private float enemySpeed;
    private float currentSpeed;

    [SerializeField] private BaseMovement baseMovement;
    private PlayerController playerController;
    private Animator animator;

    private Rigidbody2D rb;
    private Transform player;

    //patrol points
    [SerializeField] private List<PatrolPoint> patrolPoints;
    [SerializeField] private float waypointDistance = 0.1f;
    private int currentPointIndex = 0;
    private float pauseTimer = 0f;
    private bool isWaiting = false;

    private enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //flip enemy so that their vision is the right direction
        Vector3 localScale = transform.localScale;
        localScale.x = -Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("no GameObject with tag 'Player' found");
        }
    }

    private void Update()
    {
        if (player == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }
        switch (currentState)
        {
            case EnemyState.Patrol:
                animator.SetBool("attacking", false);
                Patrol();
                if (PlayerInSight())
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                animator.SetBool("attacking", true);
                AttackPlayer();
                break;
        }

        currentSpeed = rb.linearVelocityX;
        animator.SetFloat("speed", Mathf.Abs(currentSpeed));
    }

    private bool PlayerInSight()
    {
        if (player == null)
        {
            return false;
        }


        Vector2 facingDirection = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < visionRange)
        {
            float angleBetweenEnemyAndPlayer = Vector2.Angle(facingDirection, directionToPlayer);

            if (angleBetweenEnemyAndPlayer < visionAngle / 2)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, visionRange, obstacleLayer | playerLayer);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        playerController.Spotted();
                        return true;
                    }
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector2 facingDirection = transform.localScale.x < 0 ? Vector2.right : Vector2.left;

        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2) * facingDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2) * facingDirection;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
    }

    public void Patrol()
    {
        currentState = EnemyState.Patrol;
        if (isWaiting)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isWaiting = false;
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
            else
            {
                //stop moving while waiting
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }
        }

        PatrolPoint currentPatrolPoint = patrolPoints[currentPointIndex];
        Vector2 direction = (currentPatrolPoint.point.position - transform.position).normalized;

        rb.linearVelocity = new Vector2(direction.x * enemySpeed, rb.linearVelocity.y);

        //flip sprite based on direction
        Vector3 localScale = transform.localScale;
        localScale.x = direction.x >= 0 ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        if (Vector2.Distance(transform.position, currentPatrolPoint.point.position) < waypointDistance)
        {
            isWaiting = true;
            pauseTimer = currentPatrolPoint.pauseTime;
        }

    }

    private void ChasePlayer()
    {
        StartCoroutine(Chasing());
    }

    private IEnumerator Chasing()
    {
        animator.SetBool("killPlayer", true);
        yield return new WaitForSeconds(2f);
        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = new Vector2(direction.x * enemySpeed, rb.linearVelocity.y);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < 1.5f)
        {
            currentState = EnemyState.Attack;
        }
    }

    private void AttackPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        playerController.Die();
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerDeath += ResetToPatrol;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDeath -= ResetToPatrol;
    }

    public void ResetToPatrol()
    {
        currentState = EnemyState.Patrol;
        animator.SetBool("attacking", false);
        rb.linearVelocity = Vector2.zero;
    }

}
