using System;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_AlterraHub.Mono
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
        internal Color Color { get; set; }
        internal string BtnName { get; set; }
        private bool _isSelected;
        public Action<bool> OnInterfaceButton { get; set; }
        internal Action<string, object> OnButtonClick;
        private GameObject _hover;

        #endregion

        #region Overrides
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            OnInterfaceButton?.Invoke(true);
            SetHoverVisible();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {

            base.OnPointerExit(eventData);
            OnInterfaceButton?.Invoke(false);
            if (!_isSelected)
            {
                SetHoverVisible(false);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (this.IsHovered)
            {
                QuickLogger.Debug($"Clicked Button: {this.BtnName}", true);
                _isSelected = true;
                OnButtonClick?.Invoke(this.BtnName, Color);
            }
            else
            {
                QuickLogger.Debug("Button Not Hovered", true);
            }
        }
        #endregion


        /// <summary>
        /// Sets if the color item's hover ring is shown
        /// </summary>
        /// <param name="isSelected">Value to set the hover visible</param>
        internal void SetIsSelected(bool isSelected = true)
        {
            _isSelected = isSelected;
            SetHoverVisible(isSelected);

        }

        private void FindHover()
        {
            _hover = transform.gameObject.FindChild("Hover")?.gameObject;
        }


        private void SetHoverVisible(bool visible = true)
        {
            if (_hover == null)
            {
                FindHover();
            }

            _hover?.SetActive(visible);
        }
    }
}
