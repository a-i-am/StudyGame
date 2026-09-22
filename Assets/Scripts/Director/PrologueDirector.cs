using UnityEngine;
using UnityEngine.Video;

namespace StudyGame.Runtime.Director
{
    public class PrologueDirector : MonoBehaviour
    {
        [Header("Scene Components")]
        public VideoPlayer promoVideoPlayer;
        public Camera firstPersonCamera;
        
        [Header("UI Components")]
        public GameObject snsUIController;

        private void Start()
        {
            if (promoVideoPlayer != null)
            {
                promoVideoPlayer.loopPointReached += OnVideoFinished;
                promoVideoPlayer.Play();
                
                if (firstPersonCamera) firstPersonCamera.gameObject.SetActive(false);
                if (snsUIController) snsUIController.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[PrologueDirector] VideoPlayer가 없습니다. 바로 교실 씬으로 넘어갑니다.");
                OnVideoFinished(null);
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[PrologueDirector] 홍보 영상 종료. 교실에서 깨어납니다.");
            
            if (vp != null) vp.gameObject.SetActive(false);
            
            if (firstPersonCamera) firstPersonCamera.gameObject.SetActive(true);
            
            Invoke(nameof(TriggerSNSNotification), 2f);
        }

        private void TriggerSNSNotification()
        {
            Debug.Log("[PrologueDirector] 폰 진동 - SNS 팝업 연출 시작");
            if (snsUIController)
            {
                snsUIController.SetActive(true);
            }
        }
    }
}
