using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private PlayerController playerController;
    private Transform player;

    [SerializeField] private float visionRange;
    [SerializeField] private float visionAngle;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private bool useSweepingVision = false;
    [SerializeField] private float sweepSpeed = 60f;
    [SerializeField] private float maxSweepOffset = 45f;
    [SerializeField] private float sweepBaseAngle;
    private float currentSweepAngle;

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
        if (useSweepingVision)
        {
            float sweepOffset = Mathf.PingPong(Time.time * sweepSpeed, maxSweepOffset * 2f) - maxSweepOffset;
            currentSweepAngle = sweepBaseAngle + sweepOffset;
        }
    }

    public bool PlayerInSight()
    {
        if (player == null)
        {
            return false;
        }


        Vector2 facingDirection = useSweepingVision
    ? new Vector2(Mathf.Cos(currentSweepAngle * Mathf.Deg2Rad), Mathf.Sin(currentSweepAngle * Mathf.Deg2Rad))
    : (transform.localScale.x < 0 ? Vector2.right : Vector2.left);
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

        Vector2 facingDirection = useSweepingVision
    ? new Vector2(Mathf.Cos(currentSweepAngle * Mathf.Deg2Rad), Mathf.Sin(currentSweepAngle * Mathf.Deg2Rad))
    : (transform.localScale.x < 0 ? Vector2.right : Vector2.left);

        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2) * facingDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2) * facingDirection;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
    }

}
