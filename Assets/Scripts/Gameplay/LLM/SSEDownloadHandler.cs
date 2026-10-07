using System;
using System.Text;
using UnityEngine.Networking;

namespace StudyGame.LLM
{
    public class SSEDownloadHandler : DownloadHandlerScript
    {
        private Action<string> onToken;
        private Action onComplete;

        private Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        private StringBuilder lineBuffer = new StringBuilder();
        private char[] charBuffer = new char[1024];

        public SSEDownloadHandler(Action<string> onToken, Action onComplete) : base(new byte[4096])
        {
            this.onToken = onToken;
            this.onComplete = onComplete;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength < 1) return true;

            int maxChars = Encoding.UTF8.GetMaxCharCount(dataLength);
            if (charBuffer.Length < maxChars)
            {
                charBuffer = new char[maxChars];
            }

            int charsDecoded = utf8Decoder.GetChars(data, 0, dataLength, charBuffer, 0, false);
            for (int i = 0; i < charsDecoded; i++)
            {
                char c = charBuffer[i];
                if (c == '\n')
                {
                    ProcessLine(lineBuffer.ToString());
                    lineBuffer.Clear();
                }
                else if (c != '\r')
                {
                    lineBuffer.Append(c);
                }
            }

            return true;
        }

        private void ProcessLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            line = line.Trim();
            if (!line.StartsWith("data:")) return;

            string dataPayload = line.Substring(5).Trim();
            if (dataPayload == "[DONE]")
            {
                onComplete?.Invoke();
                return;
            }

            string content = ExtractDeltaContent(dataPayload);
            if (!string.IsNullOrEmpty(content))
            {
                onToken?.Invoke(content);
            }
        }

        private string ExtractDeltaContent(string json)
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
                        return json.Substring(startQuote + 1, endQuote - startQuote - 1)
                            .Replace("\\n", "\n")
                            .Replace("\\\"", "\"")
                            .Replace("\\\\", "\\");
                    }
                }
            }
            catch (Exception) { }

            return null;
        }

        protected override void CompleteContent()
        {
            if (lineBuffer.Length > 0)
            {
                ProcessLine(lineBuffer.ToString());
                lineBuffer.Clear();
            }
            onComplete?.Invoke();
        }
    }
}
