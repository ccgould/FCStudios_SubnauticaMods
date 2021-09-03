using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Cabinets.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.CrewLocker.Buildable
{
    internal class CrewLockerBuildable : SMLHelper.V2.Assets.Buildable
    {
        internal const string CrewLockerClassID = "FCSCrewLocker";
        internal const string CrewLockerFriendly = "Crew Locker";
        internal const string CrewLockerDescription = "A stylish furniture piece for storage and decoration";
        internal const string CrewLockerPrefabName = "FCS_CrewLocker";
        internal static string CrewLockerKitClassID = $"{CrewLockerClassID}_Kit";
        internal const string CrewLockerTabID = "CWL";

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        
        public CrewLockerBuildable() : base(CrewLockerClassID, CrewLockerFriendly, CrewLockerDescription)
        {

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(CrewLockerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, CrewLockerKitClassID.ToTechType(), 4500, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(FCSAssetBundlesService.PublicAPI.GetPrefabByName(CrewLockerPrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName));
                
                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.392986f, 0.07044983f);
                var size = new Vector3(0.7382165f, 2.641892f, 0.8591003f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

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
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                UWEHelpers.CreateStorageContainer(prefab, prefab.FindChild("StorageRoot"), ClassID,"Storage",6,8);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<CabinetController>();


                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);

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
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(CrewLockerKitClassID.ToTechType(), 1)
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
