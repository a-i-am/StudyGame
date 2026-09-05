using UnityEngine;
// [상태 2] 단서 밑줄 긋기 상태

public class State_Highlighting : IState
{
    private GameFlowManager _manager;

    public State_Highlighting(GameFlowManager manager)
    {
        _manager = manager;
    }

    public void Enter()
    {
        Debug.Log("== 단서 긋기 상태 진입 ==");
        // TODO: 이전에 만든 QuestionManager(MVVM)를 통해 클릭 이벤트 활성화
    }

    public void Execute()
    {
        // 필요시 타이머 체크 등 수행
    }

    public void Exit()
    {
        Debug.Log("== 단서 긋기 상태 종료 ==");
        // TODO: 클릭(단서 선택) 이벤트 일시 정지
    }
}
