namespace FCSTechFabricator.Components
{
    public class CraftingTab : FcCraftingTab
    {
        public override string AssetBundleName => FcAssetBundlesService.PublicAPI.GlobalBundleName;


#if SUBNAUTICA
        public CraftingTab(string id, string displayName, Atlas.Sprite icon) : base(id, displayName,icon)
        {

        }
#elif BELOWZERO
        public CraftingTab(string id, string displayName, UnityEngine.Sprite icon) : base(id, displayName, icon)
        {

        }
#endif
    }
}
