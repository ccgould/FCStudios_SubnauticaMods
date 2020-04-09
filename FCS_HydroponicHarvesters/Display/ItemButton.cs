using System;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_HydroponicHarvesters.Display
{
    internal class ItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TechType type = TechType.None;
        public int Amount { set; get; }
        public Action<TechType> OnButtonClick;
        public Action<bool> OnInterfaceButton;
        public Color HOVER_COLOR { get; set; } = new Color(0.07f, 0.38f, 0.7f, 1f);
        public Color STARTING_COLOR { get; set; } = Color.white;
        public InterfaceButtonMode ButtonMode { get; set; } = InterfaceButtonMode.Background;
        public Text TextComponent { get; set; }
        public int SmallFont { get; set; } = 140;
        public int LargeFont { get; set; } = 180;
        public float IncreaseButtonBy { get; set; }


        public TechType Type
        {
            set
            {
                TextLineOne = "Take " + TechTypeExtensions.Get(Language.main, value);
                type = value;
            }

            get => type;
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            
            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    this.TextComponent.fontSize = this.TextComponent.fontSize;
                    break;
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponent<Image>() != null)
                    {
                        GetComponent<Image>().color = this.STARTING_COLOR;
                    }
                    break;
                case InterfaceButtonMode.BackgroundScale:
                    if (this.gameObject != null)
                    {
                        this.gameObject.transform.localScale = this.gameObject.transform.localScale;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                OnInterfaceButton?.Invoke(true);
                switch (this.ButtonMode)
                {
                    case InterfaceButtonMode.TextScale:
                        this.TextComponent.fontSize = this.LargeFont;
                        break;
                    case InterfaceButtonMode.TextColor:
                        this.TextComponent.color = this.HOVER_COLOR;
                        break;
                    case InterfaceButtonMode.Background:
                        if (GetComponent<Image>() != null)
                        {
                            GetComponent<Image>().color = this.HOVER_COLOR;
                        }
                        break;
                    case InterfaceButtonMode.BackgroundScale:
                        if (this.gameObject != null)
                        {
                            this.gameObject.transform.localScale +=
                                new Vector3(this.IncreaseButtonBy, this.IncreaseButtonBy, this.IncreaseButtonBy);
                        }
                        break;
                }
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
                switch (this.ButtonMode)
                {
                    case InterfaceButtonMode.TextScale:
                        this.TextComponent.fontSize = this.SmallFont;
                        break;
                    case InterfaceButtonMode.TextColor:
                        this.TextComponent.color = this.STARTING_COLOR;
                        break;
                    case InterfaceButtonMode.Background:
                        if (GetComponent<Image>() != null)
                        {
                            GetComponent<Image>().color = this.STARTING_COLOR;
                        }
                        break;
                    case InterfaceButtonMode.BackgroundScale:
                        if (this.gameObject != null)
                        {
                            this.gameObject.transform.localScale -=
                                new Vector3(this.IncreaseButtonBy, this.IncreaseButtonBy, this.IncreaseButtonBy);
                        }
                        break;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            OnInterfaceButton?.Invoke(false);
            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    this.TextComponent.fontSize = this.SmallFont;
                    break;
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponent<Image>() != null)
                    {
                        GetComponent<Image>().color = this.STARTING_COLOR;
                    }
                    break;
                case InterfaceButtonMode.BackgroundScale:
                    if (this.gameObject != null)
                    {
                        this.gameObject.transform.localScale -=
                            new Vector3(this.IncreaseButtonBy, this.IncreaseButtonBy, this.IncreaseButtonBy);
                    }
                    break;
            }
        }
    }
}
