using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Draggable3D : MonoBehaviour
{
    private Camera _cam;
    private bool _isDragging = false;
    private float _zDistance;
    private Vector3 _offset;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null)
        {
            _cam = FindFirstObjectByType<Camera>();
        }
    }

    private void Update()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                _cam = FindFirstObjectByType<Camera>();
            }
            if (_cam == null) return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    _isDragging = true;
                    _zDistance = hit.distance;
                    _offset = transform.position - hit.point;
                }
            }
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPoint = ray.GetPoint(_zDistance) + _offset;
            transform.position = targetPoint;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }
}
