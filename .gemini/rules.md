# Git Commit & Push Rule

## 커밋 및 푸시 워크플로우
1. 코드 수정 및 작업 완료 후 `git status`로 변경사항을 확인하고 커밋을 진행하세요.
2. 커밋 후 커밋 메시지(Title)와 상세 설명(Description)을 정리하여 보여주고 사용자에게 Push 승인을 요청하세요.

## 커밋 메시지 작성 규칙
- **Title (첫 줄)**: Conventional Commits 형식을 준수하여 영문으로 작성 (예: `fix(editor): fix GlyphRenderMode typo in TMP font scripts`)
- **Description (본문)**: 
  - 한글로 작성하며, 개조식 불릿 포인트 (`- `) 형식 사용
  - 문장 끝은 **명사형 종결어미**(~함, ~수정, ~해결, ~반영 등)로 작성
  - 예시: `- RebuildKoPubFont.cs 등 에디터 스크립트 내 GlyphRenderMode.SDFF 오타를 SDFAA로 수정하여 컴파일 에러 해결`
