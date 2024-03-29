﻿using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UWE;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.Craftable
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

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            return CraftData.InstantiateFromPrefab(TechType.Glass);
        }
#endif
        
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(TechType.Glass, false, task);
            gameObject.Set(GameObject.Instantiate(task.Get()));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                LinkedItems = new List<TechType> {TechType.Glass},
                craftAmount = 0,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.SandSpawnableClassID.ToTechType(), 1)
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.Glass);
        }
    }
}
