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
        [SerializeField] float _maxVerticalFov = 80f;
        [SerializeField] float _hideDistance = 0.8f;

        [SerializeField] InputActionAsset _actions;
        [SerializeField] Transform _followTarget;
        [SerializeField] float _mouseSensitivity = 0.1f;
        [SerializeField] float _stickSensitivity = 1f;
        [SerializeField] float _minPitch = -30f;
        [SerializeField] float _maxPitch = 60f;

        InputAction _look;
        Renderer[] _playerRenderers;
        bool _playerHidden;
        float _yaw;
        float _pitch;

        void Awake()
        {
            _look = _actions.FindActionMap("Player", true).FindAction("Look", true);
            _playerRenderers = _player.GetComponentsInChildren<Renderer>(true);
        }

        void OnEnable()
        {
            _look.Enable();
        }

        void OnDisable()
        {
            _look.Disable();
        }

        void Start()
        {
            _yaw = _followTarget.eulerAngles.y;
            _pitch = Mathf.DeltaAngle(0f, _followTarget.eulerAngles.x);
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

            _dashCamera.Priority = _player.IsDashing ? 30 : 10;

            float explore = Mathf.Min(FovMath.VerticalFovForAspect(_exploreFov, _referenceAspect, _outputCamera.aspect), _maxVerticalFov);
            _exploreCamera.m_Lens.FieldOfView = explore;
            _dashCamera.m_Lens.FieldOfView = explore + _dashFov - _exploreFov;

            // shortcut: 즉시 숨김, 에셋 세션에서 MToon 디더 페이드로 교체
            bool hide = (_outputCamera.transform.position - _followTarget.position).sqrMagnitude < _hideDistance * _hideDistance;
            if (hide == _playerHidden) return;
            _playerHidden = hide;
            for (int i = 0; i < _playerRenderers.Length; i++) _playerRenderers[i].forceRenderingOff = hide;
        }
    }
}
