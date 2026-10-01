# StudyGame System Constitution (GEMINI.md)

이 문서는 StudyGame 프로젝트에서 AI(Gemini / Antigravity)가 코드를 분석, 설계, 작성할 때 최우선으로 준수해야 하는 최상위 글로벌 헌법이다.

---

## 1. 에이전트 행동 제한 (Mandatory Restraints)

- **추측성 구현 금지:** 모든 개발 및 리팩토링 착수 전 반드시 `docs/tasks/active/` 내의 해당 작업 명세서(`task-*.md`)를 확인한다. 명세서 없이 임의로 코드를 수정하거나 확장하지 않는다.
- **사전 실행 승인 (Permission First):** 코드를 생성하거나 수정하기 전, 항상 작업 계획과 영향 범위를 사용자에게 먼저 설명하고 명시적 승인을 기다린다.
- **실제 상태 검증 우선 (Verify Before Assume):** 코드 내 참조/이름(예: switch-case, 문자열 키, 인터페이스 목록)만 보고 에셋이나 씬 바인딩 데이터가 실존할 것으로 지레짐작하지 않는다. 반드시 실제 파일 경로 존재 여부 및 씬 직렬화 데이터(YAML/JSON)를 직접 검증한 후 상태를 판단한다.
- **엄격한 UI 규칙:** 명시적인 허가 없이 버튼이나 UI 요소를 절대 추가하지 않는다. 강제로 씬(Scene)을 전환하거나 사용자 설정을 우회하지 않는다.
- **포니테일 모드 (YAGNI):** 오버엔지니어링을 엄격히 방지하고, 최소한의 복잡도로 현재 명세된 기능만을 구현한다. 추측성 추상화나 불필요한 디자인 패턴 도입을 금지한다.
- **코딩 스타일:** 생성된 코드에 주석을 일절 추가하지 않는다. 주석 없는 깔끔한 코드를 제공한다.

---

## 2. 유니티 C# 엔지니어링 헌법 (Engine & Platform Rules)

- **메모리 및 GC 통제:**
  - 메인 루프(`Update`, `FixedUpdate`) 및 이벤트 콜백 내 힙 할당(`new`, LINQ, 람다 클로저, 박싱)을 엄격히 금지한다.
  - UI Toolkit: 매 프레임 `VisualElement.Q<T>()` 쿼리 호출을 금지하며, 초기화 시점에 1회 캐싱하여 사용한다.
- **에셋 및 메타데이터 무결성:**
  - `.meta` 파일 누락, GUID 파괴, 씬/프리팹 파일 임의 덮어쓰기를 절대 금지한다.
  - 리소스 로드는 `Resources.Load` 경로 하드코딩을 지양하고, ScriptableObject 직접 참조 또는 Addressables 체계를 따른다.
- **아키텍처 및 에디터 격리:**
  - `Assets/Editor/StudyGameDirector/` 등 에디터 확장 코드는 런타임 Assembly Definition(`*.asmdef`)에 참조되어서는 안 되며, 완벽한 물리적 격리를 유지한다.
  - UI(View)는 데이터 모델(Model)을 직접 변조하지 못하며, 이벤트 버스 또는 바인딩 계약을 통해서만 소통한다.
- **Unity MCP 도구 제어 가드:**
  - MCP 도구 호출 시 씬 Hierarchy의 루트 계층을 임의로 재구성하지 않는다.
  - 대량 에셋 Reimport나 무분별한 `SaveScene()` 호출을 지양하고, 작업 파일 단위로 국소적 변경을 적용한다.

---

## 3. 3-Tier 컨텍스트 로딩 인덱스 (Context Guide)

AI는 전체 문서를 한꺼번에 로드하지 않고, 작업 성격에 따라 아래 인덱스를 참조해 필요한 문서만 최소한으로 소비한다.

- **Tier 1. 글로벌 헌법 (항상 로드):**
  - `<Project Root>/GEMINI.md`
- **Tier 2. 코어 아키텍처 맵 (도메인 설계 시 참조):**
  - `docs/architecture/core-loop.md`: 2D 가상 OS ↔ 3D 실시간 액션 루프 및 경제 교환비
  - `docs/architecture/skill-socket-system.md`: 뼈대(Base) 스킬 + 개념(Socket) ScriptableObject + 5대 상태이상 모듈
  - `docs/architecture/lock-and-key-gimmick.md`: 몬스터 요구 태그(자물쇠) ↔ 플레이어 정답 태그(열쇠)
  - `docs/architecture/editor-director.md`: StudyGameDirector 플로우 시퀀서 및 ProBuilder 맵 양산 툴
- **Tier 3. 기능 단위 작업 명세서 (작업 시에만 인라인 주입):**
  - `docs/tasks/active/task-xxx.md`: 현재 작업에 할당된 50줄 이내의 단일 기능 계약서

---

## 4. 코어 게임플레이 및 내러티브 요약

- **코어 루프:**
  - `[Loop 1. 2D 가상 OS]` 태블릿/스마트폰 UI에서 다이어리 확인, UWB 인강/문서 열람, Q&A 디코더로 스킬 소켓팅(Loadout).
  - `[Loop 2. 3D 결계 아레나]` 기이 공간 진입, 별의 커비 디스커버리 스타일의 유연한 3인칭 뷰에서 실시간 액션 전투 수행.
  - `[Loop 3. 자물쇠-열쇠 기믹]` 몬스터의 물리적 패턴 추론 후 일치하는 교과 개념 소켓으로 약점 타격(그로기 유도).
  - `[Loop 4. 구출 및 정화]` 동급생 NPC 구출 및 암호화된 과목 데이터 조각 획득, 디지털 실외 정화 연출.
  - `[Loop 5. 복귀 및 SOS 학습]` 가상 OS로 복귀, 획득한 조각 해독 및 SOS 힌트/퀴즈를 통한 연산 포인트 수급.
- **내러티브 특수성:**
  - 주인공은 독백, 속마음, 직접 대사가 없는 '침묵하는 관찰자(Silent Protagonist)'이며, 2인칭 행동 선택지와 스마트폰 시스템 로그로 서사를 전달한다.

---

## 5. 프로젝트 고정 용어 사전 (Terminology Dictionary)

- **가상 OS (Virtual OS):** 플레이어가 2D 화면에서 정비, 다이어리, UWB, SNS를 조작하는 UI 캔버스 환경.
- **뼈대 스킬 (Base Skill):** 과목별로 제공되는 물리적/시각적 기본 공격 형태 (직선 빔, 부채꼴 파동, 전방 대시 등).
- **개념 소켓 (Concept Socket):** 뼈대 스킬에 장착하여 수치(범위, 쿨타임, 상태이상)를 변경하는 하위 교과 데이터 (ScriptableObject).
- **5대 상태이상 모듈:** 응축/당김(모듈 A), 팽창/밀쳐냄(모듈 B), 역전/반사(모듈 C), 정지/동결(모듈 D), 연쇄/반응(모듈 E).
- **약점 판독기 (WeaknessReceiver):** 몬스터나 퍼즐에 부착되어 플레이어 공격의 교과 태그 일치 여부를 판별하는 컴포넌트.
- **UWB (인게임 웹 브라우저):** 게임 내에서 외부 학습 문서나 동영상을 스트리밍하는 오버레이 브라우저 시스템.
- **StudyGameDirector:** 에피소드 시퀀스 및 스테이지 블루프린트를 제어하는 커스텀 에디터 윈도우 허브.

---

## 6. 작업 완료 검증 기준 (Definition of Done)

작업 완료를 보고하기 전 다음 3단계를 순차적으로 검증해야 한다:

1. **컴파일 및 무에러 검증:** 유니티 에디터 컴파일 성공 및 콘솔 Warning/Error 0건 확인.
2. **런타임 메모리 검증:** 신규 로직 실행 시 불필요한 GC Allocation(0 Byte 유지) 확인.
3. **경계선 준수 검증:** 해당 `task-xxx.md`에 지정된 허용 경로 외의 코어 파일이 변경되지 않았는지 확인.
