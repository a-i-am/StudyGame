using UnityEngine;
using UnityEngine.UI;

namespace StudyGame.UI
{
    public class SNSUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text senderText;
        public Text messageText;
        public Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSNS);
            }
        }

        public void ShowNotification(string sender, string message)
        {
            Debug.Log($"[SNS] {sender} : {message}");
            gameObject.SetActive(true);

            if (senderText) senderText.text = sender;
            if (messageText) messageText.text = message;
        }

        private void CloseSNS()
        {
            Debug.Log("[SNS] UI 닫힘");
            gameObject.SetActive(false);
        }
    }
}
