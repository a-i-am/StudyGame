using UnityEngine;

public enum GameState
{
    None,
    Reading,        // 문제 읽기 (타이핑 연출 등)
    Highlighting,   // 단서 밑줄 긋기 및 추리
    Submit,         // 정답 제출 확인 (팝업 대기)
    Result          // 결과 정산 및 점수 표시
}
public interface IState
 {
     // 상태에 진입할 때 1회 호출
     void Enter();

     // 상태에 머무는 동안 매 프레임 호출 (Update)
     void Execute();

     // 다른 상태로 빠져나갈 때 1회 호출
     void Exit();
 }
