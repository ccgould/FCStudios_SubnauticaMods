using ExStorageDepot.Enumerators;
using FCSCommon.Utilities;
using System;
using UnityEngine.EventSystems;

namespace ExStorageDepot.Display
{
    internal class ItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TechType type = TechType.None;
        public int Amount { set; get; }
        public Action<TechType, RemovalType> OnButtonClick;
        public TechType Type
        {
            set
            {
                TextLineOne = "Take " + TechTypeExtensions.Get(Language.main, value);
                type = value;
            }

            get => type;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            QuickLogger.Debug($"Clicked on ItemButton", true);
            if (IsHovered && type != TechType.None)
            {
                OnButtonClick?.Invoke(type, RemovalType.Click);
            }
        }
    }
}
