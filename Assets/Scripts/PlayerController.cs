using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{

    private float playerSpeed;
    [SerializeField] private float originalSpeed;
    private float currentSpeed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D rb;
    private float deadZone = 0.05f; //prevent flickering (player changing direction for a frame when coming to a stop)

    private CapsuleCollider2D capsuleCollider;
    private Vector2 standingSize;
    private Vector2 crouchingSize;
    private Vector2 standingOffset;
    private Vector2 crouchingOffset;

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private Animator animator;

    [SerializeField] private float slopeCheckDistance;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private float slopeSideAngle;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        standingSize = capsuleCollider.size;
        standingOffset = capsuleCollider.offset;

        crouchingSize = new Vector2(standingSize.x, standingSize.y / 2f);
        crouchingOffset = new Vector2(standingOffset.x, standingOffset.y - standingSize.y / 4f);
        playerSpeed = originalSpeed;
    }

    private void FixedUpdate()
    {
        //movement
        if (Input.GetKey(KeyCode.A))
        {
            MovePlayer(-playerSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            MovePlayer(playerSpeed);
        }
        else
        {
            //if there is no input reduce sliding from momentum
            float friction = 10f;  
            float newVelX = Mathf.MoveTowards(rb.linearVelocityX, 0, friction * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        }

        //jump
        if (Input.GetKey(KeyCode.Space))
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(currentSpeed, jumpForce);
            }
        }

        //sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed = originalSpeed * 1.3f;
        }
        else
        {
            playerSpeed = originalSpeed;
        }

        //crouch
        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            capsuleCollider.size = crouchingSize;
            capsuleCollider.offset = crouchingOffset;
        }
        else
        {
            capsuleCollider.size = standingSize;
            capsuleCollider.offset = standingOffset;
        }

        SlopeCheck();

    }

    private void Update()
    {
        currentSpeed = rb.linearVelocityX;
        //logic to flip the player sprite depending on direction
        Vector3 localScale = transform.localScale;
        if (currentSpeed < -deadZone)
        {
            localScale.x = Mathf.Abs(localScale.x);
        }
        else if (currentSpeed > deadZone) 
        {
            localScale.x = -Mathf.Abs(localScale.x);
        }
        transform.localScale = localScale;
        animator.SetFloat("speed", Mathf.Abs(currentSpeed));
        animator.SetBool("grounded", IsGrounded());
    }

    private void MovePlayer(float playerSpeed)
    {

        if (IsGrounded() && isOnSlope)
        {
            rb.linearVelocity = new Vector2(-playerSpeed * slopeNormalPerp.x , -playerSpeed * slopeNormalPerp.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(playerSpeed, rb.linearVelocity.y);
        }  
        
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = groundCheckPoint.position;

        SlopeCheckVertical(checkPos);
        SlopeCheckHorizontal(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, groundLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, groundLayer);

        if(slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope= false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, groundLayer);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != slopeDownAngleOld)
            {
                isOnSlope = true;
            }
            else
            {
                isOnSlope = false;
            }
                slopeDownAngleOld = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.green);
            Debug.DrawRay(hit.point, hit.normal, Color.red);
        }
    }

}

