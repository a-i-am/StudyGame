using UnityEngine;
using StudyGame.Player;

namespace StudyGame.Managers
{
    public class CursorManager : MonoBehaviour
    {
        private static CursorManager instance;
        public static CursorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("@CursorManager");
                    instance = go.AddComponent<CursorManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private int activeModalCount = 0;
        private bool gameCursorLocked = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            bool uiOpen = activeModalCount > 0;

            if (uiOpen)
            {
                if (!UnityEngine.Cursor.visible || UnityEngine.Cursor.lockState != CursorLockMode.None)
                {
                    UnityEngine.Cursor.visible = true;
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                }
            }
            else if (gameCursorLocked)
            {
                if (UnityEngine.Cursor.visible || UnityEngine.Cursor.lockState != CursorLockMode.Locked)
                {
                    UnityEngine.Cursor.visible = false;
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                }
            }
            else
            {
                if (!UnityEngine.Cursor.visible || UnityEngine.Cursor.lockState != CursorLockMode.None)
                {
                    UnityEngine.Cursor.visible = true;
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                }
            }
        }

        public void SetGameCursorLocked(bool locked)
        {
            gameCursorLocked = locked;
        }

        public bool IsAnyUIModalOpen()
        {
            return activeModalCount > 0;
        }

        public void RegisterModalOpen()
        {
            activeModalCount++;

            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetMovementEnabled(false);

            var cam = FindFirstObjectByType<CameraController>();
            if (cam != null) cam.SetInputEnabled(false);
        }

        public void RegisterModalClose()
        {
            activeModalCount = Mathf.Max(0, activeModalCount - 1);

            if (activeModalCount == 0 && gameCursorLocked)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) player.SetMovementEnabled(true);

                var cam = FindFirstObjectByType<CameraController>();
                if (cam != null) cam.SetInputEnabled(true);
            }
        }
    }
}

