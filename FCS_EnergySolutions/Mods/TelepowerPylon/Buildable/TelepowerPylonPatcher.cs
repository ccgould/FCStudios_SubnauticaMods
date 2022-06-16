using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Buildable
{
    internal partial class TelepowerPylonBuildable : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        internal const string TelepowerPylonTabID = "TP";
        internal const string TelepowerPylonFriendlyName = "Telepower Pylon";
        internal const string TelepowerPylonModName = "TelepowerPylon";
        internal const string TelepowerPylonDescription = "With a Telepower Pylon, you can send or receive energy wirelessly across vast distances. Requires one to Send / Push and another to Receive / Pull.";
        internal static string TelepowerPylonKitClassID => $"{TelepowerPylonModName}_Kit";
        internal static string TelepowerPylonClassName => TelepowerPylonModName;
        internal static string TelepowerPylonPrefabName => TelepowerPylonModName;

        public TelepowerPylonBuildable() : base(TelepowerPylonClassName, TelepowerPylonFriendlyName, TelepowerPylonDescription)
        {
            _prefab = ModelPrefab.GetPrefab(TelepowerPylonPrefabName, true);

            OnStartedPatching += () =>
            {
                var telepowerPylonKit = new FCSKit(TelepowerPylonKitClassID, TelepowerPylonFriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                telepowerPylonKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, AsyncExtensions.ToTechType(TelepowerPylonKitClassID), 120000, StoreCategory.Energy);
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var size = new Vector3(2.534196f, 10.11668f, 2.51761f);
                var center = new Vector3(-4.768372e-07f, 5.354942f, -0.01068687f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>(); 

                constructable.allowedOutside = true;
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.placeDefaultDistance = 5;
                constructable.placeMinDistance = 5;
                constructable.placeMaxDistance = 10;
                constructable.techType = TechType;
                constructable.forceUpright = true;
                
                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                PowerRelay solarPowerRelay = CraftData.GetPrefabForTechType(TechType.SolarPanel).GetComponent<PowerRelay>();
                
                var pFX = prefab.AddComponent<PowerFX>();
                pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
                pFX.attachPoint = prefab.transform;

                var pr = prefab.AddComponent<PowerRelay>();
                pr.powerFX = pFX;
                pr.dontConnectToRelays = true;
                pr.maxOutboundDistance = 15;

                //var powerSource = prefab.EnsureComponent<PowerSource>();

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<TelepowerPylonController>();


                //Resources.UnloadAsset(solarPowerRelay);

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
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            var size = new Vector3(2.534196f, 10.11668f, 2.51761f);
            var center = new Vector3(-4.768372e-07f, 5.354942f, -0.01068687f);

            GameObjectHelpers.AddConstructableBounds(prefab, size, center);

            var model = prefab.FindChild("model");

            //========== Allows the building animation and material colors ==========// 
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;
            //========== Allows the building animation and material colors ==========// 

            // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
            var lwe = prefab.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

            // Add constructible
            var constructable = prefab.AddComponent<Constructable>();

            constructable.allowedOutside = true;
            constructable.allowedInBase = false;
            constructable.allowedOnGround = true;
            constructable.allowedOnWall = false;
            constructable.rotationEnabled = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedInSub = false;
            constructable.allowedOnConstructables = false;
            constructable.model = model;
            constructable.placeDefaultDistance = 5;
            constructable.placeMinDistance = 5;
            constructable.placeMaxDistance = 10;
            constructable.techType = TechType;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel);
            yield return taskResult;
            var solarpanel = taskResult.GetResult();
            PowerRelay solarPowerRelay = solarpanel.GetComponent<PowerRelay>();
            solarPowerRelay.enabled = false;

            var pFX = prefab.AddComponent<PowerFX>();
            pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            pFX.attachPoint = prefab.transform;

            var pr = prefab.AddComponent<PowerRelay>();
            pr.powerFX = pFX;
            pr.maxOutboundDistance = 15;
            pr.powerSystemPreviewPrefab = solarPowerRelay.powerSystemPreviewPrefab;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<TelepowerPylonController>();
            
            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(AsyncExtensions.ToTechType(TelepowerPylonKitClassID),1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
    }
}