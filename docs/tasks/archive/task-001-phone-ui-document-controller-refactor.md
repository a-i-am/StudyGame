# Task-001: 스마트폰 가상 OS UI Document 컨트롤러 분리 및 상태 바인딩 정비

## 1. 개요 및 기획 의도 (CoreLoopContext)
- **Domain:** `VirtualOS_2D`
- **Intent:** `PhoneUIDocumentController`(View)와 `PhoneUIController`(Presenter) 간 중복 쿼리 및 책임 중첩을 해소하고, 런타임 캐싱을 적용하여 메시지 출력 시 GC 부하를 0 Byte로 억제한다.
- **Edge Cases:**
  - 시퀀스 도중 전화창이 강제 비활성화되거나 씬 전환이 발생할 때 진행 중인 코루틴이 안전하게 정리되어야 함.
  - 메시지가 없는 빈 SNSData 수신 시 예외 없이 창이 즉시 닫히거나 다음 상태로 전이되어야 함.

## 2. 상태 및 데이터 규격 (StateAndDataSpec)
- **Target FSM:** `PhoneViewState: Hidden -> Showing -> MessageDisplaying -> Completed -> Hidden`
- **Data Structures:** `SNSData`, `SequenceNode`
- **UI Bindings:** `#MessageScroll`, `#SenderName`, `#ProfileImage`, `#PhoneFrame`

## 3. 수정 경계선 제약 (BoundaryConstraints)
- **Allowed Paths:**
  - `Assets/Scripts/PhoneUIDocumentController.cs`
  - `Assets/Scripts/PhoneUIController.cs`
- **Prohibited Paths:**
  - `Assets/Scripts/Core/*`
  - `Assets/Scripts/Combat/*`
  - `Assets/Editor/*`
  - `*.meta`
- **Forbidden Patterns:**
  - 매 프레임 또는 루프 내 `VisualElement.Q<T>()` 호출 금지 (Awake/OnEnable 1회 캐싱)
  - 코루틴 내부 `new WaitForSeconds` 매회 동적 할당 금지 (미리 캐싱된 인스턴스 재사용)
  - 람다 클로저를 활용한 무명 델리게이트 이벤트 등록 금지

## 4. 완료 검증 기준 (Definition of Done)
1. 유니티 에디터 C# 컴파일 에러/경고 0건 검증
2. `PhoneUIDocumentController`가 순수 뷰(UXML 요소 바인딩 및 렌더링), `PhoneUIController`가 시퀀스 제어로 명확히 분리
3. 메시지 연속 출력 시 코루틴 힙 할당(GC Alloc) 0 Byte 달성
4. 허용 경로 외 프로젝트 코어 파일 무수정 확인
