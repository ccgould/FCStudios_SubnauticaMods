using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable
{
    internal class HydroponicHarvesterPatch : SMLHelper.V2.Assets.Buildable
    {
        private TechType kitTechType;

        internal const string HydroponicHarvesterModTabID = "HH";
        internal const string HydroponicHarvesterModFriendlyName = "Hydroponic Harvester";
        internal const string HydroponicHarvesterModName = "HydroponicHarvester";
        public const string HydroponicHarvesterModDescription = "3 chambers for growing DNA Samples scanned by the Matter Analyzer. Suitable for interior and exterior use.";
        internal static string HydroponicHarvesterKitClassID => $"{HydroponicHarvesterModName}_Kit";
        internal static string HydroponicHarvesterModClassName => HydroponicHarvesterModName;
        internal static string HydroponicHarvesterModPrefabName => HydroponicHarvesterModName;

        public HydroponicHarvesterPatch() : base(HydroponicHarvesterModClassName, HydroponicHarvesterModFriendlyName, HydroponicHarvesterModDescription)
        {
            OnStartedPatching += () =>
            {
                var hydroponicHarvesterKit = new FCSKit(HydroponicHarvesterKitClassID, HydroponicHarvesterModFriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                hydroponicHarvesterKit.Patch();
                kitTechType = hydroponicHarvesterKit.TechType;
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, kitTechType, 196875, StoreCategory.Production);
            };
        }
        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        public override string AssetsFolder => Mod.GetAssetPath();

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.HydroponicHarvesterPrefab);

                var size = new Vector3(1.353966f, 2.503282f, 1.006555f);
                var center = new Vector3(0.006554961f, 1.394679f, 0.003277525f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = true;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.SetActive(false);
                var storageContainer = prefab.AddComponent<GrowBedManager>();
                storageContainer.Initialize(150, 1, 1, FriendlyName, ClassID);
                storageContainer.enabled = false;
                prefab.SetActive(true);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<HydroponicHarvesterController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                return prefab;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(HydroponicHarvesterKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}