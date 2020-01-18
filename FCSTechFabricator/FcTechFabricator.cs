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
        
        protected override Atlas.Sprite GetItemSprite()
        {
           return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(), $"{ClassID}.png")));
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = base.GetGameObject();

            // TODO
            Texture2D coloredTexture = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(AssetBundleName).LoadAsset<Texture2D>(Mod.ModName);
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
