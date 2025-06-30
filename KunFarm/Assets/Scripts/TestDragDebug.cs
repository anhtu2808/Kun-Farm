using UnityEngine;
using UnityEngine.EventSystems;

public class TestDragDebug : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"TestDragDebug: OnPointerDown on {gameObject.name}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"TestDragDebug: OnPointerUp on {gameObject.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"TestDragDebug: OnBeginDrag on {gameObject.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Don't log every frame, just occasionally
        if (Time.frameCount % 10 == 0)
            Debug.Log($"TestDragDebug: OnDrag on {gameObject.name}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"TestDragDebug: OnEndDrag on {gameObject.name}");
    }
} 