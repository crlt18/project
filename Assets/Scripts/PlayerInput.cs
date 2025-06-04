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
    float deadZone = 0.05f; //prevent flickering (player changing direction for a frame when coming to a stop)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSpeed = originalSpeed;
    }

    private void FixedUpdate()
    {
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

        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(currentSpeed, jumpForce);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed = originalSpeed * 1.3f;
        }
        else
        {
            playerSpeed = originalSpeed;
        }
    }

    private void Update()
    {
        currentSpeed = rb.linearVelocityX;

        //logic to flip the player sprite depending on direction
        Vector3 localScale = transform.localScale;
        if (currentSpeed < -deadZone)
        {
            localScale.x = -Mathf.Abs(localScale.x);
        }
        else if (currentSpeed > deadZone) 
        {
            localScale.x = Mathf.Abs(localScale.x);
        }
        transform.localScale = localScale;
    }

}

