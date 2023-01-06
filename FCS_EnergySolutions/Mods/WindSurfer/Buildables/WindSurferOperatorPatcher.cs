using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_EnergySolutions.Mods.WindSurfer.Buildables
{
    internal partial class WindSurferOperatorBuildable : Craftable
    {
        private GameObject _prefab;
        public override TechGroup GroupForPDA => TechGroup.Constructor;
        public override TechCategory CategoryForPDA => TechCategory.Constructor;
        public override TechType RequiredForUnlock => TechType.Constructor;
        public override CraftTree.Type FabricatorType => CraftTree.Type.Constructor;
        public override string[] StepsToFabricatorTab => new[] { "FCSWindSurfer" };
        public override float CraftingTime => 20f;


        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        public WindSurferOperatorBuildable() : base(Mod.WindSurferOperatorClassName, Mod.WindSurferOperatorFriendlyName, Mod.WindSurferOperatorDescription)
        {
            _prefab = ModelPrefab.GetPrefab(Mod.WindSurferOperatorPrefabName, true);

            OnStartedPatching += () =>
            {
                var WindSurferOperatorKit = new FCSKit(Mod.WindSurferOperatorKitClassID, Mod.WindSurferOperatorFriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                WindSurferOperatorKit.Patch();
            };
            
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.WindSurferOperatorKitClassID.ToTechType(), 1200000, StoreCategory.Energy);
            };
        }
        
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
            var solarpanel = taskResult.GetResult();
            PowerRelay solarPowerRelay = solarpanel.GetComponent<PowerRelay>();
            solarPowerRelay.enabled = false;

            var pFX = prefab.AddComponent<PowerFX>();
            pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            pFX.attachPoint = prefab.transform;

            var pr = prefab.AddComponent<PowerRelay>();
            pr.powerFX = pFX;
            pr.maxOutboundDistance = 15;

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<WindSurferOperatorController>();


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
                    new Ingredient(Mod.WindSurferOperatorKitClassID.ToTechType(),1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }

        BuildBotPath CreateBuildBotPath(GameObject gameobjectWithComponent, Transform parent)
        {
            var comp = gameobjectWithComponent.AddComponent<BuildBotPath>();
            comp.points = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                comp.points[i] = parent.GetChild(i);
            }
            return comp;
        }
    }

    internal class WindSurferOperatorSubroot : SubRoot
    {
        public override void Awake()
        {
            var interiorTrigger = gameObject.FindChild("InteriorTrigger").EnsureComponent<InteriorTrigger>();
            this.LOD = GetComponent<BehaviourLOD>();
            this.rb = GetComponent<Rigidbody>();
            this.isBase = true;
            this.lightControl = GetComponentInChildren<LightingController>();
            this.modulesRoot = gameObject.transform;
            this.powerRelay = GetComponent<BasePowerRelay>();
            BaseManager.FindManager(this);

            if (Main.Configuration.IsTelepowerPylonEnabled)
            {
                var baseTPowerManager = gameObject.EnsureComponent<BaseTelepowerPylonManager>();
                baseTPowerManager.SubRoot = this;
                BaseTelepowerPylonManager.RegisterPylonManager(baseTPowerManager);
            }
        }

        private void OnDestroy()
        {
            if (Main.Configuration.IsTelepowerPylonEnabled)
            {
                var baseTPowerManager = gameObject.GetComponent<BaseTelepowerPylonManager>();
                BaseTelepowerPylonManager.UnRegisterPylonManager(baseTPowerManager);
            }
        }
    }

    internal class InteriorTrigger : MonoBehaviour
    {
        private SubRoot _subRoot;


        private void Awake()
        {
            _subRoot = gameObject.GetComponentInParent<SubRoot>();
        }


        private void OnTriggerEnter(Collider collider)
        {
            if (_subRoot != null)
            {
                Player.main.SetCurrentSub(_subRoot);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (_subRoot != null)
            {
                Player.main.SetCurrentSub(null);
            }
        }
    }
}