using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Buildable
{
    internal partial class FCSDeepDrillerBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override string AssetsFolder => Mod.GetAssetFolder();

        public FCSDeepDrillerBuildable() : base(Mod.DeepDrillerMk3ClassName, Mod.DeepDrillerMk3FriendlyName, Mod.DeepDrillerMk3Description)
        {
            OnFinishedPatching += () =>
            {
                var deepDrillerMk2Kit = new FCSKit(Mod.DeepDrillerMk3KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                deepDrillerMk2Kit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, deepDrillerMk2Kit.TechType, 55000, StoreCategory.Production);
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate<GameObject>(ModelPrefab.DeepDrillerPrefab);

                //========== Allows the building animation and material colors ==========// 
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                constructable.forceUpright = true;
                constructable.placeDefaultDistance = 10f;
                constructable.placeMaxDistance = 10f;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                var center = new Vector3(-2.384186e-07f, 2.500637f, -0.007555246f);
                var size = new Vector3(5.605133f, 8.229565f, 5.689059f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<TechTag>().type = TechTypeID;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<FCSDeepDrillerController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.DeepDrillerMk3KitClassID.ToTechType(), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.DeepDrillerKitClassID.ToTechType(), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
#endif
    }
}
