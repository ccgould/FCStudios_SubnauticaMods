using FCSCommon.Utilities;
using System;
using UnityEngine.EventSystems;

namespace ARS_SeaBreezeFCS32.Display
{
    internal class ItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TechType type = TechType.None;
        public int Amount { set; get; }
        public Action<TechType> OnButtonClick;
        public Action<bool> OnInterfaceButton;

        public TechType Type
        {
            set
            {
                TextLineOne = "Take " + TechTypeExtensions.Get(Language.main, value);
                type = value;
            }

            get => type;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                OnInterfaceButton?.Invoke(true);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            QuickLogger.Debug($"Clicked on ItemButton", true);
            if (IsHovered && type != TechType.None)
            {
                OnButtonClick?.Invoke(type);
                OnInterfaceButton?.Invoke(false);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            OnInterfaceButton?.Invoke(false);
        }
    }
}
