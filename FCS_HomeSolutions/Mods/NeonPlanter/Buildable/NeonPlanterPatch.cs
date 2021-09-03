using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.NeonPlanter.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.NeonPlanter.Buildable
{
    internal class NeonPlanterPatch : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _prefab;
        private Settings _settings;
        internal const string NeonPlanterClassID = "NeonPlanter";
        internal const string NeonPlanterFriendly = "Neon Planter";
        internal const string NeonPlanterDescription = "A planter that can be placed outside with color changing.";
        internal const string NeonPlanterPrefabName = "FCS_NeonPlanter";
        internal static string NeonPlanterKitClassID = $"{NeonPlanterClassID}_Kit";
        internal const string NeonPlanterTabID = "NOP";
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        
        public NeonPlanterPatch() : base(NeonPlanterClassID, NeonPlanterFriendly, NeonPlanterDescription)
        {
            _settings = new Settings
            {
                KitClassID = NeonPlanterKitClassID,
                Size = new Vector3(0.7929468f, 0.3463891f, 0.7625999f),
                Center = new Vector3(0f, 0.2503334f, 0f)
            };

            _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(NeonPlanterPrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            
            OnStartedPatching += () =>
            {
                QuickLogger.Debug("Patched Kit");
                var outDoorPlanterkit = new FCSKit(_settings.KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                outDoorPlanterkit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, NeonPlanterKitClassID.ToTechType(), 21000, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                prefab.SetActive(false);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.shader = shader;
                    }
                }

                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                var sRoot = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
                var slot1 = GameObjectHelpers.FindGameObject(prefab, "slot1");
                var slot2 = GameObjectHelpers.FindGameObject(prefab, "slot2");
                var slot3 = GameObjectHelpers.FindGameObject(prefab, "slot3");
                var slot4 = GameObjectHelpers.FindGameObject(prefab, "slot4");
                var slot_big = GameObjectHelpers.FindGameObject(prefab, "slot_big");

                var gPlant = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
                gPlant.AddComponent<ChildObjectIdentifier>();

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                
                var storage = UWEHelpers.CreateStorageContainer(prefab,sRoot,ClassID,FriendlyName,2,2);
                var planter = prefab.AddComponent<Planter>();
                planter.slots = new[]
                {
                    slot1.transform,
                    slot2.transform,
                    slot3.transform,
                    slot4.transform,
                };

                planter.bigSlots = new []{ slot_big.transform };
                planter.grownPlantsRoot = gPlant.transform;
                planter.storageContainer = storage;
                planter.environment = Planter.PlantEnvironment.Air;
                planter.constructable = constructable;
                planter.isIndoor = true;
                prefab.SetActive(true);
                prefab.AddComponent<NeonPlanterController>();
                
                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", ClassID);
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
                    new Ingredient(NeonPlanterKitClassID.ToTechType(), 1)
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
