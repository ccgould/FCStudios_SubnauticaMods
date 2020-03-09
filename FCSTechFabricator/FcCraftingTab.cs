using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechFabricator
{
    public interface IFcCraftingTab
    {
        string Id { get; }
#if SUBNAUTICA
        Atlas.Sprite Icon { get; }
#elif BELOWZERO
        Sprite Icon { get; }
#endif
        string DisplayName { get; }
        void LoadAssets(IFcAssetBundlesService assetBundlesService);
    }

    public abstract class FcCraftingTab : IFcCraftingTab
    {
        public string Id { get; }

#if SUBNAUTICA
        public Atlas.Sprite Icon { get; private set; }
#elif BELOWZERO
        public Sprite Icon { get; private set; }
#endif

        public string DisplayName { get; }

        public abstract string AssetBundleName { get; }

        public AssetBundle AssetBundle { get; private set; }

#if SUBNAUTICA
        protected FcCraftingTab(string id, string displayName, Atlas.Sprite icon)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Icon = icon;
        }
#elif BELOWZERO
        protected FcCraftingTab(string id, string displayName, Sprite icon)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Icon = icon;
        }
#endif

        public void LoadAssets(IFcAssetBundlesService assetBundlesService)
        {
            if (this.AssetBundle == null)
            {
                this.AssetBundle = assetBundlesService.GetAssetBundleByName(this.AssetBundleName);
                QuickLogger.Debug($"Assetbundle was null now == {AssetBundle}");
            }
        }
    }
}
