using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StudyGame.UI
{
    public class GenericDragAndDropHandler<TData> where TData : class
    {
        private VisualElement targetElement;
        private VisualElement rootVisualElement;
        private VisualElement ghostElement;

        private Vector2 pointerStartPos;
        private Vector3 targetStartPos;
        private bool isDragging;

        public TData BoundData { get; private set; }

        public event Action<GenericDragAndDropHandler<TData>, Vector2> OnDragStarted;
        public event Action<GenericDragAndDropHandler<TData>, Vector2> OnDragMoved;
        public event Action<GenericDragAndDropHandler<TData>, Vector2, VisualElement> OnDragEnded;

        public GenericDragAndDropHandler(VisualElement element, TData data, VisualElement root)
        {
            targetElement = element;
            BoundData = data;
            rootVisualElement = root;

            targetElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            targetElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            targetElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
            targetElement.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        public void Unregister()
        {
            if (targetElement == null) return;
            targetElement.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            targetElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            targetElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            targetElement.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            pointerStartPos = evt.position;
            targetElement.CapturePointer(evt.pointerId);
            isDragging = true;

            CreateGhost(evt.position);
            OnDragStarted?.Invoke(this, evt.position);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || !targetElement.HasPointerCapture(evt.pointerId)) return;

            if (ghostElement != null)
            {
                ghostElement.style.left = evt.position.x - (ghostElement.resolvedStyle.width / 2f);
                ghostElement.style.top = evt.position.y - (ghostElement.resolvedStyle.height / 2f);
            }

            OnDragMoved?.Invoke(this, evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!isDragging || !targetElement.HasPointerCapture(evt.pointerId)) return;

            targetElement.ReleasePointer(evt.pointerId);
            EndDrag(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (isDragging)
            {
                EndDrag(pointerStartPos);
            }
        }

        private void EndDrag(Vector2 finalPosition)
        {
            isDragging = false;

            RemoveGhost();
            VisualElement droppedTarget = rootVisualElement?.panel?.Pick(finalPosition);

            OnDragEnded?.Invoke(this, finalPosition, droppedTarget);
        }

        private void CreateGhost(Vector2 startPos)
        {
            if (rootVisualElement == null) return;

            ghostElement = new VisualElement();
            ghostElement.style.position = Position.Absolute;
            ghostElement.style.width = targetElement.resolvedStyle.width > 0 ? targetElement.resolvedStyle.width : 140;
            ghostElement.style.height = targetElement.resolvedStyle.height > 0 ? targetElement.resolvedStyle.height : 80;
            ghostElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 1.0f, 0.6f));
            ghostElement.style.borderTopLeftRadius = 8;
            ghostElement.style.borderTopRightRadius = 8;
            ghostElement.style.borderBottomLeftRadius = 8;
            ghostElement.style.borderBottomRightRadius = 8;
            ghostElement.style.left = startPos.x - (ghostElement.style.width.value.value / 2f);
            ghostElement.style.top = startPos.y - (ghostElement.style.height.value.value / 2f);
            ghostElement.pickingMode = PickingMode.Ignore;

            rootVisualElement.Add(ghostElement);
        }

        private void RemoveGhost()
        {
            if (ghostElement != null && rootVisualElement != null)
            {
                rootVisualElement.Remove(ghostElement);
                ghostElement = null;
            }
        }
    }
}
