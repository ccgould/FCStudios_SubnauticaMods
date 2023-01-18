using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEventTrigger : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerHoverHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public void OnPointerDown(PointerEventData eventData)
    {
        if (base.enabled && this.onPointerDown != null)
        {
            this.onPointerDown.Invoke(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (base.enabled && this.onPointerUp != null)
        {
            this.onPointerUp.Invoke(eventData);
        }
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        if (base.enabled && this.onPointerHover != null)
        {
            this.onPointerHover.Invoke(eventData);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (base.enabled && this.onInitializePotentialDrag != null)
        {
            this.onInitializePotentialDrag.Invoke(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (base.enabled && this.onBeginDrag != null)
        {
            this.onBeginDrag.Invoke(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (base.enabled && this.onDrag != null)
        {
            this.onDrag.Invoke(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (base.enabled && this.onEndDrag != null)
        {
            this.onEndDrag.Invoke(eventData);
        }
    }

    public PointerEventTrigger.PointerEvent onPointerDown;

    public PointerEventTrigger.PointerEvent onPointerUp;

    public PointerEventTrigger.PointerEvent onPointerHover;

    public PointerEventTrigger.PointerEvent onInitializePotentialDrag;

    public PointerEventTrigger.PointerEvent onBeginDrag;

    public PointerEventTrigger.PointerEvent onDrag;

    public PointerEventTrigger.PointerEvent onEndDrag;

    [Serializable]
    public class PointerEvent : UnityEvent<PointerEventData>
    {
    }
}
