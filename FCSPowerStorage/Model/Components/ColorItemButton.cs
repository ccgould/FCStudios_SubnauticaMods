using FCSPowerStorage.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCSPowerStorage.Model.Components
{
    /// <summary>
    /// This class handles operations for the color picker
    /// </summary>
    public class ColorItemButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Public Properties
        /// <summary>
        /// The color value for the color picker button. This is used to change the color of the button and for the base color
        /// </summary>
        public Color Color { get; set; }
        #endregion

        #region Overrides
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

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
            ColorChanger.ChangeBodyColor(transform.parent.parent.parent.parent.parent.gameObject, FcsPowerStorageDisplay, Color);
        }
        #endregion
    }
}
