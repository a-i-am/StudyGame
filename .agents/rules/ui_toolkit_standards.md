---
name: ui-toolkit-standards
description: AI가 코드를 짤 때 반드시 지켜야 할 StudyGame 프로젝트의 UI Toolkit 표준 개발 패턴
always_on: true
---

# StudyGame UI Toolkit 표준 개발 패턴

1. **이름 기반 의존성 최소화 (UQuery 래핑)**
   - UI 요소 검색 시 문자열 리터럴을 흩뿌리지 않습니다.
   - 고정 UI는 `partial class`를 활용해 소스 제너레이터가 생성한 변수(예: `_titleLabel`)만 접근하여 로직을 작성합니다. UQuery 문자열 리터럴을 직접 입력하지 마십시오.

2. **View와 Logic의 엄격한 분리 (Passive View)**
   - `~View.cs` (VisualElement 상속/래퍼 클래스)는 순수하게 데이터를 받아 UI에 할당하거나 버튼 이벤트를 노출하기만 해야 합니다. 내부 상태나 게임 로직을 가지지 않도록 작성하십시오.

3. **메모리 해제 보장 룰 (Mandatory Cleanup)**
   - `+=` 연산자로 이벤트나 콜백을 구독하는 코드를 작성할 경우, 누수 방지를 위해 **반드시 쌍으로** `DetachFromPanelEvent` (UI Toolkit) 또는 `Dispose` 내부에서 `-=`를 호출하여 해제하는 코드를 함께 작성하십시오.

4. **LLM 텍스트 스트리밍 최적화**
   - 글자마다 `Label.text += chunk` 방식을 사용하지 마십시오.
   - 잦은 텍스트 갱신은 `StringBuilder` 버퍼와 `isDirty` 플래그를 활용해 프레임별로 모아서(Throttling/Batching) 렌더링해야 합니다.
