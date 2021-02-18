using FCS_AlterraHub.Mono;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DeviceItem : InterfaceButton
    {
        private uGUI_Icon _icon;
        private Text _amount;

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_amount == null)
            {
                _amount = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(FcsDevice device)
        {
            Initialize();
            Tag = device;
            _amount.text = device.UnitID;
            _icon.sprite = SpriteManager.Get(device.GetTechType());
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _amount.text = "";
            _icon.sprite = SpriteManager.Get(TechType.None);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }
}