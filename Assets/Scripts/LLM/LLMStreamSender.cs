using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace StudyGame.LLM
{
    public class LLMStreamSender : MonoBehaviour
    {
        public static LLMStreamSender Instance { get; private set; }

        [SerializeField] private bool useMockStreaming = true;
        [SerializeField] private string apiKey = "";
        [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
        [SerializeField] private string modelName = "gpt-3.5-turbo";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartStream(string systemPrompt, string userPrompt, Action<string> onToken, Action onComplete, Action<string> onError)
        {
            if (useMockStreaming || string.IsNullOrEmpty(apiKey))
            {
                StartCoroutine(MockStreamRoutine(userPrompt, onToken, onComplete));
                return;
            }

            StartCoroutine(PostStreamRoutine(systemPrompt, userPrompt, onToken, onComplete, onError));
        }

        private IEnumerator PostStreamRoutine(string systemPrompt, string userPrompt, Action<string> onToken, Action onComplete, Action<string> onError)
        {
            string jsonBody = $"{{\"model\":\"{modelName}\",\"stream\":true,\"messages\":[{{\"role\":\"system\",\"content\":\"{EscapeJson(systemPrompt)}\"}},{{\"role\":\"user\",\"content\":\"{EscapeJson(userPrompt)}\"}}]}}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new SSEDownloadHandler(onToken, onComplete);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "text/event-stream");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[LLMStreamSender] Stream failed: {request.error}. Fallback to Mock Stream.");
                    StartCoroutine(MockStreamRoutine(userPrompt, onToken, onComplete));
                }
            }
        }

        private IEnumerator MockStreamRoutine(string userPrompt, Action<string> onToken, Action onComplete)
        {
            yield return new WaitForSeconds(0.2f);
            
            string mockResponse = "";
            if (userPrompt.Contains("정답") || userPrompt.Contains("등비수열") || userPrompt.Contains("극한"))
            {
                mockResponse = $"질문하신 '{userPrompt}'에 대하여 힌트를 드리겠습니다. 현상은 '등비수열의 극한' 개념과 밀접하게 연관되어 있으니, 공비의 절댓값 조건에 유의하면서 단서를 조합해보세요!";
            }
            else
            {
                mockResponse = $"질문하신 '{userPrompt}'에 대하여 힌트를 드리겠습니다. 수열의 각 항이 일정한 비율로 변할 때 목표값이 어떻게 되는지 관찰해보세요!";
            }

            for (int i = 0; i < mockResponse.Length; i++)
            {
                onToken?.Invoke(mockResponse[i].ToString());
                yield return new WaitForSeconds(0.05f);
            }

            onComplete?.Invoke();
        }

        private string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
