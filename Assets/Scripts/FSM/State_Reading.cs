using UnityEngine;

// [상태 1] 문제 읽기 상태
public class State_Reading : IState
{
    private GameFlowManager _manager;

    public State_Reading(GameFlowManager manager)
    {
        _manager = manager;
    }
    
    public void Enter()
    {
        // TODO: MVVM 구조를 활용해 텍스트 타이핑 연출 시작
        // TODO: 단서 긋기 UI 비활성화, 텍스트 상자 활성화
         // 예: 문제 데이터를 불러와서 ViewModel에 세팅하는 비즈니스 로직
        // 예: 타이머 0으로 초기화
    }

    public void Execute()
    {
        // 타이핑 연출 중에 스킵 입력을 받는 등 매 프레임 처리할 로직
    }

    public void Exit()
    {
        Debug.Log("== 문제 읽기 상태 종료 ==");
        // TODO: 타이핑 연출 강제 종료 및 텍스트 100% 출력 처리
    }
}    