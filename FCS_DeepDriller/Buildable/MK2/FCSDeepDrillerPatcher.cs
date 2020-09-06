using System;
using System.Collections.Generic;
using System.IO;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_DeepDriller.Buildable.MK2
{
    internal partial class FCSDeepDrillerBuildable : SMLHelper.V2.Assets.Buildable
    {
        private static readonly FCSDeepDrillerBuildable Singleton = new FCSDeepDrillerBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override string IconFileName => "FCSDeepDriller.png";
        public override TechType RequiredForUnlock { get; } = TechType.ExosuitDrillArmModule;

        public override string AssetsFolder { get; } = $"FCS_DeepDriller/Assets";

        public FCSDeepDrillerBuildable() : base(Mod.ModClassID, Mod.ModFriendlyName, Mod.ModDecription)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        internal static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate<GameObject>(_prefab);

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

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                //var beacon = prefab.AddComponent<Beacon>();


                var center = new Vector3(0, 3.106274f, 0);
                var size = new Vector3(6.85554f, 6.670462f, 7.002856f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                //beacon.label = "DeepDriller";
                //prefab.AddComponent<LiveMixin>();
                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<TechTag>().type = TechTypeID;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<FCSDeepDrillerController>();

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
                    new Ingredient(Mod.DeepDrillerKitClassID.ToTechType(), 1)
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
