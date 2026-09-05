using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    // 싱글톤 패턴으로 어디서든 접근 가능하게 설정
    public static GameFlowManager Instance { get; private set; }

    private Dictionary<GameState, IState> stateDictionary = new Dictionary<GameState, IState>();
    private IState currentState;
    public BindableProperty<GameState> CurrentStateType { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 💡 BindableProperty는 클래스이므로 Awake에서 반드시 new로 생성(초기화)해주어야 합니다.
        CurrentStateType = new BindableProperty<GameState>(GameState.None);

        InitializeStates();
    }

    // 1. 게임에 필요한 모든 상태를 생성하여 딕셔너리에 등록합니다.
    private void InitializeStates()
    {
        stateDictionary.Add(GameState.Reading, new State_Reading(this));
        stateDictionary.Add(GameState.Highlighting, new State_Highlighting(this));
        stateDictionary.Add(GameState.Submit, new State_Submit(this));
        stateDictionary.Add(GameState.Result, new State_Result(this));
    }

    private void Start()
    {
        // 게임 시작 시 첫 상태로 진입
        ChangeState(GameState.Reading);
    }

    private void Update()
    {
        // 현재 활성화된 상태의 Execute를 매 프레임 실행
        currentState?.Execute();
    }

    // 2. 상태를 전환하는 핵심 함수
    public void ChangeState(GameState newState)
    {
        if (CurrentStateType.Value == newState) return; // 이미 해당 상태면 무시

        // 기존 상태가 있다면 Exit() 호출하여 깔끔하게 정리
        currentState?.Exit();

        if (stateDictionary.TryGetValue(newState, out IState nextState))
        {
            currentState = nextState;

            CurrentStateType.Value = newState; // 이 순간 UI 갱신 이벤트가 발송됨

            // 새 상태 진입
            currentState.Enter();

            Debug.Log($"[GameFlowManager] 상태 전환 완료: {newState}");
        }
        else
        {
            Debug.LogError($"[GameFlowManager] {newState} 상태가 등록되지 않았습니다!");
        }
    }
}
