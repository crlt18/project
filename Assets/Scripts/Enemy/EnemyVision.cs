using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [SerializeField] private Light2D visionLight;

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

        if (visionLight != null)
        {
            visionLight.pointLightInnerAngle = visionAngle * 0.8f;
            visionLight.pointLightOuterAngle = visionAngle;
            visionLight.pointLightOuterRadius = visionRange;
            visionLight.transform.rotation = Quaternion.Euler(0, 0, currentSweepAngle - 90f);
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

        if (visionStopPoints.Count > 0) 
        {
            float angleInRadians = currentSweepAngle * Mathf.Deg2Rad;
            facingDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        }
        else
        {
            facingDirection = (transform.localScale.x < 0) ? Vector2.right : Vector2.left;
        }

        Vector2 directionToPlayer = player.position - transform.position;


        directionToPlayer = directionToPlayer.normalized;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < visionRange)
        {
            float angleBetweenEnemyAndPlayer = Vector2.Angle(facingDirection, directionToPlayer);

            if (angleBetweenEnemyAndPlayer < visionAngle / 2)
            {
                RaycastHit2D hit = Physics2D.Raycast((transform.position + new Vector3(0,0.5f,0)), directionToPlayer, visionRange, obstacleLayer | playerLayer);
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

        Vector2 origin = (Vector2)(transform.position + new Vector3(0, 0.5f, 0));

        float centerAngle;
        if (visionStopPoints != null && visionStopPoints.Count > 0)
        {
            centerAngle = currentSweepAngle;
        }
        else
        {
            centerAngle = (transform.localScale.x < 0) ? 0f : 180f;
        }

        float halfVision = visionAngle / 2f;

        Vector2 leftDir = new Vector2(Mathf.Cos((centerAngle + halfVision) * Mathf.Deg2Rad),
                                      Mathf.Sin((centerAngle + halfVision) * Mathf.Deg2Rad));

        Vector2 rightDir = new Vector2(Mathf.Cos((centerAngle - halfVision) * Mathf.Deg2Rad),
                                       Mathf.Sin((centerAngle - halfVision) * Mathf.Deg2Rad));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine((Vector3)origin, (Vector3)(origin + leftDir * visionRange));
        Gizmos.DrawLine((Vector3)origin, (Vector3)(origin + rightDir * visionRange));

        Vector2 centerDir = new Vector2(Mathf.Cos(centerAngle * Mathf.Deg2Rad), Mathf.Sin(centerAngle * Mathf.Deg2Rad));
        Gizmos.color = Color.green;
        Gizmos.DrawLine((Vector3)origin, (Vector3)(origin + centerDir * visionRange));
    }

}
