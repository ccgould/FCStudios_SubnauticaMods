using System;
using FCS_DeepDriller.Configuration;
using FCSTechFabricator;

namespace FCS_DeepDriller.Craftable
{
    using SMLHelper.V2.Assets;
    using System.Collections.Generic;
    using FCSCommon.Extensions;
    using FCSCommon.Utilities;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    internal class FCSGlassCraftable : FcCraftable
    {
        private TechData _ingredients;

        public FCSGlassCraftable(FcCraftingTab parentTab) :base("FCSGlass", "Glass", "SiO4. Pure fused quartz glass.", parentTab)
        {
            _ingredients = new TechData
            {
                LinkedItems = new List<TechType> {TechType.Glass},
                craftAmount = 0,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.SandSpawnableClassID.ToTechType(), 1)
                }
            };
        }


        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Glass));
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return _ingredients;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.Glass);
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return _ingredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png"));
        }
#endif

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.BundleName;
        public override string IconFileName => "";
        public override string AssetsFolder => "";
    }
}
