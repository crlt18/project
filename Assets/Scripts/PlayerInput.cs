using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{

    [SerializeField] private float playerSpeed;
    private float currentSpeed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D rb;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(currentSpeed, jumpForce);
        }
    }

    private void Update()
    {
        currentSpeed = rb.linearVelocityX;
    }

}

