using System.Collections;
using UnityEngine;
using StudyGame.Combat;
using StudyGame.Partner;

namespace StudyGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 6.5f;
        public float rotationSpeed = 12f;
        public float gravity = -18f;
        
        [Header("Jump Settings")]
        public float jumpForce = 7.5f;
        public float doubleJumpForce = 7.0f;
        public int maxJumps = 2;

        [Header("Dash Settings")]
        public float dashSpeed = 18f;
        public float dashDuration = 0.22f;
        public float dashCooldown = 0.9f;

        [Header("Combat & Companion")]
        public WeaponController weaponController;
        public PartnerController partnerController;

        private CharacterController characterController;
        private bool isMovementEnabled = false;
        private Vector3 velocity;
        private int jumpCount = 0;
        
        private bool isDashing = false;
        private float lastDashTime = -999f;
        private Vector3 lastMoveDir = Vector3.forward;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (weaponController == null)
            {
                weaponController = GetComponentInChildren<WeaponController>();
            }
        }

        private void Update()
        {
            if (!isMovementEnabled) return;

            HandleGroundingAndGravity();
            HandleInput();
        }

        private void HandleGroundingAndGravity()
        {
            if (characterController.isGrounded)
            {
                if (velocity.y < 0)
                {
                    velocity.y = -2f;
                }
                jumpCount = 0;
            }
        }

        private void HandleInput()
        {
            // 1. Dash Input (LeftShift or Right Click)
            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && CanDash())
            {
                StartCoroutine(PerformDashRoutine());
                return;
            }

            if (isDashing) return;

            // 2. Jump & Double Jump Input
            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
            {
                velocity.y = (jumpCount == 0) ? jumpForce : doubleJumpForce;
                jumpCount++;
                TriggerJumpVisualEffect(jumpCount);
            }

            // 3. Attack Input (Left Click)
            if (Input.GetMouseButtonDown(0))
            {
                // Check if not clicking UI
                if (UnityEngine.EventSystems.EventSystem.current == null ||
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    PerformAttack();
                }
            }

            // 4. Movement Calculation
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                Vector3 moveDir = inputDirection;

                if (Camera.main != null)
                {
                    float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                    float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }
                else
                {
                    Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                lastMoveDir = moveDir.normalized;
                characterController.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private bool CanDash()
        {
            return !isDashing && (Time.time - lastDashTime >= dashCooldown);
        }

        private IEnumerator PerformDashRoutine()
        {
            isDashing = true;
            lastDashTime = Time.time;

            Vector3 dashDirection = lastMoveDir;
            float elapsed = 0f;

            // Flatten vertical velocity during dash
            velocity.y = 0f;

            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
                yield return null;
            }

            isDashing = false;
        }

        private void PerformAttack()
        {
            if (weaponController != null)
            {
                weaponController.PerformSwing();
            }

            if (partnerController != null)
            {
                partnerController.PerformComboAttack();
            }
        }

        private void TriggerJumpVisualEffect(int jumpIndex)
        {
            // Subtle stretch effect on jump
            StartCoroutine(SquashAndStretchRoutine(jumpIndex == 2));
        }

        private IEnumerator SquashAndStretchRoutine(bool isDoubleJump)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 stretchScale = isDoubleJump ? 
                new Vector3(originalScale.x * 0.8f, originalScale.y * 1.3f, originalScale.z * 0.8f) : 
                new Vector3(originalScale.x * 0.9f, originalScale.y * 1.15f, originalScale.z * 0.9f);

            transform.localScale = stretchScale;
            yield return new WaitForSeconds(0.12f);
            transform.localScale = originalScale;
        }

        public void SetMovementEnabled(bool isEnabled)
        {
            isMovementEnabled = isEnabled;
            if (!isEnabled)
            {
                velocity = Vector3.zero;
            }
        }

        public void SetPartner(PartnerController partner)
        {
            partnerController = partner;
            if (partnerController != null)
            {
                partnerController.SetTargetPlayer(transform);
            }
        }
    }
}
