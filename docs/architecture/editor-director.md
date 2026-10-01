# StudyGameDirector & ProBuilder Level Tooling Architecture

이 문서는 StudyGame의 에피소드 시퀀싱 허브인 **`StudyGameDirector`**와 **`ProBuilder` 기반 절차적 레벨 양산 도구**의 구조 및 에디터-런타임 격리 원칙을 정의하는 명세서다.

---

## 1. StudyGameDirector 시스템 개요

`StudyGameDirector`는 유니티 에디터 내에서 에피소드 흐름, 씬 전환, 시나리오 노드 연결, 3D 결계 맵 생성을 시각적으로 제어하는 자체 제작 커스텀 에디터 도구다.

### 핵심 구성 요소 및 로컬 소스 매핑
- **중앙 허브 (`DirectorEditorWindow.cs`, `StudyGameDirectorHub.cs`):** 그래프 뷰 기반으로 에피소드별 노드 시퀀스를 편집하는 메인 에디터 윈도우.
- **스테이지 블루프린트 (`StageBlueprintData.cs`):** 특정 챕터/에피소드가 요구하는 과목 테마, 등장 괴이 프로필, 필요 교과 개념 태그, 보스 결계 형태를 담은 데이터 직렬화 규격.
- **노드 시스템 (`ScenarioSequenceNode.cs`, `DirectorGraphView.cs`, `WindowNode.cs`):** 시나리오 대화(YarnSpinner 연동), 3D 결계 진입, 가상 OS 복귀 시퀀스를 노드 단위로 시각화.
- **프록시 및 상태 관리 (`NodeProxyManager.cs`, `DirectorStateManager.cs`, `DirectorSaveData.cs`):** 에디터 세션 간 작업 상태 보존 및 JSON 데이터 직렬화.

---

## 2. ProBuilder 절차적 맵 생성 도구군

로컬 `Assets/Editor/`에 구축된 13종의 ProBuilder 생성기는 런타임에 실행되는 무거운 코드가 아니며, 에디터 타임에 고품질 3D 메쉬와 콜라이더를 사전 베이킹(Bake)하는 양산 툴이다.

### 맵 생성기 분류
1. **학문 테마별 연구실 및 교실 맵:**
   - 언어/문법: `CreateLanguageLabProBuilder.cs`, `CreateLanguageProofreadingLabProBuilder.cs`, `CreateSyntaxCorridorProBuilder.cs`
   - 수학/통계: `CreateAlgorithmStatisticsRoomProBuilder.cs`, `CreateGeometryClassroomProBuilder.cs`
   - 과학/물리: `CreateMolecularCultivationRoomProBuilder.cs`, `CreateVacuumDynamicsLabProBuilder.cs`
   - 역사/인문: `CreateChronologyArchivesProBuilder.cs`, `CreateGrandLibraryAcademyProBuilder.cs`, `CreateMootCourtDebateRoomProBuilder.cs`
2. **전투 및 결계 아레나 맵:**
   - 대형 투기장: `CreateLargeArenaProBuilder.cs`, `CreateCentralBroadcastingStudioProBuilder.cs`
   - 외부/테라스: `CreateTerraceProBuilder.cs`, `CreateCartographyObservatoryProBuilder.cs`

### 레벨 디자인 3대 원칙
- **부유형 파편 결계 (Floating & Void):** 사방이 꽉 막힌 답답한 벽을 지양하고, 책상/칠판 등 학교 사물들이 허공에 떠서 경계선(사물 범퍼)을 형성한다.
- **카메라 사각지대 제로:** 3인칭 프리 백뷰가 벽에 가려지지 않도록 평평하고 넓은 중앙 공간을 유지한다.
- **원클릭 프리팹 베이킹:** ProBuilder 스크립트로 생성된 지형은 최종적으로 정적 콜라이더가 포함된 프리팹(Prefab)으로 변환되어 씬에 배치된다.

---

## 3. 에디터 ↔ 런타임 물리적 격리 원칙 (MANDATORY)

- **어셈블리 오염 금지:**
  - `Assets/Editor/` 및 `Assets/Editor/StudyGameDirector/` 내부의 모든 코드는 `UnityEditor` API를 사용하므로, 런타임 스크립트(`Assets/Scripts/`)의 Assembly Definition(`*.asmdef`)에서 절대 참조할 수 없다.
- **데이터 기반 소통 (Loose Coupling):**
  - 에디터 도구는 런타임 인스턴스를 직접 조작하지 않고, 오직 `StageBlueprintData`나 ScriptableObject 에셋을 파일로 디스크에 저장하는 방식으로만 런타임에 데이터를 전달한다.
- **안전한 MCP 작업:**
  - Unity MCP를 통해 에디터 도구를 호출할 때, `DirectorSaveData.cs` 및 `StudyGameDirectorData.json`의 무결성을 항상 보존하며 임의로 JSON 스키마를 변경하지 않는다.
