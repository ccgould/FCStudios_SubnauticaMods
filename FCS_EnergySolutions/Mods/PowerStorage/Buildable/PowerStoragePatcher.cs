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
using FCS_EnergySolutions.Mods.PowerStorage.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_EnergySolutions.Mods.PowerStorage.Buildable
{
    internal class PowerStoragePatcher : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
        public PowerStoragePatcher() : base(Mod.PowerStorageClassName, Mod.PowerStorageFriendlyName, Mod.PowerStorageDescription)
        {

            OnStartedPatching += () =>
            {
                var powerStorageKit = new FCSKit(Mod.PowerStorageKitClassID, FriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                powerStorageKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.PowerStorageKitClassID.ToTechType(), 101250, StoreCategory.Energy);
            };
        }



#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.PowerStoragePrefab);

                var center = new Vector3(0f, 4.330446f, 0f);
                var size = new Vector3(7.856985f, 7.846021f, 7.994404f);

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
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;
                constructable.placeDefaultDistance = 5;
                constructable.placeMinDistance = 5;
                constructable.placeMaxDistance = 10;
                constructable.forceUpright = true;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;

                PowerRelay solarPowerRelay = CraftData.GetPrefabForTechType(TechType.SolarPanel).GetComponent<PowerRelay>();
                
                var ps = prefab.AddComponent<PowerSource>();
                ps.maxPower = 10000f;

                var pFX = prefab.AddComponent<PowerFX>();
                pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
                pFX.attachPoint = GameObjectHelpers.FindGameObject(prefab,"connection_port").transform;

                var pr = prefab.AddComponent<PowerRelay>();
                pr.powerFX = pFX;
                pr.maxOutboundDistance = 20;
                pr.internalPowerSource = ps;

                prefab.AddComponent<PowerStorageController>();

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
                var prefab = GameObject.Instantiate(ModelPrefab.PowerStoragePrefab);

                var center = new Vector3(0f, 4.330446f, 0f);
                var size = new Vector3(7.856985f, 7.846021f, 7.994404f);

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
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;
                constructable.placeDefaultDistance = 5;
                constructable.placeMinDistance = 5;
                constructable.placeMaxDistance = 10;
                constructable.forceUpright = true;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;

                var result = new TaskResult<GameObject>();
                yield return CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel, false, result);
                PowerRelay solarPowerRelay = result.Get().GetComponent<PowerRelay>();
                
                var ps = prefab.AddComponent<PowerSource>();
                ps.maxPower = 10000f;

                var pFX = prefab.AddComponent<PowerFX>();
                pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
                pFX.attachPoint = GameObjectHelpers.FindGameObject(prefab,"connection_port").transform;

                var pr = prefab.AddComponent<PowerRelay>();
                pr.powerFX = pFX;
                pr.maxOutboundDistance = 20;
                pr.internalPowerSource = ps;

                prefab.AddComponent<PowerStorageController>();

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
                    new Ingredient(Mod.PowerStorageKitClassID.ToTechType(),1)
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
