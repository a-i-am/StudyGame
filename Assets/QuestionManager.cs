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

public class QuestionManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] List<MockPassageData> mockPassages;
    [SerializeField] TextMeshProUGUI passageText;
    [SerializeField] int testIndex = 0;

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

            Debug.Log($"클릭된 문장 ID: {linkId}");
            Debug.Log($"클릭된 문장 내용: {linkInfo.GetLinkText()}");
        }
    }
    void Start()
    {
        BuildLinkedText();
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

            // <link="인덱스번호">문장내용</link> 형태로 조립
            // 문장과 문장 사이에 띄어쓰기를 한 칸 넣어주어 자연스럽게 이어붙임
            sb.Append($"<link=\"{i}\">{sentence}</link> ");
        }
        
        passageText.text = sb.ToString();
        passageText.ForceMeshUpdate();
    }
}
