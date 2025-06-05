using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
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
            rb.linearVelocity = new Vector2(-playerSpeed, rb.linearVelocity.y);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(playerSpeed, rb.linearVelocity.y);
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
        animator.speed = currentSpeed;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

}

