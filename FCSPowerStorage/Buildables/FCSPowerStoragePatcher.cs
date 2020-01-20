using FCSCommon.Utilities;
using FCSPowerStorage.Helpers;
using FCSPowerStorage.Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.IO;
using System.Linq;
using FCSCommon.Helpers;
using UnityEngine;
using Information = FCSPowerStorage.Configuration.Information;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable : Buildable
    {
        public override string AssetsFolder { get; } = $"{Information.ModFolderName}/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        private static readonly FCSPowerStorageBuildable Singleton = new FCSPowerStorageBuildable();
        public FCSPowerStorageBuildable() : base(Information.ModName, Information.ModFriendlyName, Information.ModDescription)
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
            SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 


            QuickLogger.Debug("Adding Constructible");

            // Add constructible
            var constructable = prefab.EnsureComponent<Constructable>();
            constructable.allowedOnWall = true;
            constructable.allowedOnGround = false;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOutside = false;
            constructable.model = prefab.FindChild("model");
            constructable.techType = TechType;

            // Add constructible bounds

            var center = new Vector3(0.2078698f, -0.04198265f, 0.2626062f);
            var size = new Vector3(1.412603f, 1.45706f, 0.4747875f);

            GameObjectHelpers.AddConstructableBounds(prefab,size,center);

            QuickLogger.Debug("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.EnsureComponent<TechTag>().type = this.TechType;

            QuickLogger.Debug("GetOrAdd PrefabIdentifier");

            prefab.EnsureComponent<PrefabIdentifier>().ClassId = this.ClassID;

            QuickLogger.Debug("Add GameObject CustomBatteryController");

            prefab.EnsureComponent<FCSPowerStorageDisplay>();

            prefab.EnsureComponent<FCSPowerStorageController>();

            QuickLogger.Debug("Made GameObject");

            return prefab;
        }

#if SUBNAUTICA
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
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug("Creating FCS Battery Storage recipe...");
            // Create and associate recipe to the new TechType

            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = LoadData.BatteryConfiguration.ConvertToIngredients().ToList()
            };
            QuickLogger.Debug("Created Ingredients for FCS Power Storage");
            return customFabRecipe;
        }
#endif
    }
}
