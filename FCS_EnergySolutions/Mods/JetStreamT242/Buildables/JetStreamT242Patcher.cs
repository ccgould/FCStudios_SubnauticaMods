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
using FCS_EnergySolutions.Mods.JetStreamT242.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_EnergySolutions.Mods.JetStreamT242.Buildables
{

    internal class JetStreamT242Patcher : SMLHelper.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        public JetStreamT242Patcher() : base(Mod.JetStreamT242ClassName, Mod.JetStreamT242FriendlyName, Mod.JetStreamT242Description)
        {
            _prefab = ModelPrefab.GetPrefab(Mod.JetStreamT242PrefabName);

            OnStartedPatching += () =>
            {
                var jetStreamT242Kit = new FCSKit(Mod.JetStreamT242KitClassID, FriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                jetStreamT242Kit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.JetStreamT242KitClassID.ToTechType(),  157500, StoreCategory.Energy);
            };
        }
        
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);

            var size = new Vector3(2.493512f, 1.875936f, 1.439421f);
            var center = new Vector3(0.07963049f, 1.088284f, 0f);

            //GameObjectHelpers.AddConstructableBounds(prefab, size, center);

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
            var constructable = prefab.EnsureComponent<Constructable>();

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

            var result = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel, false, result);
            PowerRelay solarPowerRelay = result.Get().GetComponent<PowerRelay>();

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
            prefab.AddComponent<JetStreamT242Controller>();

            Resources.UnloadAsset(solarPowerRelay);
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
                    new Ingredient(Mod.JetStreamT242KitClassID.ToTechType(),1)
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
