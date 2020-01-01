namespace FlexibleTechFabricator
{
    using SMLHelper.V2.Assets;
    using UnityEngine;

    public abstract class FcCraftable : Craftable
    {
        protected IFcTechFabricatorService FabricatorService { get; private set; }
        protected IFcAssetBundlesService AssetBundlesService { get; private set; }

        protected abstract string AssetBundleName { get; }
        protected AssetBundle AssetBundle { get; private set; }

        public override CraftTree.Type FabricatorType => CraftTree.Type.None;
        public IFcCraftingTab ParentTab { get; }

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

        public void Patch(IFcTechFabricatorService fabricatorService, IFcAssetBundlesService assetBundlesService)
        {
            this.FabricatorService = fabricatorService;
            this.AssetBundlesService = assetBundlesService;

            base.Patch();
        }
    }
}
