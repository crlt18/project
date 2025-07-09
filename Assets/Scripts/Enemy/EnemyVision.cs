using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private PlayerController playerController;
    private Transform player;

    [SerializeField] private float visionRange;
    [SerializeField] private float visionAngle;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private float sweepSpeed = 60f;
    [SerializeField] private float sweepBaseAngle;
    private float currentSweepAngle;

    [SerializeField] private List<VisionStopPoint> visionStopPoints;
    private int visionIndex;
    private bool isVisionWaiting = false;
    private float visionPauseTimer = 0f;
    private bool visionForward = true;

    private void Awake()
    {
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
        if (visionStopPoints.Count > 0)
        {
            HandleVisionStopPoints();
        }
    }
    private void HandleVisionStopPoints()
    {
        VisionStopPoint currentStop = visionStopPoints[visionIndex];

       
        Vector2 dir = (currentStop.point.position - transform.position).normalized;  //calculate target angle based on direction to the stop point
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

       
        sweepBaseAngle = Mathf.LerpAngle(sweepBaseAngle, targetAngle, Time.deltaTime * sweepSpeed / 10f);    //smoothly rotate sweepBaseAngle toward the target
        currentSweepAngle = sweepBaseAngle;

        if (Mathf.Abs(Mathf.DeltaAngle(sweepBaseAngle, targetAngle)) < 1f && !isVisionWaiting)
        {
            isVisionWaiting = true;
            visionPauseTimer = currentStop.pauseTime;
        }

        if (isVisionWaiting)
        {
            visionPauseTimer -= Time.deltaTime;
            if (visionPauseTimer <= 0f)
            {
                isVisionWaiting = false;
                if (visionForward)
                {
                    visionIndex++;
                    if (visionIndex >= visionStopPoints.Count)
                    {
                        visionIndex = visionStopPoints.Count - 2;
                        visionForward = false;
                    }
                }
                else
                {
                    visionIndex--;
                    if (visionIndex < 0)
                    {
                        visionIndex = 1;
                        visionForward = true;
                    }
                }

            }
        }
    }



    public bool PlayerInSight()
    {
        if (player == null)
        {
            return false;
        }

        Vector2 facingDirection;

        float angleInRadians = currentSweepAngle * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Vector2 directionToPlayer = player.position - transform.position;

        directionToPlayer = directionToPlayer.normalized;

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

        Vector2 facingDirection = new Vector2(Mathf.Cos(currentSweepAngle * Mathf.Deg2Rad), Mathf.Sin(currentSweepAngle * Mathf.Deg2Rad));

        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2) * facingDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2) * facingDirection;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
    }
}
