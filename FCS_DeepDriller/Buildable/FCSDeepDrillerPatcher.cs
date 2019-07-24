using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_DeepDriller.Buildable
{
    using SMLHelper.V2.Assets;
    using System;

    internal partial class FCSDeepDrillerBuildable : Buildable
    {
        private static readonly FCSDeepDrillerBuildable Singleton = new FCSDeepDrillerBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"FCS_DeepDriller/Assets";
        public static DeepDrillerCfg DeepDrillConfig { get; internal set; }

        public FCSDeepDrillerBuildable() : base(Mod.ModName, Mod.ModFriendlyName, Mod.ModDecription)
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


            string savedDataJson = File.ReadAllText(Path.Combine(AssetHelper.GetConfigFolder(Mod.ModName), $"{Singleton.ClassID}.json")).Trim();

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            DeepDrillConfig = JsonConvert.DeserializeObject<DeepDrillerCfg>(savedDataJson, jsonSerializerSettings);
            DeepDrillConfig.Convert();

            QuickLogger.Debug($"Biome Ores Count {DeepDrillConfig.BiomeOresTechType.Count}");

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
                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
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
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

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

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    //TODO Change to KIT
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.Diamond, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
    }
}
