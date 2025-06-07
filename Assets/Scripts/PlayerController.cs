using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BaseMovement baseMovement;
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
            if (baseMovement.IsGrounded())
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
        if (Input.GetKey(KeyCode.S) && baseMovement.IsGrounded())
        {
            capsuleCollider.size = crouchingSize;
            capsuleCollider.offset = crouchingOffset;
        }
        else
        {
            capsuleCollider.size = standingSize;
            capsuleCollider.offset = standingOffset;
        }

        baseMovement.SlopeCheck();

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
        animator.SetBool("grounded", baseMovement.IsGrounded());
    }

    private void MovePlayer(float playerSpeed)
    {
        if (baseMovement.IsGrounded() && baseMovement.isOnSlope)
        {
            rb.linearVelocity = new Vector2(-playerSpeed * baseMovement.slopeNormalPerp.x , -playerSpeed * baseMovement.slopeNormalPerp.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(playerSpeed, rb.linearVelocity.y);
        }  
        
    }


}

