using System;
using FCS_AlterraHub.Enumerators;
using FCS_HomeSolutions.SeaBreeze.Buildable;
using FCSCommon.Utilities;
using UnityEngine.EventSystems;

namespace FCS_HomeSolutions.SeaBreeze.Display
{
    internal class ItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TechType type = TechType.None;
        public int Amount { set; get; }
        public EatableType EatableType { set; get; }
        public Action<TechType, EatableType> OnButtonClick;
        public Action<bool> OnInterfaceButton;

        public TechType Type
        {
            set
            {
                TextLineOne = string.Format(SeaBreezeAuxPatcher.TakeItemFormat(), Language.main.Get(value));
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
                OnButtonClick?.Invoke(type, EatableType);
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
