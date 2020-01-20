using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator
{
    public abstract class FcCraftable : Craftable
    {
        protected IFcTechFabricatorService FabricatorService { get; private set; }
        protected IFcAssetBundlesService AssetBundlesService { get; private set; }

        protected abstract string AssetBundleName { get; }
        protected AssetBundle AssetBundle { get; private set; }

        public override CraftTree.Type FabricatorType => CraftTree.Type.None;
        public IFcCraftingTab ParentTab { get; }

#if SUBNAUTICA
        protected FcCraftable(string classId, string friendlyName, string description, FcCraftingTab parentTab)
            : base(classId, friendlyName, description)
        {
            this.ParentTab = parentTab;

            OnStartedPatching += () =>
            {
                this.ParentTab.LoadAssets(this.AssetBundlesService);
                this.AssetBundle = this.AssetBundlesService.GetAssetBundleByName(this.AssetBundleName);
            };

            OnFinishedPatching += () =>
            {
                if (!this.FabricatorService.HasCraftingTab(this.ParentTab.Id))
                {
                    string tabId = this.ParentTab.Id;
                    string displayText = this.ParentTab.DisplayName;
                    Atlas.Sprite icon = this.ParentTab.Icon;

                    this.FabricatorService.AddTabNode(tabId, displayText, icon);
                }

                this.FabricatorService.AddCraftNode(this, this.ParentTab.Id);
            };
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            Texture2D iconTexture = this.AssetBundle.LoadAsset<Texture2D>(this.IconFileName);
            return ImageUtils.LoadSpriteFromTexture(iconTexture);
            
        }

#elif BELOWZERO
        protected FcCraftable(string classId, string friendlyName, string description, FcCraftingTab parentTab)
            : base(classId, friendlyName, description)
        {
            this.ParentTab = parentTab;

            OnStartedPatching += () =>
            {
                this.ParentTab.LoadAssets(this.AssetBundlesService);
                this.AssetBundle = this.AssetBundlesService.GetAssetBundleByName(this.AssetBundleName);
            };

            OnFinishedPatching += () =>
            {
                if (!this.FabricatorService.HasCraftingTab(this.ParentTab.Id))
                {
                    string tabId = this.ParentTab.Id;
                    string displayText = this.ParentTab.DisplayName;
                    Sprite icon = this.ParentTab.Icon;

                    this.FabricatorService.AddTabNode(tabId, displayText, icon);
                }

                this.FabricatorService.AddCraftNode(this, this.ParentTab.Id);
            };
        }

        protected override Sprite GetItemSprite()
        {
            Texture2D iconTexture = this.AssetBundle.LoadAsset<Texture2D>(this.IconFileName);
            return ImageUtils.LoadSpriteFromTexture(iconTexture);

        }

#endif

        public void Patch(IFcTechFabricatorService fabricatorService, IFcAssetBundlesService assetBundlesService)
        {
            this.FabricatorService = fabricatorService;
            this.AssetBundlesService = assetBundlesService;

            base.Patch();
        }
    }
}
