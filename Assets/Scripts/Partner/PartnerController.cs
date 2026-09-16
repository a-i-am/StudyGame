using System.Collections;
using UnityEngine;
using StudyGame.Combat;

namespace StudyGame.Partner
{
    [RequireComponent(typeof(CharacterController))]
    public class PartnerController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform targetPlayer;
        [SerializeField] private float followDistance = 2.2f;
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 5f;

        [Header("Weapon & Combat")]
        [SerializeField] private WeaponController weaponController;

        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (weaponController == null)
            {
                weaponController = GetComponentInChildren<WeaponController>();
            }
        }

        public void SetTargetPlayer(Transform playerTransform)
        {
            targetPlayer = playerTransform;
            if (targetPlayer != null)
            {
                // Snap to player initial location offset
                Vector3 initialPos = targetPlayer.position - targetPlayer.forward * followDistance;
                characterController.enabled = false;
                transform.position = initialPos;
                characterController.enabled = true;
            }
        }

        private void Update()
        {
            if (targetPlayer == null) return;

            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            Vector3 targetPosition = targetPlayer.position - (targetPlayer.forward * followDistance);
            Vector3 distanceDelta = targetPosition - transform.position;
            distanceDelta.y = 0; // Ignore vertical height difference forXZ distance calculation

            float distance = distanceDelta.magnitude;

            if (distance > 0.5f)
            {
                Vector3 moveDir = distanceDelta.normalized;

                // Match rotation to movement direction or player facing
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                float currentSpeed = (distance > followDistance * 2f) ? moveSpeed * 1.5f : moveSpeed;
                characterController.Move(moveDir * currentSpeed * Time.deltaTime);
            }
            else
            {
                // Rotate smoothly to face the same direction as the player when idle
                transform.rotation = Quaternion.Slerp(transform.rotation, targetPlayer.rotation, rotationSpeed * Time.deltaTime);
            }

            // Sync Jump if player is significantly higher and partner is grounded
            if (isGrounded && (targetPlayer.position.y - transform.position.y > 1.2f))
            {
                velocity.y = jumpForce;
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        public void PerformComboAttack()
        {
            if (weaponController != null)
            {
                weaponController.PerformSwing();
            }
        }

        public WeaponController GetWeaponController()
        {
            return weaponController;
        }
    }
}
