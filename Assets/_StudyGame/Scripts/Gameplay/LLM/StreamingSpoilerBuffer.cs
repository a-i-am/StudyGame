using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StudyGame.LLM
{
    public class StreamingSpoilerBuffer
    {
        private readonly List<string> forbiddenWords = new List<string>();
        private readonly Action<char> onSafeCharCallback;
        private readonly int windowSize;
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly string replacement = "그 핵심 규칙";

        public StreamingSpoilerBuffer(List<string> forbiddenWords, Action<char> onSafeCharCallback, int windowSize = 12)
        {
            if (forbiddenWords != null)
            {
                foreach (string word in forbiddenWords)
                {
                    if (string.IsNullOrWhiteSpace(word)) continue;
                    string trimmed = word.Trim();
                    if (!this.forbiddenWords.Contains(trimmed))
                    {
                        this.forbiddenWords.Add(trimmed);
                    }
                    string noSpace = trimmed.Replace(" ", "");
                    if (noSpace != trimmed && !this.forbiddenWords.Contains(noSpace))
                    {
                        this.forbiddenWords.Add(noSpace);
                    }
                }
            }

            this.onSafeCharCallback = onSafeCharCallback;
            this.windowSize = Math.Max(4, windowSize);
        }

        public void AppendChunk(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;

            buffer.Append(chunk);
            CheckAndCensorBuffer();
            FlushSafeWindow();
        }

        public void FlushRemaining()
        {
            CheckAndCensorBuffer();
            while (buffer.Length > 0)
            {
                char safeChar = buffer[0];
                buffer.Remove(0, 1);
                onSafeCharCallback?.Invoke(safeChar);
            }
        }

        private void CheckAndCensorBuffer()
        {
            if (buffer.Length == 0 || forbiddenWords.Count == 0) return;

            string currentText = buffer.ToString();
            bool modified = false;

            foreach (string word in forbiddenWords)
            {
                if (currentText.Contains(word))
                {
                    Debug.Log($"[스포일러 차단] 정답 키워드 감지 ➔ 마스킹 처리 ({word} -> {replacement})");
                    currentText = currentText.Replace(word, replacement);
                    modified = true;
                }
            }

            if (modified)
            {
                buffer.Clear();
                buffer.Append(currentText);
            }
        }

        private void FlushSafeWindow()
        {
            while (buffer.Length > windowSize)
            {
                char safeChar = buffer[0];
                buffer.Remove(0, 1);
                onSafeCharCallback?.Invoke(safeChar);
            }
        }
    }
}
