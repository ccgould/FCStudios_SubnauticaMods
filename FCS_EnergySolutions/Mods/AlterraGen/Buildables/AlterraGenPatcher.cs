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
using FCS_EnergySolutions.Mods.AlterraGen.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_EnergySolutions.Mods.AlterraGen.Buildables
{
    internal partial class AlterraGenBuildable : SMLHelper.Assets.Buildable
    {
        private GameObject _prefab;
        public override TechType RequiredForUnlock => FCSAlterraHubService.PublicAPI.IsInOreBuildMode() ? Mod.AlterraGenKitClassID.ToTechType() : TechType.None;
        
        public AlterraGenBuildable() : base(Mod.AlterraGenModClassName, Mod.AlterraGenModFriendlyName, Mod.AlterraGenModDescription)
        {
            
            _prefab = ModelPrefab.GetPrefab(Mod.AlterraGenModPrefabName,true);

            OnStartedPatching += () =>
            {
                var alterraGenKit = new FCSKit(Mod.AlterraGenKitClassID, Mod.AlterraGenModFriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                alterraGenKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                AdditionalPatching();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.AlterraGenKitClassID.ToTechType(), 90000, StoreCategory.Energy);
            };
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            //Scale the object
            prefab.transform.localScale += new Vector3(0.24f, 0.24f, 0.24f);

            var size = new Vector3(2.493512f, 1.875936f, 1.439421f);
            var center = new Vector3(0.07963049f, 1.088284f, 0f);

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
            constructable.allowedInBase = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnWall = false;
            constructable.rotationEnabled = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedInSub = false;
            constructable.allowedOnConstructables = false;
            constructable.model = model;
            constructable.techType = TechType;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel);
            yield return taskResult;

            PowerRelay solarPowerRelay = taskResult.GetResult().GetComponent<PowerRelay>();

            var ps = prefab.AddComponent<PowerSource>();
            ps.maxPower = 500f;

            var pFX = prefab.AddComponent<PowerFX>();
            pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            pFX.attachPoint = prefab.transform;

            var pr = prefab.AddComponent<PowerRelay>();
            pr.powerFX = pFX;
            pr.maxOutboundDistance = 15;
            pr.internalPowerSource = ps;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<AlterraGenController>();


            Resources.UnloadAsset(solarPowerRelay);

            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            TechData customFabRecipe;

            if (FCSAlterraHubService.PublicAPI.IsInOreBuildMode())
            {
                // Create and associate recipe to the new TechType
                customFabRecipe = new RecipeData()
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>()
                    {
                        new Ingredient(TechType.Glass, 2),
                        new Ingredient(TechType.PlasteelIngot, 2),
                        new Ingredient(TechType.AdvancedWiringKit, 1),
                        new Ingredient(TechType.Silicone, 2),
                        new Ingredient(TechType.Lubricant, 1)
                    }
                };
            }
            else
            {
                customFabRecipe = new RecipeData()
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>()
                    {
                        new Ingredient(Mod.AlterraGenKitClassID.ToTechType(),1)
                    }
                };
            }

            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;
    }
}