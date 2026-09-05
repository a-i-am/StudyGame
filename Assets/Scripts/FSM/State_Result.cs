public class State_Result : IState
{
    private GameFlowManager _manager;
    public State_Result(GameFlowManager manager) { _manager = manager; }

    public void Enter()
    {
        // 예: ViewModel에 있는 TotalScore를 정답 데이터와 비교하여 등급 산출
        // 예: 서버에 결과 데이터 전송 로직 호출
    }
    public void Execute() { }
    public void Exit() { }
}
