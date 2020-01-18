using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechFabricator
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

        protected FcCraftingTab(string id, string displayName, Atlas.Sprite icon)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Icon = icon;
        }

        public void LoadAssets(IFcAssetBundlesService assetBundlesService)
        {
            QuickLogger.Debug($"Assetbundle == {AssetBundle}");
            if (this.AssetBundle == null)
            {
                this.AssetBundle = assetBundlesService.GetAssetBundleByName(this.AssetBundleName);
                QuickLogger.Debug($"Assetbundle was null now == {AssetBundle}");
            }
        }
    }
}
