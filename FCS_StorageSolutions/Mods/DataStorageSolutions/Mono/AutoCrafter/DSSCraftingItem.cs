using FCS_AlterraHub.Model;
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
        private FCSToolTip _toolTip;


        private void Initialize()
        {
            UseSetUseTextRaw = true;
            if (_isInitialized) return;

            var icon = gameObject.FindChild("Icon");
            _toolTip = icon.AddComponent<FCSToolTip>();
            _toolTip.RequestPermission = () => true;
            _icon = icon.EnsureComponent<uGUI_Icon>();
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
            _toolTip.TechType = techType;
            var itemName = Language.main.Get(techType);
            _icon.sprite = SpriteManager.Get(techType);
            TextLineOne = AuxPatchers.CraftFormatted(itemName);
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