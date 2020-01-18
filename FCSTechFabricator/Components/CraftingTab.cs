namespace FCSTechFabricator.Components
{
    public class CraftingTab : FcCraftingTab
    {
        public override string AssetBundleName => FcAssetBundlesService.PublicAPI.GlobalBundleName;

        

        public CraftingTab(string id, string displayName,Atlas.Sprite icon) : base(id, displayName,icon)
        {

        }
    }
}
