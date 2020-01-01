using UnityEngine;

namespace FlexibleTechFabricator
{
    public interface IFcCraftingTab
    {
        string Id { get; }
        Atlas.Sprite Icon { get; }
        string DisplayName { get; }
        void LoadAssets(IFcAssetBundlesService assetBundlesService);
    }

    public abstract class FcCraftingTab : IFcCraftingTab
    {
        public string Id { get; }

        public Atlas.Sprite Icon { get; private set; }

        public string DisplayName { get; }

        public abstract string AssetBundleName { get; }

        public AssetBundle AssetBundle { get; private set; }

        protected FcCraftingTab(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }

        public void LoadAssets(IFcAssetBundlesService assetBundlesService)
        {
            if (this.AssetBundle == null)
            {
                this.AssetBundle = assetBundlesService.GetAssetBundleByName(this.AssetBundleName);
            }
        }
    }
}
