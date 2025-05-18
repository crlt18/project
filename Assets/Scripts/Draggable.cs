using UnityEngine;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;

    public Transform startPosition;
    public Transform targetPosition;
    public float snapDistance = 0.5f;
    private void Awake()
    {
        mainCamera = Camera.main;
        transform.position = startPosition.position;
    }

    private void Update()
    {
        // get current mouse position in world space
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        // start drag on left click press
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                isDragging = true;
                offset = transform.position - mouseWorldPos;
            }
        }

        // stop drag on left click release
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);
            if (distanceToTarget <= snapDistance)
            {
                transform.position = targetPosition.position;
            }
        }

        if (isDragging)
        {
            Vector3 targetPos = mouseWorldPos + offset;

            Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.3f, LayerMask.GetMask("Walls"));
            if (hit == null)  // no wall collision, safe to move
            {
                transform.position = targetPos;
            }
        }
    }
}
