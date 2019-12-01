using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Utility;
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

            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.DeepDrillerKitClassID,
                null,
                Mod.ModFriendlyName,
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" },
                null);

            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.SolarAttachmentKitClassID,
                "This specially made attachment allows you to run your deep driller off solar power.",
                "Deep Driller Solar Attachment",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" },
                "SolarAttachment_DD.png");
            
            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.FocusAttachmentKitClassID,
                "This specially made attachment allows you to scan for one specific ore.",
                "Deep Driller Focus Attachment",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" },
                "FocusAttachment_DD.png");
            
            PatchHelpers.AddNewKit(
                FCSTechFabricator.Configuration.BatteryAttachmentKitClassID,
                "This specially made attachment allows you to run your deep driller off powercell power",
                "Deep Driller Powercell Attachment",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" },
                "BatteryAttachment_DD.png");

            PatchHelpers.AddNewModule(
                FCSTechFabricator.Configuration.DrillerMK1ModuleClassID,
                "This upgrade allows deep driller to drill 15 resources per day.",
                "Deep Driller MK 1",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" });
            
            PatchHelpers.AddNewModule(
                FCSTechFabricator.Configuration.DrillerMK2ModuleClassID,
                "This upgrade allows deep driller to drill 22 resources per day.",
                "Deep Driller MK 2",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" });

            PatchHelpers.AddNewModule(
                FCSTechFabricator.Configuration.DrillerMK3ModuleClassID,
                "This upgrade allows deep driller to drill 30 resources per day.",
                "Deep Driller MK 3",
                FCSTechFabricator.Configuration.DeepDrillerClassID,
                new[] { "AIS", "DD" });

            //TODO Add Updates

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

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType("DeepDrillerKit_DD"), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
    }
}
