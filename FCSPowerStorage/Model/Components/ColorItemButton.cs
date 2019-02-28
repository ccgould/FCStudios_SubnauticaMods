using FCSPowerStorage.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCSPowerStorage.Model.Components
{
    public class ColorItemButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public Color Color { get; set; }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            //transform.gameObject.FindChild("Hover").SetActive(true);
            transform.gameObject.FindChild("Hover").SetActive(true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            transform.gameObject.FindChild("Hover").SetActive(false);

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            //Log.Info(transform.parent.parent.parent.parent.name);
            //Log.Info(transform.parent.gameObject.FindChild("/Power_Storage").name);
            ColorChanger.ChangeBodyColor(transform.parent.parent.parent.parent.parent.gameObject, FcsPowerStorageDisplay, Color);

        }
    }
}
