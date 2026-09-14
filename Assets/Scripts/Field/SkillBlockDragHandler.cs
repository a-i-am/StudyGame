using UnityEngine;
using UnityEngine.UIElements;

public class SkillBlockDragHandler
{
    private VisualElement targetElement;
    private VisualElement dropTargetSlot;
    private System.Action<VisualElement> onDropCallback;
    private bool isDragging = false;
    private Vector2 clickOffset;
    private VisualElement originalParent;
    private int originalIndex;

    public SkillBlockDragHandler() { }

    public SkillBlockDragHandler(VisualElement element, VisualElement targetSlot, System.Action<VisualElement> onDropCallback = null)
    {
        this.dropTargetSlot = targetSlot;
        this.onDropCallback = onDropCallback;
        RegisterCallbacks(element);
    }

    public void RegisterCallbacks(VisualElement element)
    {
        targetElement = element;
        targetElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        targetElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        targetElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (targetElement == null) return;

        isDragging = true;
        targetElement.CapturePointer(evt.pointerId);

        originalParent = targetElement.parent;
        if (originalParent != null)
        {
            originalIndex = originalParent.IndexOf(targetElement);
        }

        Rect worldBound = targetElement.worldBound;
        clickOffset = new Vector2(evt.position.x - worldBound.x, evt.position.y - worldBound.y);

        VisualElement rootContainer = targetElement.panel.visualTree;
        VisualElement mainRoot = rootContainer.Q<VisualElement>("Root");
        VisualElement dragOverlay = mainRoot ?? rootContainer;

        dragOverlay.Add(targetElement);

        targetElement.style.position = Position.Absolute;
        targetElement.style.left = evt.position.x - clickOffset.x;
        targetElement.style.top = evt.position.y - clickOffset.y;
        targetElement.style.scale = new StyleScale(new Scale(new Vector3(1.1f, 1.1f, 1f)));
        targetElement.style.opacity = 0.95f;
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging || !targetElement.HasPointerCapture(evt.pointerId)) return;

        targetElement.style.left = evt.position.x - clickOffset.x;
        targetElement.style.top = evt.position.y - clickOffset.y;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!isDragging || !targetElement.HasPointerCapture(evt.pointerId)) return;

        isDragging = false;
        targetElement.ReleasePointer(evt.pointerId);

        bool droppedInSlot = false;
        if (dropTargetSlot != null && dropTargetSlot.worldBound.Overlaps(targetElement.worldBound))
        {
            droppedInSlot = true;
        }

        ResetPosition();

        if (droppedInSlot)
        {
            onDropCallback?.Invoke(targetElement);
        }
    }

    public void ResetPosition()
    {
        if (targetElement != null)
        {
            if (originalParent != null && targetElement.parent != originalParent)
            {
                if (originalIndex >= 0 && originalIndex <= originalParent.childCount)
                {
                    originalParent.Insert(originalIndex, targetElement);
                }
                else
                {
                    originalParent.Add(targetElement);
                }
            }

            targetElement.style.position = StyleKeyword.Null;
            targetElement.style.left = StyleKeyword.Null;
            targetElement.style.top = StyleKeyword.Null;
            targetElement.style.scale = StyleKeyword.Null;
            targetElement.style.opacity = StyleKeyword.Null;
        }
    }
}