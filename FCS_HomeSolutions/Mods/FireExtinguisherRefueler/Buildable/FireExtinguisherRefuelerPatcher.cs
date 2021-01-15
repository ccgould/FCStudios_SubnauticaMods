using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable
{
    internal partial class FireExtinguisherRefuelerBuildable : SMLHelper.V2.Assets.Buildable
    {
        public FireExtinguisherRefuelerBuildable() : base(Mod.FireExtinguisherRefuelerClassID, Mod.FireExtinguisherRefuelerFriendly, Mod.FireExtinguisherRefuelerDescription)
        {

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(Mod.FireExtinguisherRefuelerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.FireExtinguisherRefuelerKitClassID.ToTechType(), 60000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.FireExtinguisherRefuelerPrefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0.002549291f, 0.04286739f, 0.2148812f);
                var size = new Vector3(0.9426436f, 0.5879701f, 0.4037365f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<FireExtinguisherRefuelerController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                //MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

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
                    new Ingredient(Mod.FireExtinguisherRefuelerKitClassID.ToTechType(), 1)
                }
            };
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
                    new Ingredient(Mod.FireExtinguisherRefuelerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }
#endif

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
