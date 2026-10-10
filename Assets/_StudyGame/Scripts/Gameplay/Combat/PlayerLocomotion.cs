using UnityEngine;
using UnityEngine.InputSystem;

namespace StudyGame.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerLocomotion : MonoBehaviour
    {
        static readonly int SpeedHash = Animator.StringToHash("Speed");

        [SerializeField] InputActionAsset _actions;
        [SerializeField] Transform _cameraTransform;
        [SerializeField] Animator _animator;
        [SerializeField] float _moveSpeed = 5f;
        [SerializeField] float _sprintAddition = 3.5f;
        [SerializeField] float _rotationSharpness = 12f;
        [SerializeField] float _gravity = 9.8f;
        [SerializeField] float _dashDistance = 5f;
        [SerializeField] float _dashDuration = 0.25f;
        [SerializeField] float _dashInvulnerableDuration = 0.2f;
        [SerializeField] float _dashCooldown = 0.5f;

        CharacterController _controller;
        InputAction _move;
        InputAction _dashAction;
        InputAction _sprint;
        DashTimer _dashTimer;
        Vector3 _dashDirection;
        float _verticalVelocity;

        public bool IsDashing => _dashTimer != null && _dashTimer.IsDashing;
        public bool IsInvulnerable => _dashTimer != null && _dashTimer.IsInvulnerable;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _dashTimer = new DashTimer(_dashDuration, _dashInvulnerableDuration, _dashCooldown);
            InputActionMap map = _actions.FindActionMap("Player", true);
            _move = map.FindAction("Move", true);
            _dashAction = map.FindAction("Dash", true);
            _sprint = map.FindAction("Sprint", true);
        }

        void OnEnable()
        {
            _actions.Enable();
        }

        void OnDisable()
        {
            _actions.Disable();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector2 input = _move.ReadValue<Vector2>();
            Vector3 wishDir = CameraRelative(input);

            if (_dashAction.WasPressedThisFrame())
                TryDash(wishDir);

            float speed = 0f;
            Vector3 displacement;
            if (_dashTimer.IsDashing)
            {
                displacement = _dashDirection * (_dashDistance / _dashDuration * _dashTimer.Tick(dt));
                Face(_dashDirection, dt);
            }
            else
            {
                _dashTimer.Tick(dt);
                if (input.sqrMagnitude > 1e-4f)
                    speed = _moveSpeed + (_sprint.IsPressed() ? _sprintAddition : 0f);
                displacement = wishDir * (speed * dt);
                Face(wishDir, dt);
            }

            _verticalVelocity = _controller.isGrounded ? -1f : _verticalVelocity - _gravity * dt;
            _controller.Move(displacement + Vector3.up * (_verticalVelocity * dt));

            if (_animator != null) _animator.SetFloat(SpeedHash, speed);
        }

        public bool TryDash(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-4f) direction = transform.forward;
            if (!_dashTimer.TryStart()) return false;
            _dashDirection = direction.normalized;
            return true;
        }

        Vector3 CameraRelative(Vector2 input)
        {
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            Vector3 dir = forward.normalized * input.y + right.normalized * input.x;
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        void Face(Vector3 direction, float dt)
        {
            if (direction.sqrMagnitude < 1e-4f) return;
            Quaternion target = Quaternion.LookRotation(direction);
            float t = 1f - Mathf.Exp(-_rotationSharpness * dt);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
        }
    }
}
