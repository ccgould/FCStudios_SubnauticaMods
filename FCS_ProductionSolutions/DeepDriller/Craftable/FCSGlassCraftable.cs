using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.DeepDriller.Craftable
{
    internal class FcsGlassCraftable : SMLHelper.V2.Assets.Craftable
    {

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        public override string AssetsFolder => Mod.GetAssetFolder();
        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;
        public override string[] StepsToFabricatorTab => new[] {"Resources","BasicMaterials"};
        public FcsGlassCraftable() : base("FCSGlass", "Sand Infused Glass", "SiO2. Pure fused sand glass.")
        {

        }

        public override GameObject GetGameObject()
        {
            try
            {
                if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.Glass), out string filepath))
                {
                    GameObject prefab = Resources.Load<GameObject>(filepath);

                    if (prefab != null)
                    {
                        var Obj = GameObject.Instantiate(prefab);
                        return Obj;
                    }
                }
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
