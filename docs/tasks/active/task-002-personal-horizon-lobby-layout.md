# Task-002: 우유니 소금사막형 수평선 개인 공간 로비 및 9:16 모바일/PC 반응형 와이어프레임 구축

## 1. 개요 및 기획 의도 (CoreLoopContext)
- **Domain:** `VirtualOS_2D` & `PersonalSpace_Lobby`
- **Intent:** 우유니 소금사막처럼 은은한 바닥 환경광과 캐릭터 반사가 있는 고요한 2.5D 수평선 개인 공간(가상 OS 로비)을 구축하고, 그 위에 9:16 모바일 세로형 및 PC 와이드 반응형 UI 기능 모듈 뼈대(UXML/USS)를 오버레이한다.
- **Edge Cases:**
  - 화면 비율이 9:16(모바일 세로)에서 16:9(PC 가로)로 변경될 때 수평선 뷰포트와 UI 모듈이 찌그러짐 없이 리플로우되어야 함.
  - 바닥 반사 연출은 모바일 저사양 기기에서도 부하 없이 동작하도록 가벼운 평면 렌더링 방식을 사용해야 함.

## 2. 상태 및 데이터 규격 (StateAndDataSpec)
- **Modules:**
  - `#TopStatusBar`: 시간, 배터리, 결계 동기화율(Sync) 투명 상단바
  - `#HorizonViewport`: 2.5D 우유니 수평선 반사 공간 뷰포트 (캐릭터 배회 영역)
  - `#AnomalyWidget`: 결계 이상 징후 브리핑 및 SOS 알림 위젯
  - `#AppGrid`: 4대 코어 앱 독 (`교과 아카이브`, `가상 SNS`, `스킬 덱`, `환경 설정`)
  - `#DiveCta`: 3D 결계 다이브(출격) 메인 버튼
  - `#QuickDock`: 하단 긴급 구원 퀴즈 및 설정 숏컷
- **Layout Tokens:**
  - `.mobile-portrait`: 세로 9:16 단일 컬럼 스택
  - `.pc-landscape`: 가로 16:9 와이드 2분할 뷰

## 3. 수정 경계선 제약 (BoundaryConstraints)
- **Allowed Paths:**
  - `docs/tasks/active/task-002-personal-horizon-lobby-layout.md`
  - `Assets/UI/Lobby/*`
  - `Assets/Scenes/PersonalHorizonLobby.unity`
  - `Assets/Scripts/UI/Lobby/*`
- **Prohibited Paths:**
  - `Assets/Scripts/Core/*`
  - `Assets/Scripts/Combat/*`
  - `Assets/Editor/*` (디렉터 제외)
- **Forbidden Patterns:**
  - 생성 코드 내 주석 일절 추가 금지
  - Update 내 매 프레임 `VisualElement.Q<T>()` 및 GC 힙 할당(`new`) 금지

## 4. 완료 검증 기준 (Definition of Done)
1. 유니티 에디터 C# 컴파일 에러/경고 0건 검증
2. 우유니 소금사막형 수평선 개인 공간 씬 및 9:16 모바일/PC 반응형 와이어프레임(UXML/USS) 구축
3. 주석 없는 깔끔한 코드 준수
4. 허용 경로 외 프로젝트 코어 파일 무수정 확인
