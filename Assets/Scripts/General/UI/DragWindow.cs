using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragWindow : MonoBehaviour, IDragHandler
{
    public RectTransform dragRectTransform;
    public Canvas canvas;

    public void OnDrag(PointerEventData eventData)
    {
        dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
