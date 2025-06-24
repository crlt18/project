using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{

    public static event System.Action OnPlayerDeath;

    [SerializeField] private BaseMovement baseMovement;
    private float playerSpeed;
    [SerializeField] private float originalSpeed;
    private float currentSpeed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D rb;
    private float deadZone = 0.05f; //prevent flickering (player changing direction for a frame when coming to a stop)
    private bool jumpRequested = false;
    [HideInInspector] public bool canBackstab;
    [HideInInspector] public GameObject enemyInRange;

    [SerializeField] private Transform ceilingCheckPoint; 
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.9f, 0.1f); 
    [SerializeField] private LayerMask ceilingLayerMask; 

    private CapsuleCollider2D capsuleCollider;
    private Vector2 standingSize;
    private Vector2 crouchingSize;
    private Vector2 standingOffset;
    private Vector2 crouchingOffset;

    private bool isSpotted = false;
    private bool isCrouching = false;

    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("found animator");
        }
        standingSize = capsuleCollider.size;
        standingOffset = capsuleCollider.offset;

        crouchingSize = new Vector2(standingSize.x, standingSize.y / 2f);
        crouchingOffset = new Vector2(standingOffset.x, standingOffset.y - standingSize.y / 4f);
        playerSpeed = originalSpeed;
    }

    private void FixedUpdate()
    {
        if (!isSpotted)
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

            if (jumpRequested)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpRequested = false;
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
            if (Input.GetKey(KeyCode.LeftControl) && baseMovement.IsGrounded())
            {
                capsuleCollider.size = crouchingSize;
                capsuleCollider.offset = crouchingOffset;
                isCrouching = true;
            }
            else if (isCrouching & CanStandUp())
            {
                capsuleCollider.size = standingSize;
                capsuleCollider.offset = standingOffset;
                isCrouching = false;
            }

            baseMovement.SlopeCheck();
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
        animator.SetFloat("speed", Mathf.Abs(currentSpeed));
        animator.SetBool("grounded", baseMovement.IsGrounded());

        if (!isSpotted)
        {
            //jump
            if (Input.GetKeyDown(KeyCode.Space) && baseMovement.IsGrounded())
            {

                jumpRequested = true;
   
            }

            //go down
            if (Input.GetKeyDown(KeyCode.S))
            {
                DropDown();
            }

            if (canBackstab && Input.GetMouseButtonDown(0))
            {
                Destroy(enemyInRange);
            }
        }
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

    public void Spotted()
    {
        isSpotted = true;
        animator.SetBool("spotted", true);
    }

    public void Die()
    {
        StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        animator.SetBool("dead", true);
        yield return new WaitForSeconds(2f);
        OnPlayerDeath?.Invoke();
        Destroy(gameObject);    
    }

    private void DropDown()
    {
        StartCoroutine(DropThroughPlatform());
    }

    private IEnumerator DropThroughPlatform()
    {
        gameObject.layer = LayerMask.NameToLayer("PlayerThroughPlatform");

        yield return new WaitForSeconds(0.5f);

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private bool CanStandUp()
    {
        return !Physics2D.OverlapBox(ceilingCheckPoint.position, ceilingCheckSize, 0f, ceilingLayerMask);
    }
}

