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

        [Header("LLM Settings")]
        [SerializeField] private bool useMockStreaming = false;
        
        [Tooltip("비워두면 PC 환경변수(NVIDIA_API_KEY)에서 자동으로 읽어옵니다. 보안상 비워두는 것을 권장합니다.")]
        [SerializeField] private string apiKey = "";
        
        [Tooltip("NVIDIA NIM (클라우드) 기본 주소")]
        [SerializeField] private string apiUrl = "https://integrate.api.nvidia.com/v1/chat/completions";
        
        [Tooltip("현재 사용 가능한 NVIDIA NIM 모델입니다.")]
        [SerializeField] private string modelName = "meta/llama-3.2-11b-vision-instruct";

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
            if (useMockStreaming)
            {
                StartCoroutine(MockStreamRoutine(userPrompt, onToken, onComplete));
                return;
            }

            string actualApiKey = apiKey;
            if (string.IsNullOrEmpty(actualApiKey))
            {
                actualApiKey = System.Environment.GetEnvironmentVariable("NVIDIA_API_KEY");
                if (string.IsNullOrEmpty(actualApiKey))
                {
                    Debug.LogError("[LLMStreamSender] API Key가 설정되지 않았습니다. 환경변수 NVIDIA_API_KEY를 확인해주세요.");
                    onError?.Invoke("API Key is missing.");
                    return;
                }
            }

            Debug.Log($"[LLMStreamSender] API 호출 시작. 모델: {modelName}, URL: {apiUrl}, Key: {actualApiKey.Substring(0, Mathf.Min(8, actualApiKey.Length))}...");
            StartCoroutine(PostStreamRoutine(systemPrompt, userPrompt, actualApiKey, onToken, onComplete, onError));
        }

        private IEnumerator PostStreamRoutine(string systemPrompt, string userPrompt, string actualApiKey, Action<string> onToken, Action onComplete, Action<string> onError)
        {
            string jsonBody = $"{{\"model\":\"{modelName}\",\"stream\":true,\"messages\":[{{\"role\":\"system\",\"content\":\"{EscapeJson(systemPrompt)}\"}},{{\"role\":\"user\",\"content\":\"{EscapeJson(userPrompt)}\"}}]}}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new SSEDownloadHandler(onToken, onComplete);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "text/event-stream");
                request.SetRequestHeader("Authorization", $"Bearer {actualApiKey}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string responseBody = request.downloadHandler?.text ?? "(no body)";
                    Debug.LogError($"[LLMStreamSender] API 호출 실패! result={request.result}, error={request.error}, HTTP={request.responseCode}, body={responseBody}");
                    onError?.Invoke($"API 호출 실패: {request.error} (HTTP {request.responseCode})");
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
