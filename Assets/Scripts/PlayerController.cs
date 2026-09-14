using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float fallingSpeed = 3f;
    [SerializeField] private float jumpForce = 20f;

    private Rigidbody rb;
    private Transform cameraTransform;

    private Vector3 moveDir;
    private bool isDragging = false;
    private bool jumpRequested = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (StageManager.Instance != null && StageManager.Instance.CurrentState != StageManager.GameState.Exploration)
        {
            moveDir = Vector3.zero;
            jumpRequested = false;
            return;
        }

        HandleInput();
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (StageManager.Instance != null && StageManager.Instance.CurrentState != StageManager.GameState.Exploration)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
        }

        float targetYVelocity = rb.linearVelocity.y;

        if (jumpRequested)
        {
            targetYVelocity = jumpForce;
            jumpRequested = false;
        }
        else if (targetYVelocity < 0)
        {
            targetYVelocity += Physics.gravity.y * fallingSpeed * Time.fixedDeltaTime;
        }

        if (moveDir != Vector3.zero)
        {
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, targetYVelocity, moveDir.z * moveSpeed);

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0, targetYVelocity, 0);
        }
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * v + camRight * h).normalized;
        }
        else
        {
            moveDir = new Vector3(h, 0, v).normalized;
        }
    }

    bool IsGrounded()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return true;

        Vector3 rayOrigin = new Vector3(transform.position.x, col.bounds.min.y + 0.1f, transform.position.z);
        float rayLength = 0.3f;
        return Physics.Raycast(rayOrigin, Vector3.down, rayLength);
    }
}
