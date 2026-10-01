# StudyGame Persona & Workflow Rules Backup

## 1. 개인 설정 (Personal Preferences)
- 코딩 스타일: 생성된 코드에 주석을 일절 추가하지 마세요. 주석 없는 깔끔한 코드를 제공하세요.

## 2. 메타 동작 (포니테일 모드 / Ponytail Mode)
현재 프로젝트(StudyGame)에서는 폴더 경로와 무관하게 매 턴마다 항상 다음 원칙을 준수해야 합니다.
- 코드를 제안하거나 수정할 때는 항상 먼저 /ponytail-audit, /ponytail-debt, /ponytail-gain, /ponytail-review 관점에서 분석하세요.
- 오버엔지니어링(YAGNI)을 엄격히 방지하고, 최소한의 복잡도로 현재 필요한 기능만을 구현하세요.
- 가장 중요: 코드를 작성하거나 수정하기 전에는 반드시 저(사용자)에게 실행 허가를 구하고 승인을 기다리세요.
- 엄격한 UI 규칙: 아무리 편리해 보이더라도 명시적인 허가 없이 버튼이나 UI 요소를 절대 추가하지 마세요. 강제로 씬(Scene)을 전환하거나 사용자 설정을 우회하지 마세요.

## 3. 게임 폴리싱 및 연출 방향 (Game Polish & Direction)
'상용 모바일/PC 게임의 완성도 기준'에 비추어 현재 게임을 검토하세요. 새로운 콘텐츠를 추가하기보다, 먼저 '기존 게임플레이가 충분히 전달되지 않거나 전반적인 퀄리티를 떨어뜨리는 요소'를 찾으세요.
- 단순히 화려하게만 만들지 말고, 의도가 명확하고 일관되며 플레이어가 자신의 행동에 따른 원인과 결과를 즉각 이해할 수 있는 상태를 목표로 하세요.
- 현재 프로젝트에서 중복된 이펙트, 중복 코드, 미사용 기능, 역할이 겹치는 UI, 일관되지 않은 스타일을 식별하고, 이에 대해 유지 / 병합 / 삭제 여부를 제안하세요.

## 4. 분석 파이프라인 (Analysis Pipeline)
중요한 이벤트와 인터랙션(특히 로직은 존재하나 연출이 부족한 이벤트)을 파악할 때는 다음 흐름에 따라 분석하세요.
- [입력 → 예비 동작(Anticipation) → 실행(Action) → 충돌/변화 → 결과 → 회복/정리]
- 체크리스트: 피드백(시각/청각/햅틱), 트윈/선형보간, 스쿼시&스트레치, 히트스톱, 파티클/사운드 동기화, UI/UX, 연속적 경험.

## 5. 출력 형식 (Output Format)
1. [게임 폴리싱 제안] (우선순위 1, 2, 3)
2. [포니테일 코드 분석] (/ponytail-audit, /ponytail-debt, /ponytail-gain, /ponytail-review)
3. [실행 허가 요청]

## 6. GUI Layout Validation & Commit Convention
- GUI Layout: dynamic text space, visual layout rules.
- Commit Convention: description 명사형 개조식 사전 승인.
