# 검사 채점표 (Evaluation Rubric) - Open Source UI & Tool Integration

이 채점표는 GitHub Copilot CLI가 Antigravity의 작업물을 검사할 때 사용하는 기준입니다.

## 공통 기준 (Common Criteria)
- [ ] 문법적 오류(Syntax Error)나 명백한 런타임 버그가 없는가?
- [ ] 보안 취약점(예: 하드코딩된 비밀번호나 API 키 노출)이 없는가?

## 기능 요구사항 (Functional Requirements)
- [ ] OpenSource_Showcase.uxml과 USS가 ZoboUI, UI Toolkit Design System, UIToolkitExtensions를 정상적으로 참조하고 있는가?
- [ ] 생성되거나 수정된 C#, UXML, USS 코드에 주석(//, /* */, <!-- -->)이 0개(전무)인가?
- [ ] 기존 Combat/Core 시스템 코드와 격리되어 경계 무결성이 유지되는가?

## 추가 요구사항 (Micro-loop Additions)
