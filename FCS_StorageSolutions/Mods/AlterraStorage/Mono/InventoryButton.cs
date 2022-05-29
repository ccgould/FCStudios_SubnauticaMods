using FCS_AlterraHub.Mono;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.AlterraStorage.Mono
{
    internal class InventoryButton : InterfaceButton
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
                _amount = gameObject.GetComponentInChildren<Text>();
            }
        }

        internal void Set(TechType techType, int amount)
        {
            Initialize();
            Tag = techType;
            _amount.text = amount.ToString();
            _icon.sprite = SpriteManager.Get(techType);
            TextLineOne = Language.main.Get(techType);
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