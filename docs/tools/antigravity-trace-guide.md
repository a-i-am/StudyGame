# Antigravity-trace 사용 가이드

`antigravity-trace`는 Google Antigravity IDE의 내부 LLM API 호출, 시스템 프롬프트, 도구 명세, 및 확장-에이전트 서버 간 통신 트래픽을 가로채어 로깅하는 분석 도구입니다.

---

## 1. 설치 경로
* **로컬 경로:** `d:\repos\tools\antigravity-trace\`

---

## 2. 사용 방법 (Python 환경)

### (1) 가상환경 및 의존성 설치
```bash
cd d:\repos\tools\antigravity-trace
python -m venv venv
.\venv\Scripts\activate
pip install -r requirements.txt
```

### (2) 프록시 후킹 활성화
```bash
python antigravity-trace.py --verbose
```
* Antigravity IDE 확장 경로에 섀도우 래퍼가 설치되어 트래픽을 가로챕니다.
* 로그는 `~/antigravity-trace` 폴더에 HTML 및 JSONL 형식으로 실시간 기록됩니다.

### (3) 프록시 제거 (원복)
```bash
python antigravity-trace.py --uninstall
```

---

## 3. 주의사항
* Antigravity IDE가 업데이트되면 확장이 무효화될 수 있으므로 필요 시 재설치 또는 언인스톨을 수행합니다.
* 유니티 런타임 프로젝트 코드와는 완전히 분리되어 외부 도구로 동작합니다.
