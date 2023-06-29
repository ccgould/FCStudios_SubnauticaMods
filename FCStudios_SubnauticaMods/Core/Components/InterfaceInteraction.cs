using UnityEngine.EventSystems;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;

/// <summary>
/// This component is for use on canvas where the <see cref="HoverInteraction"/> needs to be ignored to allow UI clicking
/// </summary>

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public class InterfaceInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerHoverHandler
{
    public bool IsInRange { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsInRange = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsInRange = false;
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        IsInRange = true;
    }
}
