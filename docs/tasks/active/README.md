# Active Tasks Directory

이 디렉터리는 현재 진행 중인 **단일 기능 작업 명세서(`task-xxx.md`)**를 보관하는 공간이다.

- **원칙:** AI 에이전트는 세션 시작 시 이 폴더에 존재하는 활성 명세서 1개만 주입받아 작업을 수행한다.
- **수명 주기:** 작업이 완료되고 Definition of Done(DoD) 검증이 모두 통과되면, 해당 명세서는 `docs/tasks/archive/`로 이동한다.
- **포맷 규격:** `docs/tasks/schema/task-manifest.schema.json` 참조.
