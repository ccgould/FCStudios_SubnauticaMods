using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_EnergySolutions.TelepowerPylon.Buildables
{
    using SMLHelper.V2.Assets;
    internal partial class TelepowerPylonBuildable : Buildable
    {
        private GameObject _prefab;

        public TelepowerPylonBuildable() : base(Mod.TelepowerPylonClassName, Mod.TelepowerPylonFriendlyName, Mod.TelepowerPylonDescription)
        {
            _prefab = ModelPrefab.GetPrefab(Mod.TelepowerPylonPrefabName, true);

            OnStartedPatching += () =>
            {
                var telepowerPylonKit = new FCSKit(Mod.TelepowerPylonKitClassID, Mod.TelepowerPylonFriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                telepowerPylonKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.TelepowerPylonKitClassID.ToTechType(), 120000, StoreCategory.Energy);
            };
        }

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
                
                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                PowerRelay solarPowerRelay = CraftData.GetPrefabForTechType(TechType.SolarPanel).GetComponent<PowerRelay>();

                var pFX = prefab.AddComponent<PowerFX>();
                pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
                pFX.attachPoint = prefab.transform;

                var pr = prefab.AddComponent<PowerRelay>();
                pr.powerFX = pFX;
                pr.maxOutboundDistance = 100;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<TelepowerPylonController>();


                Resources.UnloadAsset(solarPowerRelay);

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.TelepowerPylonKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{ClassID}.png")));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.TelepowerPylonKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
    }
}