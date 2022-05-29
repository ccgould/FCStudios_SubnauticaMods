using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_AlterraHub.Model
{
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
}
