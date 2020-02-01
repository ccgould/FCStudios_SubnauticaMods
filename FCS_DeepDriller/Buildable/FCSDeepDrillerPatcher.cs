using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Extensions;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_DeepDriller.Buildable
{
    using SMLHelper.V2.Assets;
    using System;

    internal partial class FCSDeepDrillerBuildable : Buildable
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
                prefab = GameObject.Instantiate(_prefab);

                var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
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

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                //var beacon = prefab.AddComponent<Beacon>();


                var center = new Vector3(0, 2.433337f, 0);
                var size = new Vector3(4.821606f, 4.582462f, 4.941598f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                //beacon.label = "DeepDriller";
                //prefab.AddComponent<LiveMixin>();
                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
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
