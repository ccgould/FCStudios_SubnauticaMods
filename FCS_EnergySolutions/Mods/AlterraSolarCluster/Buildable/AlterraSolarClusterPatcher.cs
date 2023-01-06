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
using FCS_EnergySolutions.Mods.AlterraSolarCluster.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_EnergySolutions.AlterraSolarCluster.Buildables
{
    internal partial class AlterraSolarClusterBuildable : SMLHelper.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
        private GameObject _prefab;
        public override string DiscoverMessage => $"{FriendlyName} Unlocked!";
        
        public AlterraSolarClusterBuildable() : base(Mod.AlterraSolarClusterModClassName, Mod.AlterraSolarClusterModFriendlyName, Mod.AlterraSolarClusterModDescription)
        {
            _prefab = ModelPrefab.GetPrefab(Mod.AlterraSolarClusterModPrefabName,true);

            OnStartedPatching += () =>
            {
                var AlterraSolarClusterKit = new FCSKit(Mod.AlterraSolarClusterKitClassID, Mod.AlterraSolarClusterModFriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                AlterraSolarClusterKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.AlterraSolarClusterKitClassID.ToTechType(), 450000, StoreCategory.Energy);
            };
        }



        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            //Scale the object
            prefab.transform.localScale += new Vector3(0.24f, 0.24f, 0.24f);

            var size = new Vector3(8.08f, 7.540555f, 8.08f);
            var center = new Vector3(0f, 4.026321f, 0f);

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
            lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

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

            PowerRelay solarPowerRelay = taskResult.GetResult().GetComponent<PowerRelay>();

            var ps = prefab.AddComponent<PowerSource>();
            ps.maxPower = 2975f;

            var pFX = prefab.AddComponent<PowerFX>();
            pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            pFX.attachPoint = prefab.transform;

            var pr = prefab.AddComponent<PowerRelay>();
            pr.powerFX = pFX;
            pr.maxOutboundDistance = 15;
            pr.internalPowerSource = ps;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<AlterraSolarClusterController>();


            Resources.UnloadAsset(solarPowerRelay);

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
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
                    new Ingredient(Mod.AlterraSolarClusterKitClassID.ToTechType(),1)
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