using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

// 데이터를 담을 클래스 생성
[System.Serializable]
public class MockPassageData
{
    public string passageId;       // 예: "P_001"
    [TextArea(3, 5)]               // 인스펙터에서 넓게 입력할 수 있도록 속성 추가
    public string contentText;     // 실제 지문 텍스트
    public string correctKeyword;  // 추후 사용할 정답 정보
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

    [Header("Feedback Settings")]
    HighlightMode currentHighlightMode = HighlightMode.Underline;

    // 다중 선택된 문장의 인덱스들을 저장할 HashSet (중복 방지 및 빠른 탐색)
    private HashSet<int> selectedSentenceIndices = new HashSet<int>();
    void Start()
    {
        BuildLinkedText();
    }

    // 마우스 클릭이나 터치가 발생할 때 자동으로 호출되는 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        Camera eventCamera = eventData.pressEventCamera;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(passageText, eventData.position, eventCamera);

        // 2. 클릭된 곳에 링크가 존재한다면 (linkIndex가 -1이 아니라면)
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
            }
            else
            {
                selectedSentenceIndices.Add(clickedIndex);
            }

            // 상태가 변했으므로 텍스트 다시 렌더링
            BuildLinkedText();
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && passageText != null)
        {
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

            // HashSet에 현재 문장의 인덱스가 포함되어 있는지 확인하여 다중 선택 처리

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

            // <link="인덱스번호">문장내용</link> 형태로 조립
            // 문장과 문장 사이에 띄어쓰기를 한 칸 넣어주어 자연스럽게 이어붙임
            sb.Append($"<link=\"{i}\">{sentence}</link> ");
        }
        
        passageText.text = sb.ToString();
        passageText.ForceMeshUpdate();
    }

    // 다음 문제로 넘어갈 때 선택 내역을 초기화하는 유틸리티 함수
    public void ClearSelection()
    {
        selectedSentenceIndices.Clear();
        BuildLinkedText();
    }
}
