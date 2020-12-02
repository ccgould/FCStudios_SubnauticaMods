using FCSCommon.Enums;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSCommon.Components
{
    /// <summary>
    /// This class is a component for all interface buttons except the color picker and the paginator.
    /// </summary>
    internal class InterfaceButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Public Properties

        /// <summary>
        /// The pages to change to.
        /// </summary>
        public GameObject ChangePage { get; set; }
        public string BtnName { get; set; }
        public Color HOVER_COLOR { get; set; } = new Color(0.07f, 0.38f, 0.7f, 1f);
        public Color STARTING_COLOR { get; set; } = Color.white;
        public InterfaceButtonMode ButtonMode { get; set; } = InterfaceButtonMode.Background;
        public Text TextComponent { get; set; }
        public int SmallFont { get; set; } = 140;
        public int LargeFont { get; set; } = 180;
        public float IncreaseButtonBy { get; set; }
        public Action<bool> OnInterfaceButton { get; set; }

        public Action<string, object> OnButtonClick;

        internal bool IsSelected { get; set; }
        private Image _bgImage;

        

        #endregion

        #region Public Methods
        public virtual void OnEnable()
        {
            if(IsSelected) return;
            if (string.IsNullOrEmpty(BtnName)) return;

            Disabled = false;

            UpdateTextComponent(IsTextMode());
            //QuickLogger.Debug($"Button Name:{BtnName} || Button Mode {ButtonMode}", true);

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    this.TextComponent.fontSize = this.TextComponent.fontSize;
                    break;
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    FindImage();
                    if (_bgImage != null)
                    {
                        _bgImage.color = this.STARTING_COLOR;
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

        private void FindImage()
        {
            if(_bgImage == null)
            {
                _bgImage = GetComponentInChildren<Image>();
            }
        }

        #endregion

        private void UpdateTextComponent(bool force = true)
        {
            if (TextComponent == null && force)
            {
                TextComponent = gameObject.GetComponentInChildren<Text>();
                if (TextComponent == null)
                {
                    QuickLogger.Error("Was not able to find the Text Component in the gameObject please set the TextComponent Manually");
                }
            }
        }

        #region Public Overrides

        public override void OnDisable()
        {
            base.OnDisable();

            if (string.IsNullOrEmpty(BtnName)) return;
            UpdateTextComponent(IsTextMode());
            //QuickLogger.Debug($"Button Name:{BtnName} || Button Mode {ButtonMode}", true);
            
            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    this.TextComponent.fontSize = this.TextComponent.fontSize;
                    break;
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    FindImage();
                    if (_bgImage != null)
                    {
                        _bgImage.color = this.STARTING_COLOR;
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

        private bool IsTextMode()
        {
            return ButtonMode == InterfaceButtonMode.TextColor || ButtonMode == InterfaceButtonMode.TextScale;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            UpdateTextComponent(IsTextMode());
            OnInterfaceButton?.Invoke(true);
            if (this.IsHovered)
            {
                switch (this.ButtonMode)
                {
                    case InterfaceButtonMode.TextScale:
                        if (TextComponent == null || IsSelected) return;
                        this.TextComponent.fontSize = this.LargeFont;
                        break;
                    case InterfaceButtonMode.TextColor:
                        if (TextComponent == null || IsSelected) return;
                        this.TextComponent.color = this.HOVER_COLOR;
                        break;
                    case InterfaceButtonMode.Background:
                        FindImage();
                        if (_bgImage != null || !IsSelected)
                        {
                            _bgImage.color = this.HOVER_COLOR;
                        }
                        break;
                    case InterfaceButtonMode.BackgroundScale:
                        if (this.gameObject != null || !IsSelected)
                        {
                            this.gameObject.transform.localScale +=
                                new Vector3(this.IncreaseButtonBy, this.IncreaseButtonBy, this.IncreaseButtonBy);
                        }
                        break;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (IsSelected) return;
            UpdateTextComponent(IsTextMode());
            OnInterfaceButton?.Invoke(false);

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    if (TextComponent == null) return;
                    this.TextComponent.fontSize = this.SmallFont;
                    break;
                case InterfaceButtonMode.TextColor:
                    if (TextComponent == null) return;
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    FindImage();
                    if (_bgImage != null)
                    {
                        _bgImage.color = this.STARTING_COLOR;
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

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                QuickLogger.Debug("OnPointerClick Interface Button: IsPointerOverGameObject: False",true);
                return;
            }

            if (this.IsHovered)
            {
                QuickLogger.Debug($"Clicked Button: {this.BtnName}", true);
                OnButtonClick?.Invoke(this.BtnName, this.Tag);
            }
            else
            {
                QuickLogger.Debug("Is Hovered Returned false",true);
            }
        }
        #endregion

        public void ChangeText(string message)
        {
            if (TextComponent == null)
            {
                QuickLogger.Debug("Text Component returned null when trying to change the text in the InterfaceButton.Trying to locate");
                UpdateTextComponent();

                if (TextComponent == null)
                {
                    QuickLogger.Error("Was not able to find the Text Component in the gameObject please set the TextComponent Manually");
                    return;
                }

            }
            TextComponent.text = message;
        }
        public void Select()
        {
            IsSelected = true;

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.Background:
                    FindImage();
                    if (_bgImage != null)
                    {
                        _bgImage.color = this.HOVER_COLOR;
                    }
                    break;

                case InterfaceButtonMode.TextColor:
                    if (TextComponent == null) return;
                    this.TextComponent.color = this.HOVER_COLOR;
                    break;
            }
        }
        public void DeSelect()
        {
            IsSelected = false;

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.Background:
                    FindImage();
                    if (_bgImage != null)
                    {
                        _bgImage.color = STARTING_COLOR;
                    }
                    break;

                case InterfaceButtonMode.TextColor:
                    if (TextComponent == null) return;
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
            }
        }
    }
}
