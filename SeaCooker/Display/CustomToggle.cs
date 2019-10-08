using FCSCommon.Enums;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AE.SeaCooker.Display
{
    /// <summary>
    /// This class is a component for all interface buttons except the color picker and the paginator.
    /// </summary>
    internal class CustomToggle : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Public Properties

        public Color HOVER_COLOR { get; set; } = new Color(0.07f, 0.38f, 0.7f, 1f);
        public Color STARTING_COLOR { get; set; } = Color.white;
        public Text TextComponent { get; set; }

        public object Tag { get; set; }

        public Action<string, object> OnButtonClick;
        private GameObject _checkMark;

        public string BtnName { get; set; }

        public InterfaceButtonMode ButtonMode { get; set; }

        #endregion

        private void Awake()
        {
            FindToggle();
        }

        private void FindToggle()
        {
            var background = gameObject.FindChild("Background");

            if (background == null)
            {
                QuickLogger.Error("CustomToggle couldn't find Background");
                return;
            }

            _checkMark = background.FindChild("Checkmark")?.gameObject;

            if (_checkMark == null)
            {
                QuickLogger.Error("CustomToggle couldn't find Checkmark");
            }
        }

        #region Public Methods

        public void OnEnable()
        {
            if (string.IsNullOrEmpty(BtnName)) return;

            Disabled = false;

            UpdateTextComponent(IsTextMode());
            QuickLogger.Debug($"Button Name:{BtnName} || Button Mode {ButtonMode}", true);

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponentInChildren<Image>() != null)
                    {
                        GetComponentInChildren<Image>().color = this.STARTING_COLOR;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            QuickLogger.Debug($"Button Name:{BtnName} || Button Mode {ButtonMode}", true);

            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponentInChildren<Image>() != null)
                    {
                        GetComponentInChildren<Image>().color = this.STARTING_COLOR;
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
            if (this.IsHovered)
            {
                switch (this.ButtonMode)
                {
                    case InterfaceButtonMode.TextColor:
                        this.TextComponent.color = this.HOVER_COLOR;
                        break;
                    case InterfaceButtonMode.Background:
                        if (GetComponentInChildren<Image>() != null)
                        {
                            GetComponentInChildren<Image>().color = this.HOVER_COLOR;
                        }
                        break;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            UpdateTextComponent(IsTextMode());
            switch (this.ButtonMode)
            {
                case InterfaceButtonMode.TextColor:
                    this.TextComponent.color = this.STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponentInChildren<Image>() != null)
                    {
                        GetComponentInChildren<Image>().color = this.STARTING_COLOR;
                    }
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (!IsHovered) return;
            QuickLogger.Debug($"Clicked Button: {this.BtnName}", true);
            ToggleCheckMark();
            OnButtonClick?.Invoke(this.BtnName, this.Tag);
        }
        #endregion

        private void ToggleCheckMark()
        {
            _checkMark.SetActive(!_checkMark.activeSelf);
        }

        internal void SetToggleState(bool isChecked)
        {
            if (_checkMark == null)
            {
                FindToggle();
            }
            _checkMark?.SetActive(isChecked);
            QuickLogger.Debug($"SeaCooker Check Box Set To: {_checkMark.activeSelf}", true);
        }

        private IEnumerator AttemptToSetToggle(bool isChecked)
        {
            while (_checkMark == null)
            {
                QuickLogger.Debug($"Attempting to set toggle.", true);
                yield return null;
            }
            _checkMark.SetActive(isChecked);
            QuickLogger.Debug($"SeaCooker Check Box Set To: {_checkMark.activeSelf}", true);
        }

        internal void ChangeText(string message)
        {
            if (TextComponent == null)
            {
                QuickLogger.Info("Text Component returned null when trying to change the text in the InterfaceButton.Trying to locate");
                UpdateTextComponent();

                if (TextComponent == null)
                {
                    QuickLogger.Error("Was not able to find the Text Component in the gameObject please set the TextComponent Manually");
                    return;
                }
            }
            TextComponent.text = message;
        }

        internal bool CheckState()
        {
            return _checkMark.activeSelf;
        }
    }
}
