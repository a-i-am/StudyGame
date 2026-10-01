# Evaluation Rubric: UI Architecture Implementation

## 1. Description
Evaluate the implementation of the UI Architecture Plan PoC (Proof of Concept) which includes 3 UXML files and 3 C# partial classes in `Assets/UI/UXML/` and `Assets/UI/Views/`.

## 2. Functional Requirements
- [ ] **AnimeLobbyView.uxml / .cs**: Must exist. Must use ZoboUI layout classes (`flex-row`, etc.) and Sinanata DS tokens (`ds-btn`, `ds-panel`). The C# class must be a `partial class` and not contain any `this.Q<T>()` boilerplate.
- [ ] **StreamExperienceView.uxml / .cs**: Must exist. The C# class must implement a `StringBuilder` based text buffering with an `_isDirty` flag pattern for optimization.
- [ ] **VisualNovelView.uxml / .cs**: Must exist. The C# class must demonstrate `RegisterCallback<DetachFromPanelEvent>` for memory cleanup.
- [ ] **UIGalleryDirector.cs**: The `GetDescriptionForUXML` method must contain entries for `AnimeLobbyView`, `StreamExperienceView`, and `VisualNovelView`.

## 3. Style and Constraints
- Code must be clean and uncommented (as per rule).
- C# files must use `public partial class`.

## 추가 요구사항 (Micro-loop Additions)
