using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSInventoryItem : InterfaceButton
    {
        private uGUI_Icon _icon;
        private Text _amount;
        private FCSToolTip _tooltip;

        private void Initialize()
        {
            if (_tooltip == null)
            {
                _tooltip = gameObject.AddComponent<FCSToolTip>();
                _tooltip.Description = true;
            }

            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_amount == null)
            {
                _amount = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(TechType techType, int amount)
        {
            Initialize();
            Tag = techType;
            _tooltip.TechType = techType;
            _tooltip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 1f);
            _amount.text = amount.ToString();
            _icon.sprite = SpriteManager.Get(techType);
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