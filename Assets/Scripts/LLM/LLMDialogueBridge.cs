using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace StudyGame.LLM
{
    public class LLMDialogueBridge : MonoBehaviour
    {
        public static LLMDialogueBridge Instance { get; private set; }

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

        public void SendQuestion(string systemPrompt, string userQuestion, Action<string> onComplete, Action<string> onError)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                StartCoroutine(MockResponseRoutine(userQuestion, onComplete));
                return;
            }

            StartCoroutine(PostWebRequestRoutine(systemPrompt, userQuestion, onComplete, onError));
        }

        private IEnumerator PostWebRequestRoutine(string systemPrompt, string userQuestion, Action<string> onComplete, Action<string> onError)
        {
            string jsonBody = $"{{\"model\":\"{modelName}\",\"messages\":[{{\"role\":\"system\",\"content\":\"{EscapeJson(systemPrompt)}\"}},{{\"role\":\"user\",\"content\":\"{EscapeJson(userQuestion)}\"}}]}}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    string reply = ExtractContentFromJson(responseText);
                    onComplete?.Invoke(reply);
                }
                else
                {
                    Debug.LogWarning($"[LLMBridge] Request failed: {request.error}. Fallback to Mock Response.");
                    StartCoroutine(MockResponseRoutine(userQuestion, onComplete));
                }
            }
        }

        private IEnumerator MockResponseRoutine(string userQuestion, Action<string> onComplete)
        {
            yield return new WaitForSeconds(1.2f);
            string mockAnswer = $"질문하신 '{userQuestion}'에 대한 힌트입니다: 현상과 관련된 수학적 규칙에 주의를 기울여 보세요!";
            onComplete?.Invoke(mockAnswer);
        }

        private string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private string ExtractContentFromJson(string json)
        {
            try
            {
                int contentIdx = json.IndexOf("\"content\":");
                if (contentIdx >= 0)
                {
                    int startQuote = json.IndexOf("\"", contentIdx + 10);
                    int endQuote = json.IndexOf("\"", startQuote + 1);
                    if (startQuote >= 0 && endQuote > startQuote)
                    {
                        return json.Substring(startQuote + 1, endQuote - startQuote - 1).Replace("\\n", "\n");
                    }
                }
            }
            catch (Exception) { }
            return "응답을 해석하는 중에 오류가 발생했습니다.";
        }
    }
}
