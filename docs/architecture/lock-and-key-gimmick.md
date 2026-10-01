# Lock & Key Gimmick & Weakness Architecture

이 문서는 StudyGame의 몬스터 약점 파훼 메커니즘인 **[자물쇠(Lock)와 열쇠(Key)]** 패턴과 **[로직-연출 분리 원칙]**을 정의하는 아키텍처 명세서다.

---

## 1. 자물쇠-열쇠 패턴의 핵심 설계 원리

개별 몬스터마다 기믹 파훼 스크립트를 새로 작성하지 않는다. 모든 몬스터와 환경 퍼즐은 단 하나의 범용 컴포넌트인 **`WeaknessReceiver`**를 공유한다.

- **발동 주체 (Sender):** 플레이어의 타격 데이터 패킷 (`Damage`, `WeaknessTag[]`)
- **수신 객체 (Receiver):** 몬스터나 퍼즐에 부착된 `WeaknessReceiver` 컴포넌트
- **핵심 분기:** 날아온 태그 배열에 자신이 요구하는 정답 태그가 포함되어 있는지 단 한 줄로 판별

```text
[플레이어 타격: 데미지 50, 태그: [Math_Limit, Math_Convergence]]
      │
      ▼ (충돌 판정)
[몬스터 WeaknessReceiver: 요구 태그 = Math_Convergence]
      │
      ├── 일치(정답) ➔ '절대 논리 장벽' 해제 + '그로기(인식 붕괴)' 상태 전이 ➔ OnGimmickSolved 이벤트 호출
      └── 불일치(오답) ➔ 데미지 무효화(0 처리) + '논리 부적합' 힌트 시각 피드백
```

---

## 2. 로직(Logic)과 연출(Presentation)의 완전한 분리

- `WeaknessReceiver`는 오직 상태 전이와 이벤트 발행만 수행한다.
- 몬스터별 고유 연출(방패 산산조각, 기괴한 비명, 특정 애니메이션)은 인스펙터 상에서 `UnityEvent` 또는 C# 델리게이트를 통해 외부 컴포넌트에 연결한다.
- 이를 통해 신규 몬스터와 기믹을 추가할 때 코딩 없이 데이터 에셋과 연출 바인딩만으로 무한 확장이 가능하다.

```csharp
public class HitData {
    public float damage;
    public string[] tags;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}

public class WeaknessReceiver : MonoBehaviour {
    [SerializeField] private string requiredTag;
    [SerializeField] private bool isGimmickActive = true;
    
    public event Action OnGimmickSolved;
    public event Action OnGimmickFailed;

    public bool EvaluateHit(HitData hit) {
        if (!isGimmickActive) return true;
        
        for (int i = 0; i < hit.tags.Length; i++) {
            if (hit.tags[i] == requiredTag) {
                isGimmickActive = false;
                OnGimmickSolved?.Invoke();
                return true;
            }
        }

        OnGimmickFailed?.Invoke();
        return false;
    }
}
```

---

## 3. 기믹 레벨 디자인 및 난이도 스캐폴딩

유저가 맹목적인 찍기(가위바위보)로 기믹을 통과하는 것을 막고, 교육적 추론의 재미를 단계별로 제공한다.

### 1) 일반 몬스터 (형성평가 / 튜토리얼)
- 몬스터 머리 위에 요구 태그(예: `[등차수열]`, `[문장구조]`)가 UI로 명확히 표시된다.
- 유저는 UI를 보며 자신이 소켓팅한 스킬과 기믹 간의 1차원적 매칭을 가볍게 학습한다.

### 2) 보스전 및 심화 미션 (총괄평가 / 실전 추론)
- 몬스터 머리 위의 UI 태그가 블라인드(가림) 처리된다.
- 몬스터는 물리적 이상 행동(끝없는 자가 분열, 원거리 투사체 100% 반사막, 돌진 무적 등)을 시각적으로 보여준다.
- 유저는 현상을 관찰하고 "분열하니까 수렴/극한으로 묶어야 한다", "반사하니까 역설/반어로 뒤집어야 한다"는 교과 논리를 스스로 추론하여 공략한다.

---

## 4. 텍티컬 일시정지 (Tactical Pause & OS 모드)

- **최초 1~2회 조우:**
  - 보스가 특수 기믹 패턴을 시전할 때 `Time.timeScale`을 0.1배속으로 감속(불릿 타임).
  - 화면 상단에 스마트폰 OS 경고 텍스트(예: `[System: 개체 수 지수함수적 팽창 감지. 질량의 '응축/수렴' 필요]`) 팝업 및 기계음 TTS 출력.
  - 유저에게 텍스트를 읽고 소켓을 교체할 수 있는 물리적 시간(1~2초) 부여.
- **3회 이상 숙련 페이즈:**
  - 불릿 타임 발동 없이 좌측 상단 미니 토스트 로그로 전환되어 하이스피드 액션의 템포를 보장.
  - 가상 OS 환경설정에서 유저가 튜토리얼 어시스트 수준을 자유롭게 On/Off 가능.
