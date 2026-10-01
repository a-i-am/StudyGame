# Skill & Concept Socket Architecture

이 문서는 StudyGame의 전투 스킬 아키텍처인 **[뼈대 스킬(Base) + 개념 소켓(Socket)]**과 **[5대 코어 상태이상 모듈]**의 데이터 구조 및 작동 원리를 정의하는 명세서다.

---

## 1. 스킬 계층의 본질 및 2원화 분리

### 유저 체감 계층 (In-game UI Presentation)
유저에게는 공학적 모듈명이 노출되지 않으며, 직관적인 무기 튜닝 룩앤필을 제공한다.
- **과목 대분류:** 국어, 수학, 영어, 탐구, 역사 등
- **뼈대 스킬 (Base Skill):** 과목의 핵심 영역을 상징하는 기본 물리적 공격 형태 (모션, 투사체 형태)
- **개념 소켓 (Concept Socket):** 하위 교과 개념을 룬처럼 끼워 넣어 스킬의 특성과 기믹 파훼력을 변환

**인게임 표기 예시:**
```text
뼈대 스킬: [독서] - 논리적 꿰뚫기 (기본 형태: 전방을 향해 쏘는 관통형 직선 레이저)
 ├── 개념 소켓 1: [인과관계] (효과: 사거리가 멀어질수록 데미지 최대 2.5배 증폭)
 └── 개념 소켓 2: [비례/반비례] (효과: 플레이어 체력이 낮을수록 레이저 굵기가 2배 확장)
```

### 개발자 데이터 계층 (ScriptableObject & Data-Driven)
'5대 코어 모듈'은 상위 계층이 아니라 `ConceptSocketSO` 내부의 **로직 분기용 `CoreModuleType` Enum 변수**다.
새로운 개념이 추가되어도 C# 스크립트를 새로 짜지 않고, ScriptableObject의 수치 조정만으로 무한 양산한다.

---

## 2. 5대 코어 상태이상 모듈 (Core Modules)

프로그래머는 아래 5개 모듈의 물리/상태이상 로직만 견고하게 구현하며, 교과 텍스트와 변수는 ScriptableObject로 주입된다.

| 모듈 | 물리적 작용 (공통 엔진 로직) | 할당 교과 개념 예시 (데이터 주도 매핑) |
| :--- | :--- | :--- |
| **모듈 A (응축 / 당김)** | 적 이동 속도 0, 타격 지점 중심 몹몰이 | 수학(수렴, 극한), 국어(요약, 귀납), 역사(중앙집권) |
| **모듈 B (팽창 / 밀쳐냄)** | 강력한 원뿔형 넉백, 적 방패(무적 가드) 파괴 | 수학(발산, 무한대), 국어(과장법, 연역), 역사(영토 확장) |
| **모듈 C (역전 / 반사)** | 적 투사체 궤적 및 판정 벡터 180도 반전 | 수학(역함수, 반비례), 국어(역설법, 반어법), 과학(작용-반작용) |
| **모듈 D (정지 / 동결)** | 적의 특정 기믹 패턴 캐스팅 강제 차단 | 수학(상수), 국어(정적 이미지/심상), 과학(절대영도) |
| **모듈 E (연쇄 / 반응)** | 상태이상 피격 적 사망 시 주변 연쇄 폭발 | 수학(점화식, 등비수열), 국어(연쇄법), 과학(연쇄 핵반응) |

---

## 3. C# 데이터 계약 구조 (Data Contracts)

```csharp
public enum SubjectType {
    Korean,
    Math,
    English,
    Science,
    History
}

public enum AttackForm {
    StraightLaser,
    RadialWave,
    Projectile,
    MeleeSlash,
    BarrierShield
}

public enum CoreModuleType {
    ModuleA_PullAndCondense,
    ModuleB_KnockbackAndPierce,
    ModuleC_ReverseAndReflect,
    ModuleD_FreezeAndSilence,
    ModuleE_ChainReaction
}

public class BaseSkillSO : ScriptableObject {
    public SubjectType subject;
    public string skillName;
    public AttackForm form;
    public float baseDamage;
    public float baseCooldown;
    public GameObject baseVfxPrefab;
    public int maxSocketSlots = 2;
}

public class ConceptSocketSO : ScriptableObject {
    public string conceptId;
    public string conceptName;
    public SubjectType subject;
    public CoreModuleType moduleType;
    public float conditionThreshold;
    public float effectMultiplier;
    public float duration;
    public Color vfxTint;
    public float vfxScaleMultiplier = 1.0f;
    public string targetWeaknessTag;
    public bool overrideSubjectTrait;
}
```

---

## 4. 하이브리드 연출 및 과목 규칙 (Hybrid VFX & Traits)

- **하이브리드 VFX 원칙:**
  - 스킬의 내부 로직 자체를 새로 짜지 않는다.
  - 데이터 수치 변화 시 파티클 스케일, 타격음, 카메라 셰이크를 극대화하여 유저에게 "스킬 메커니즘 자체가 물리적으로 달라졌다"는 손맛을 제공한다.
- **과목별 고유 패시브 (Subject Traits):**
  - 수학: 즉발/단발성 타격 (발동 속도 0.1초, 크리티컬 확률 보정)
  - 국어: 지속성 도트 타격 (효과 지속 시간 1.5배, 범위 확장)
  - 과학: 연쇄 반응 계수 증가
- **특수 룰 우선 원칙 (Specific Overrides General):**
  - 수학의 `무한대`나 `상수`처럼 과목의 기본 템포(즉발)를 거스르는 특수 개념은 `overrideSubjectTrait = true` 플래그를 통해 고유 지속시간을 강제 적용한다. (에픽/희귀 등급 소켓)
