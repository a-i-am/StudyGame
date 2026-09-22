using UnityEngine;

namespace StudyGame.Runtime.Combat
{
    public class CameraViewManager : MonoBehaviour
    {
        public Transform target;
        
        public enum ViewMode { BackView, QuarterView }
        public ViewMode currentView = ViewMode.BackView;

        [Header("View Settings")]
        public Vector3 backViewOffset = new Vector3(0f, 2.5f, -5f);
        public float backViewPitch = 15f;
        public float backViewYaw = 0f;

        public Vector3 quarterViewOffset = new Vector3(0f, 7f, -6f);
        public float quarterViewPitch = 45f;
        public float quarterViewYaw = 0f;

        private Camera cam;
        private float currentYaw = 0f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            
            if (target != null) currentYaw = target.eulerAngles.y;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                CycleView();
            }

            if (currentView == ViewMode.BackView)
            {
                currentYaw += Input.GetAxis("Mouse X") * 3f;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position;
            Quaternion desiredRot = Quaternion.identity;

            switch (currentView)
            {
                case ViewMode.BackView:
                    Quaternion backRot = Quaternion.Euler(backViewPitch, currentYaw + backViewYaw, 0f);
                    desiredPos += backRot * backViewOffset;
                    desiredRot = backRot;
                    break;

                case ViewMode.QuarterView:
                    desiredPos += quarterViewOffset;
                    desiredRot = Quaternion.Euler(quarterViewPitch, quarterViewYaw, 0f);
                    break;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPos, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 10f * Time.deltaTime);
        }

        private void CycleView()
        {
            int next = (int)currentView + 1;
            if (next > 1) next = 0;
            currentView = (ViewMode)next;
            Debug.Log($"[CameraViewManager] 뷰 스왑: {currentView}");
        }
    }
}

