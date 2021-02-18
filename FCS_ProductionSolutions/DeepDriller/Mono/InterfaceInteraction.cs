using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class InterfaceInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    }
}