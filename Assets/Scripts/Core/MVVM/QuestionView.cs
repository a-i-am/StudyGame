using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;


public class QuestionView : BaseView<QuestionViewModel>, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI passageText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("State UI Panels/Buttons")]
    [SerializeField] private GameObject readingPanel; // '단서 찾기 시작'
    [SerializeField] private GameObject highlightingPanel; // 모드 변경, '제출하기' 버튼이 있는 패널
    [SerializeField] private GameObject submitPopupPanel; // 제출 재확인 팝업

    public override void Bind(QuestionViewModel vm)
    {
        base.Bind(vm);

        viewModel.TotalScore.Subscribe(UpdateScoreUI).AddTo(disposables);
        viewModel.OnSelectionChanged += BuildLinkedText;

        GameFlowManager.Instance.CurrentStateType.Subscribe(OnGameStateChanged).AddTo(disposables);

        UpdateScoreUI(viewModel.TotalScore.Value);
        BuildLinkedText();
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"현재 총 단서 점수: {newScore}";
        }
    }

    private void BuildLinkedText()
    {
        if (viewModel == null || viewModel.CurrentPassage == null) return;

        string targetText = viewModel.CurrentPassage.contentText;
        string pattern = @"(?<=[.!?])\s+";
        string[] sentences = System.Text.RegularExpressions.Regex.Split(targetText, pattern);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < sentences.Length; i++)
        {
            string sentence = sentences[i].Trim();
            if (string.IsNullOrEmpty(sentence)) continue;

            if (viewModel.SelectedIndices.Contains(i))
            {
                switch (viewModel.CurrentHighlightMode)
                {
                    case HighlightMode.Color: sentence = $"<color=#FFD700>{sentence}</color>"; break;
                    case HighlightMode.Underline: sentence = $"<u>{sentence}</u>"; break;
                    case HighlightMode.Mark: sentence = $"<mark=#FFFF0055>{sentence}</mark>"; break;
                }
            }
            sb.Append($"<link=\"{i}\">{sentence}</link> ");
        }

        passageText.text = sb.ToString();
        passageText.ForceMeshUpdate();
    }

    private void OnGameStateChanged(GameState state)
    {
        readingPanel.SetActive(false);
        highlightingPanel.SetActive(false);
        submitPopupPanel.SetActive(false);

        switch (state)
        {
            case GameState.Reading:
                readingPanel.SetActive(true);
                // 지문을 읽기만 해야 하므로, 클릭(단서 선택) 불가 처리
                passageText.raycastTarget = false;
                break;
            case GameState.Highlighting:
                highlightingPanel.SetActive(true);
                // 이제 단서를 선택해야 하므로 클릭 가능하게 변경
                passageText.raycastTarget = true;
                break;
            case GameState.Submit:
                highlightingPanel.SetActive(true); // 배경 유지
                submitPopupPanel.SetActive(true);  // 제출 팝업 띄우기
                passageText.raycastTarget = false; // 선택 수정 불가
                break;
            case GameState.Result:
                // 결과창 UI 띄우기 처리...
                break;

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // TODO : (이전 클릭 로직 동일하게 구현하기)
    }

    // [단서 찾기 시작] 버튼에 연결
    public void OnClickCancelHighlighting()
    {
        GameFlowManager.Instance.ChangeState(GameState.Highlighting);
    }

    // [제출하기] 버튼에 연결
    public void OnClickSubmit()
    {
        GameFlowManager.Instance.ChangeState(GameState.Submit);
    }

    // [제출 취소] 버튼에 연결 (팝업 닫기)
    public void OnClickCancelSubmit()
    {
        GameFlowManager.Instance.ChangeState(GameState.Highlighting);
    }
}
