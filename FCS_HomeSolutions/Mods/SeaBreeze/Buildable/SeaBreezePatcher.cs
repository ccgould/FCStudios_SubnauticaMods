using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.ModManagers;
using FCS_HomeSolutions.Mods.SeaBreeze.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.SeaBreeze.Buildable
{
    internal partial class SeaBreezeBuildable : SMLHelper.V2.Assets.Buildable
    {

        internal const string SeaBreezeClassID = "Seabreeze";
        internal const string SeaBreezeFriendly = "Seabreeze";
        internal const string SeaBreezeDescription = "Refrigeration unit for perishable food and cool, refreshing water.";
        internal const string SeaBreezePrefabName = "SeaBreezeFCS32";
        internal static string SeaBreezeKitClassID = $"{SeaBreezeClassID}_Kit";
        internal const string SeaBreezeTabID = "SB";
        public SeaBreezeBuildable() : base(SeaBreezeClassID, SeaBreezeFriendly, SeaBreezeDescription)
        {
            OnStartedPatching += () =>
            {

                var seaBreezeKit = new FCSKit(SeaBreezeKitClassID, FriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                seaBreezeKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, SeaBreezeKitClassID.ToTechType(), 131250, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
                SeaBreezeAuxPatcher.AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(FCSAssetBundlesService.PublicAPI.GetPrefabByName(SeaBreezePrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName));

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.104696f, -0.02666293f);
                var size = new Vector3(1f, 2.013306f, 0.6097867f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;


                prefab.SetActive(false);
                var storageContainer = prefab.AddComponent<FCSFridge>();
                storageContainer.Initialize(QPatch.Configuration.SeaBreezeStorageLimit, 1, 1, FriendlyName, ClassID);
                storageContainer.enabled = false;
                prefab.SetActive(true);


                prefab.AddComponent<AnimationManager>();
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.EnsureComponent<SeaBreezeController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(SeaBreezeKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
