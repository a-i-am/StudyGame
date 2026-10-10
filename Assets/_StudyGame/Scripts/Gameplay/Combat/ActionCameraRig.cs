using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StudyGame.Combat
{
    public sealed class ActionCameraRig : MonoBehaviour
    {
        [SerializeField] CinemachineVirtualCamera _exploreCamera;
        [SerializeField] CinemachineVirtualCamera _dashCamera;
        [SerializeField] PlayerLocomotion _player;
        [SerializeField] Camera _outputCamera;
        [SerializeField] float _exploreFov = 50f;
        [SerializeField] float _dashFov = 58f;
        [SerializeField] float _referenceAspect = 16f / 9f;
        [SerializeField] int _activePriority = 20;
        [SerializeField] int _inactivePriority = 10;

        [SerializeField] InputActionAsset _actions;
        [SerializeField] Transform _followTarget;
        [SerializeField] float _mouseSensitivity = 0.1f;
        [SerializeField] float _stickSensitivity = 1f;
        [SerializeField] float _minPitch = -30f;
        [SerializeField] float _maxPitch = 60f;

        InputAction _look;
        bool _dashActive;
        float _lastAspect = -1f;
        float _yaw;
        float _pitch;

        void Awake()
        {
            _look = _actions.FindActionMap("Player", true).FindAction("Look", true);
        }

        void Start()
        {
            _yaw = _followTarget.eulerAngles.y;
            _pitch = Mathf.DeltaAngle(0f, _followTarget.eulerAngles.x);
            ApplyPriorities(false);
        }

        void LateUpdate()
        {
            Vector2 look = _look.ReadValue<Vector2>();
            float scale = _look.activeControl != null && _look.activeControl.device is Pointer
                ? _mouseSensitivity
                : _stickSensitivity * Time.deltaTime;
            _yaw += look.x * scale;
            _pitch = Mathf.Clamp(_pitch - look.y * scale, _minPitch, _maxPitch);
            _followTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            bool dashing = _player.IsDashing;
            if (dashing != _dashActive) ApplyPriorities(dashing);

            float aspect = _outputCamera.aspect;
            if (!Mathf.Approximately(aspect, _lastAspect))
            {
                _lastAspect = aspect;
                _exploreCamera.m_Lens.FieldOfView = FovMath.VerticalFovForAspect(_exploreFov, _referenceAspect, aspect);
                _dashCamera.m_Lens.FieldOfView = FovMath.VerticalFovForAspect(_dashFov, _referenceAspect, aspect);
            }
        }

        void ApplyPriorities(bool dashing)
        {
            _dashActive = dashing;
            _exploreCamera.Priority = dashing ? _inactivePriority : _activePriority;
            _dashCamera.Priority = dashing ? _activePriority : _inactivePriority;
        }
    }
}
