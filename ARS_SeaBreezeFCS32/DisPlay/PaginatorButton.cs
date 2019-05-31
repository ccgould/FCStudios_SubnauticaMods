using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARS_SeaBreezeFCS32.Display
{
    internal class PaginatorButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {

        private static readonly Color HOVER_COLOR = new Color(0.07f, 0.38f, 0.7f, 1f);
        private static Color STARTING_COLOR = Color.white;
        public int AmountToChangePageBy { get; set; } = 1;
        private Image image;
        public string HoverTextLineOne { get; set; }
        public string HoverTextLineTwo { get; set; }

        public Action<int> OnChangePageBy { get; set; }

        public Action<bool> OnInterfaceButton;
        public void Start()
        {
            image = GetComponent<Image>();
            STARTING_COLOR = image.color;
            TextLineOne = HoverTextLineOne;
            TextLineTwo = HoverTextLineTwo;
        }

        public void OnEnable()
        {
            if (image != null)
            {
                image.color = STARTING_COLOR;
            }
        }

        public override void OnDisable()
        {
            if (image != null)
            {
                image.color = STARTING_COLOR;
            }
            base.OnDisable();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                OnInterfaceButton?.Invoke(true);
                image.color = HOVER_COLOR;
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            OnInterfaceButton?.Invoke(false);
            image.color = STARTING_COLOR;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (IsHovered)
            {
                OnChangePageBy?.Invoke(AmountToChangePageBy);
            }
        }
    }
}
