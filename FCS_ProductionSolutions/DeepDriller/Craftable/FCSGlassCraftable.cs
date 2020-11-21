using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Craftable
{
    internal class FcsGlassCraftable : SMLHelper.V2.Assets.Craftable
    {

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        public override string AssetsFolder => Mod.GetAssetFolder();
        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;
        public override string[] StepsToFabricatorTab => new[] {"Resources","BasicMaterials"};
        public FcsGlassCraftable() : base("FCSGlass", "Glass", "SiO4. Pure fused quartz glass.")
        {

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
            return new TechData
            {
                LinkedItems = new List<TechType> { TechType.Glass },
                craftAmount = 0,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.SandSpawnableClassID.ToTechType(), 1)
                }
            }; ;
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
    }
}
