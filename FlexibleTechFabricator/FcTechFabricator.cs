namespace FlexibleTechFabricator
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Handlers;
    using UnityEngine;

    internal class FcTechFabricator : CustomFabricator
    {
        private const string AssetBundleName = "fcstechfabricatormodbundle";

        public FcTechFabricator()
            : base("FcTechFabricator", "friendly name goes here", "Description goes here")
        {
        }

        public override Models Model => Models.Fabricator;


        protected override Atlas.Sprite GetItemSprite()
        {
            // TODO
            Texture2D iconTexture = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(AssetBundleName).LoadAsset<Texture2D>("FCSTechFabricatorIcon");
            return new Atlas.Sprite(iconTexture);
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = base.GetGameObject();

            // TODO
            Texture2D coloredTexture = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(AssetBundleName).LoadAsset<Texture2D>("FCSTechFabricator");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            };
        }
    }
}
