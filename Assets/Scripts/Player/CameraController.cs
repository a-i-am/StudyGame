using UnityEngine;

namespace StudyGame.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        public Transform target;
        public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        [Header("Orbit Settings")]
        public float distance = 5.0f;
        public float minDistance = 2.0f;
        public float maxDistance = 10.0f;
        
        [Header("Rotation Settings")]
        public float sensitivityX = 4.0f;
        public float sensitivityY = 4.0f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        [Header("Smoothing")]
        public float smoothTime = 0.12f;
        
        private float currentX = 0.0f;
        private float currentY = 20.0f;
        private Vector3 rotationSmoothVelocity;
        private Vector3 currentRotation;
        
        private bool isInputEnabled = false;

        private void LateUpdate()
        {
            if (target == null) return;

            if (isInputEnabled)
            {
                // Process Input
                currentX += Input.GetAxis("Mouse X") * sensitivityX;
                currentY -= Input.GetAxis("Mouse Y") * sensitivityY;
                currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

                // Zoom Input
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    distance -= scroll * 5f;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                }
            }

            // Smooth Rotation
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(currentY, currentX, 0), ref rotationSmoothVelocity, smoothTime);
            transform.eulerAngles = currentRotation;

            // Calculate Position
            Vector3 targetPos = target.position + targetOffset;
            transform.position = targetPos - transform.forward * distance;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            
            // Initialize rotation based on current camera orientation
            Vector3 angles = transform.eulerAngles;
            currentX = angles.y;
            currentY = angles.x;
            currentRotation = new Vector3(currentY, currentX, 0);
        }

        public void SetInputEnabled(bool isEnabled)
        {
            isInputEnabled = isEnabled;
            if (isEnabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
