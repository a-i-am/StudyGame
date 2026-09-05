using System;
using System.Collections.Generic;

public class QuestionViewModel : BaseViewModel
{
    private QuestionModel _model;

    // View가 구독할 점수 데이터 (값이 바뀌면 자동으로 View에 알림을 보냄)
    public BindableProperty<int> TotalScore { get; private set; }
    
    // 텍스트를 다시 조립하라고 View에게 알리는 Action 델리게이트
    public Action OnSelectionChanged; 

    // View가 화면을 그릴 때 필요한 데이터를 꺼내갈 수 있도록 열어둠
    public MockPassageData CurrentPassage => _model.CurrentPassage;
    public HashSet<int> SelectedIndices => _model.SelectedIndices;
    public HighlightMode CurrentHighlightMode => _model.CurrentHighlightMode;

    public QuestionViewModel(QuestionModel model)
    {
        _model = model;
        TotalScore = new BindableProperty<int>(0);
        CalculateTotalScore();
    }

    // View에서 문장을 클릭했을 때 호출할 함수 (선택 토글)
    public void ToggleSentenceSelection(int index)
    {
        if (_model.SelectedIndices.Contains(index))
        {
            _model.SelectedIndices.Remove(index);
        }
        else
        {
            _model.SelectedIndices.Add(index);
        }

        CalculateTotalScore();        // 점수 재계산
        OnSelectionChanged?.Invoke(); // View에게 텍스트 갱신 알림
    }

    // 모드 변경 버튼을 눌렀을 때 호출할 함수
    public void SetHighlightMode(HighlightMode mode)
    {
        _model.CurrentHighlightMode = mode;
        OnSelectionChanged?.Invoke();
    }

    // 내부 점수 합산 로직
    private void CalculateTotalScore()
    {
        int sum = 0;
        foreach (int idx in _model.SelectedIndices)
        {
            // 인스펙터에 점수가 세팅되어 있다면 해당 점수 적용, 아니면 기본 10점
            if (_model.CurrentPassage.sentenceScores != null && idx < _model.CurrentPassage.sentenceScores.Count)
            {
                sum += _model.CurrentPassage.sentenceScores[idx];
            }
            else
            {
                sum += 10; 
            }
        }
        
        // Value 값을 변경하면 BindableProperty 내부에서 자동으로 이벤트를 발송함
        TotalScore.Value = sum; 
    }

    // 뷰모델 파괴 시 메모리 누수 방지용
    public override void Dispose()
    {
        OnSelectionChanged = null;
    }
}