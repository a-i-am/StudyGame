using UnityEngine;

public class State_Submit : IState
{
    private GameFlowManager _manager;
    public State_Submit(GameFlowManager manager) { _manager = manager; }

    public void Enter() { Debug.Log("== 정답 제출 확인 상태 =="); }
    public void Execute() { }
    public void Exit() { }
}
