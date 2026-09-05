using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;

    // 마우스로 클릭한 UI 안의 오프셋 위치를 기억할 변수
    private Vector2 dragOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = rectTransform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentRectTransform == null)
        {
            return;
        }

        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform, eventData.position, eventData.pressEventCamera, out localMousePos))
        {
            dragOffset = localMousePos - rectTransform.anchoredPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentRectTransform == null)
        {
            return;
        }

        Vector2 localMousePos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform, eventData.position, eventData.pressEventCamera, out localMousePos))
        {
            rectTransform.anchoredPosition = localMousePos - dragOffset;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 종료");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
        Debug.Log("OnPointerDown : <color=cyan><b>[UI 클릭됨]</b></color> 마우스 이벤트 정상 작동 중!");
    }
}
