using System.IO;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator
{
    internal class FcTechFabricator : CustomFabricator
    {
        private const string AssetBundleName = "fcstechfabricatormodbundle";

        public FcTechFabricator()
            : base(Mod.ModClassID, Mod.ModFriendly, Mod.ModDescription)
        {

        }

        public override Models Model => Models.Fabricator;
        

        public override GameObject GetGameObject()
        {
            GameObject prefab = base.GetGameObject();

            // Custom skin
            Texture2D coloredTexture = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(AssetBundleName).LoadAsset<Texture2D>(Mod.ModName);
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;
            prefab.AddComponent<FCSTechFabGameLoadUtil>();

            return prefab;
        }

#if SUBNAUTICA
        protected override Atlas.Sprite GetItemSprite()
        {
           return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(), $"{ClassID}.png")));
        }
        
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.Gold, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Quartz, 2)
                }
            };
        }
#elif BELOWZERO
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), $"{ClassID}.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.Gold, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Quartz, 2)
                }
            };
        }
#endif
    }
}
