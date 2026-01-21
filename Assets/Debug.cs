using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDebug : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnPointerDown(PointerEventData e) => Debug.Log("PointerDown");
    public void OnBeginDrag(PointerEventData e) => Debug.Log("BeginDrag");
    public void OnDrag(PointerEventData e) => Debug.Log("Drag");
    public void OnEndDrag(PointerEventData e) => Debug.Log("EndDrag");
}