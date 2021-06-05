using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = System.Object;

namespace FCS_AlterraHub.Model.GUI
{
    public class FCSGuiToggle : OnScreenButton, IPointerEnterHandler,IPointerExitHandler
    {
        private uGUI_Icon _icon;
        private Text _amount;
        private Toggle _toggle;
        private FCSToolTip _toolTip;
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

            if (_toolTip == null)
            {
                _toolTip = gameObject.AddComponent<FCSToolTip>();
                _toolTip.Description = true;
                _toolTip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject,Player.main.gameObject,2f);
                _toolTip.ToolTipStringDelegate += () => ToolTipString;
            }
        }

        public string ToolTipString { get; set; }

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