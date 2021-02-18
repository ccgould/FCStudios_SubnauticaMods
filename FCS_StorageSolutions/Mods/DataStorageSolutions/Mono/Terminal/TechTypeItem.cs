using FCS_AlterraHub.Mono;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class TechTypeItem : FCSToggleButton
    {
        private uGUI_Icon _icon;
        private Text _amount;

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }
        }

        internal void Set(TechType techType)
        {
            Initialize();
            Tag = techType;
            _icon.sprite = SpriteManager.Get(techType);
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _icon.sprite = SpriteManager.Get(TechType.None);
            Tag = null;
            Hide();
            DeSelect();
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