using System;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = System.Object;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    public class FCSGuiToggle : OnScreenButton, IPointerEnterHandler,IPointerExitHandler
    {
        private uGUI_Icon _icon;
        private Text _amount;
        private Toggle _toggle;
        public string BTNName { get; set; }
        public Action<string,object> OnButtonClick { get; set; }
        public Object Tag { get; set; }

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_toggle == null)
            {
                _toggle = gameObject.GetComponentInChildren<Toggle>();
                _toggle.onValueChanged.AddListener((value =>
                {
                    OnButtonClick?.Invoke(BTNName, Tag);
                }));

            }
        }

        public void Set(TechType techType)
        {
            Initialize();
            _icon.sprite = SpriteManager.Get(techType);
            Show();
        }

        public void Reset()
        {
            Initialize();
            _icon.sprite = SpriteManager.Get(TechType.None);
            _toggle.isOn = false;
            Tag = null;
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Select()
        {
            _toggle.isOn = true;
        }

        public bool IsChecked => _toggle?.isOn ?? false;
    }
}