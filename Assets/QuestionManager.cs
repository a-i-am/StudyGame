using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

[System.Serializable]
public class MockPassageData
{
    public string passageId;
    [TextArea(3, 5)]
    public string contentText;
    public string correctKeyword;

    [Header("문장별 가중치 (순서대로 1번, 2번 문장...")]
    public List<int> sentenceScores; // 인스펙터에서 각 문장 배점을 설정할 리스트
}

public enum HighlightMode
{
    Color,
    Underline,
    Mark
}

public class QuestionManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Data Settings")]
    [SerializeField] List<MockPassageData> mockPassages;
    [SerializeField] int testIndex = 0;

    [Header("UI Components")]
    [SerializeField] TextMeshProUGUI passageText;
    [SerializeField] TextMeshProUGUI scoreText; // 점수를 표시할 UI 텍스트

    [Header("Feedback Settings")]
    [SerializeField] HighlightMode currentHighlightMode = HighlightMode.Underline;

    private HashSet<int> selectedSentenceIndices = new HashSet<int>();
    private int previousTestIndex = -1;

    void Start()
    {
        BuildLinkedText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Camera eventCamera = eventData.pressEventCamera;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(passageText, eventData.position, eventCamera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = passageText.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();

            if (int.TryParse(linkId, out int clickedIndex))
            {
                if (selectedSentenceIndices.Contains(clickedIndex))
                {
                    selectedSentenceIndices.Remove(clickedIndex);
                }
                else
                {
                    selectedSentenceIndices.Add(clickedIndex);
                }

                // 텍스트 시각적 효과 갱신
                BuildLinkedText();

                // 점수 UI 갱신
                UpdateTotalScore();
            }
        }
    }

    void OnValidate()
    {
        if (passageText != null)
        {
            if (previousTestIndex != testIndex)
            {
                selectedSentenceIndices.Clear();
                previousTestIndex = testIndex;
            }
            BuildLinkedText();
        }
    }

    public void BuildLinkedText()
    {
        if (mockPassages == null || testIndex < 0 || testIndex >= mockPassages.Count)
        {
            return;
        }

        string targetText = mockPassages[testIndex].contentText;
        string pattern = @"(?<=[.!?])\s+";
        string[] sentences = Regex.Split(targetText, pattern);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < sentences.Length; i++)
        {
            string sentence = sentences[i].Trim();
            if (string.IsNullOrEmpty(sentence))
            {
                continue;
            }

            if (selectedSentenceIndices.Contains(i))
            {
                switch (currentHighlightMode)
                {
                    case HighlightMode.Color:
                        sentence = $"<color=#FFD700>{sentence}</color>";
                        break;
                    case HighlightMode.Underline:
                        sentence = $"<u>{sentence}</u>";
                        break;
                    case HighlightMode.Mark:
                        sentence = $"<mark=#FFFF0055>{sentence}</mark>";
                        break;
                }
            }

            sb.Append($"<link=\"{i}\">{sentence}</link> ");
        }

        passageText.text = sb.ToString();
        passageText.ForceMeshUpdate();
    }

    // 선택된 문장들의 가중치를 합산하여 화면에 표시하는 함수
    private void UpdateTotalScore()
    {
        // scoreText가 연결되어 있지 않다면 에러 방지를 위해 리턴
        if (scoreText == null || mockPassages.Count == 0)
        {
            return;
        }

        int totalSum = 0;
        var currentData = mockPassages[testIndex];

        foreach (int index in selectedSentenceIndices)
        {
            int score = 0;

            if (currentData.sentenceScores != null && index < currentData.sentenceScores.Count)
            {
                score = currentData.sentenceScores[index];
            }
            else
            {
                // 인스펙터에 배점을 입력하지 않았거나 개수가 모자란 문장은 기본 10점으로 처리 (예외 처리)
                score = 10;
            }

            totalSum += score;
        }

        scoreText.text = $"현재 총 단서 점수: {totalSum}";
    }
    public void ClearSelection()
    {
        selectedSentenceIndices.Clear();
        BuildLinkedText();
    }
}