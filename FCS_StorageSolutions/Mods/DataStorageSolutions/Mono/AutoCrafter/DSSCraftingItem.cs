using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSCraftingItem : InterfaceButton
    {
        private uGUI_Icon _icon;
        private bool _isInitialized;
        private DSSAutoCrafterDisplay _controller;
        private TechType _techType;

        private void Initialize()
        {

            if (_isInitialized) return;
            
            _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            _isInitialized = true;
            OnButtonClick += (s, o) =>
            {
                _controller.AddNewCraftingItem(new CraftingItem(_techType));
            };
        }

        internal void Set(TechType techType, DSSAutoCrafterDisplay controller)
        {
            Initialize();
            _controller = controller;
            _techType = techType;
            _icon.sprite = SpriteManager.Get(techType);
            TextLineOne = AuxPatchers.CraftFormatted(Language.main.Get(techType));
            Show();
        }

        internal void Reset()
        {
            Initialize();
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