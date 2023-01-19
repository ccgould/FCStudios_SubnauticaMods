using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEventTrigger : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerHoverHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public void OnPointerDown(PointerEventData eventData)
    {
        QuickLogger.Info("OnPointerDown", true);
        if (base.enabled && this.onPointerDown != null)
        {
            this.onPointerDown.Invoke(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        QuickLogger.Info("OnPointerUp", true);
        if (base.enabled && this.onPointerUp != null)
        {
            this.onPointerUp.Invoke(eventData);
        }
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        QuickLogger.Info("OnPointerHover", true);
        if (base.enabled && this.onPointerHover != null)
        {
            this.onPointerHover.Invoke(eventData);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        QuickLogger.Info("OnInitializePotentialDrag", true);
        if (base.enabled && this.onInitializePotentialDrag != null)
        {
            this.onInitializePotentialDrag.Invoke(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        QuickLogger.Info("OnBeginDrag", true);
        if (base.enabled && this.onBeginDrag != null)
        {
            this.onBeginDrag.Invoke(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        QuickLogger.Info("OnDrag", true);
        if (base.enabled && this.onDrag != null)
        {
            this.onDrag.Invoke(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        QuickLogger.Info("OnEndDrag", true);
        if (base.enabled && this.onEndDrag != null)
        {
            this.onEndDrag.Invoke(eventData);
        }
    }

    public PointerEventTrigger.PointerEvent onPointerDown = new();

    public PointerEventTrigger.PointerEvent onPointerUp = new();

    public PointerEventTrigger.PointerEvent onPointerHover = new();

    public PointerEventTrigger.PointerEvent onInitializePotentialDrag = new();

    public PointerEventTrigger.PointerEvent onBeginDrag = new();

    public PointerEventTrigger.PointerEvent onDrag = new();

    public PointerEventTrigger.PointerEvent onEndDrag = new();

    [Serializable]
    public class PointerEvent : UnityEvent<PointerEventData>
    {
    }
}
