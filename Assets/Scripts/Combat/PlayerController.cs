using UnityEngine;

namespace StudyGame.Runtime.Combat
{
    /// <summary>
    /// 빠르고 즉각적인 핵 앤 슬래시 조작을 위한 플레이어 컨트롤러
    /// 카메라 시점에 상대적인 WASD 이동과 우클릭 대시를 지원합니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float dashSpeed = 15f;
        public float dashDuration = 0.2f;
        
        private CharacterController controller;
        private Camera mainCamera;
        private bool isDashing = false;
        private float dashTimer = 0f;
        private Vector3 dashDirection;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (isDashing)
            {
                HandleDash();
                return;
            }

            HandleMovement();
            HandleRotation();
            
            // 우클릭 또는 좌측 Shift로 대시
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartDash();
            }
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (mainCamera == null) return;

            // 카메라가 바라보는 방향 기준 이동
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h).normalized;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            if (h != 0 || v != 0)
            {
                Vector3 camForward = mainCamera.transform.forward;
                Vector3 camRight = mainCamera.transform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();
                
                Vector3 moveDir = (camForward * v + camRight * h).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.deltaTime);
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = transform.forward;
        }

        private void HandleDash()
        {
            dashTimer -= Time.deltaTime;
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
    }
}
