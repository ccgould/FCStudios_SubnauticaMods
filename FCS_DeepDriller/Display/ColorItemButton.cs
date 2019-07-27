using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_DeepDriller.Display
{
    /// <summary>
    /// This class handles operations for the color picker
    /// </summary>
    internal class ColorItemButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Public Properties
        /// <summary>
        /// The color value for the color picker button. This is used to change the color of the button and for the base color
        /// </summary>
        public Color Color { get; set; }
        public string BtnName { get; set; }

        public Action<string, object> OnButtonClick;
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

            if (this.IsHovered)
            {
                QuickLogger.Debug($"Clicked Button: {this.BtnName}", true);
                OnButtonClick?.Invoke(this.BtnName, Color);
            }
        }
        #endregion
    }
}
