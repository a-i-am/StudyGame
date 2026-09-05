using System.Collections.Generic;

public class QuestionModel : IModel
{
    // 현재 화면에 띄워야 할 지문 데이터
    public MockPassageData CurrentPassage { get; set; }
    
    // 유저가 클릭해서 선택한 문장 인덱스 모음
    public HashSet<int> SelectedIndices { get; set; } = new HashSet<int>();
    
    // 현재 선택된 시각적 피드백 모드
    public HighlightMode CurrentHighlightMode { get; set; } = HighlightMode.Underline;
}