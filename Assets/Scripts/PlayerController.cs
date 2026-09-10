using Unity.VisualScripting;
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
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleInput();
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpRequested = true;
        }

    }

    void FixedUpdate()
    {
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

        if (isDragging)
        {

        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = (camForward * v + camRight * h).normalized;
    }

    bool IsGrounded()
    {
        // 1. 캐릭터의 콜라이더 영역을 가져와 정확한 '발바닥' 높이를 계산합니다.
        Collider col = GetComponent<Collider>();
        Vector3 rayOrigin = new Vector3(transform.position.x, col.bounds.min.y + 0.1f, transform.position.z);

        // 발바닥에서 시작하므로 레이 길이는 0.3 정도면 바닥을 충분히 감지합니다.
        float rayLength = 0.3f;

        // 선이 캡슐 바깥으로 살짝 튀어나오므로 Scene 뷰(Shaded 모드)에서도 관찰하기 쉽습니다.
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);

        // 2. 바닥 판정 '결과값'을 콘솔에 직접 출력하여 확인합니다.
        bool isHit = Physics.Raycast(rayOrigin, Vector3.down, rayLength);
        Debug.Log("바닥 판정 결과: " + isHit);

        return isHit;
    }
}
