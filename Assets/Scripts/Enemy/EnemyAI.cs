using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;   //ensure player cant be seen if hiding
    [SerializeField] private float enemySpeed;
    private float currentSpeed;

    [SerializeField] private BaseMovement baseMovement;
    private PlayerController playerController;
    private Animator animator;

    private Rigidbody2D rb;
    private Transform player;

    private EnemyVision enemyVision;

    //patrol points
    [SerializeField] private List<PatrolPoint> patrolPoints;
    [SerializeField] private float waypointDistance = 0.1f;
    private int currentPointIndex = 0;
    private float pauseTimer = 0f;
    private bool isWaiting = false;
    private bool isChasing = false;


    private enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyVision = GetComponent<EnemyVision>();

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
                if (enemyVision.PlayerInSight())
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                animator.SetBool("runToPlayer", false);
                animator.SetBool("attacking", true);
                AttackPlayer();
                break;
        }

        currentSpeed = rb.linearVelocityX;
        animator.SetFloat("speed", Mathf.Abs(currentSpeed));
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
        if (!isChasing)
        {
            StartCoroutine(Chasing());
        }
    }


    private IEnumerator Chasing()
    {
        isChasing = true;
        animator.SetBool("killPlayer", true);
        yield return new WaitForSeconds(2f);
        Vector2 direction = (player.position - transform.position).normalized;
        animator.SetBool("runToPlayer", true);
        animator.SetBool("killPlayer", false);

        rb.linearVelocity = new Vector2(direction.x * enemySpeed, rb.linearVelocity.y);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < 1.5f)
        {
            currentState = EnemyState.Attack;
        }
        if (currentState == EnemyState.Patrol)
        {
            isChasing = false;
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
