using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using FCSPowerStorage.Model;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable : Buildable
    {
        public override string AssetsFolder { get; } = @"FCSPowerStorage/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        private static readonly FCSPowerStorageBuildable Singleton = new FCSPowerStorageBuildable();
        public FCSPowerStorageBuildable() : base(Information.ModName, "FCS Power Storage", Information.ModDescription)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(_prefab);

            //========== Allows the building animation and material colors ==========// 


            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 


            QuickLogger.Debug("Adding Constructible");

            // Add constructible
            var constructable = prefab.GetOrAddComponent<Constructable>();
            constructable.allowedOnWall = true;
            constructable.allowedOnGround = false;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOutside = false;
            constructable.model = prefab.FindChild("model");
            constructable.techType = TechType;

            // Add constructible bounds
            prefab.AddComponent<ConstructableBounds>().bounds =
                new OrientedBounds(
                new Vector3(-0.1f, -0.1f, 0f),
                new Quaternion(0, 0, 0, 0),
                new Vector3(0.9f, 0.5f, 0f));

            QuickLogger.Debug("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.GetOrAddComponent<TechTag>().type = this.TechType;

            QuickLogger.Debug("GetOrAdd PrefabIdentifier");

            prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

            QuickLogger.Debug("Add GameObject CustomBatteryController");

            prefab.GetOrAddComponent<CustomBatteryController>();

            QuickLogger.Debug("Made GameObject");

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug("Creating FCS Battery Storage recipe...");
            // Create and associate recipe to the new TechType

            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = LoadData.BatteryConfiguration.ConvertToIngredients().ToList()
            };
            QuickLogger.Debug("Created Ingredients for FCS Power Storage");
            return customFabRecipe;
        }
    }
}
