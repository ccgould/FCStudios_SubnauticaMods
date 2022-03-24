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
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
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

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var center = new Vector3(0f, -0.7508821f, -0.01471424f);
                var size = new Vector3(21.9853f, 4.032976f, 21.93637f);
                GameObjectHelpers.AddConstructableBounds(prefab, size, center);
                
                var center1 = new Vector3(-0.04416943f, 3.445226f, -1.634275f);
                var size1 = new Vector3(10.03543f, 4.415778f, 5.564189f);
                GameObjectHelpers.AddConstructableBounds(prefab, size1, center1);
                
                var center2 = new Vector3(-0.5151188f, 1.820723f, 1.931715f);
                var size2 = new Vector3(5.648634f, 1.023046f, 1.70119f);
                GameObjectHelpers.AddConstructableBounds(prefab, size2, center2);
                
                var center3 = new Vector3(1.211275f, 0.902276f, 3.666859f);
                var size3 = new Vector3(2.013066f, 2.804552f, 2.079138f);
                GameObjectHelpers.AddConstructableBounds(prefab, size3, center3);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;

                //Load the rocket platform for reference. I only use it for the constructor animation and sounds.
                GameObject rocketPlatformReference = CraftData.GetPrefabForTechType(TechType.RocketBase);

                //Get the Transform of the models
                //Transform interiorModels = GameObjectHelpers.FindGameObject(prefab, "BuildingInsideMesh").transform;

                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global; //You don't want your vehicles to disappear when out of range

                //Adds a Rigidbody. So it can move.
                var rigidbody = prefab.AddComponent<Rigidbody>();
                rigidbody.mass = 10000f; //Has to be really heavy. I'm pretty sure it's measured in KG.

                //Basically an extension to Unity rigidbodys. Necessary for buoyancy.
                var worldForces = prefab.AddComponent<WorldForces>();
                worldForces.useRigidbody = rigidbody;
                worldForces.underwaterGravity =-20f; //Despite it being negative, which would apply downward force, this actually makes it go UP on the y axis.
                worldForces.aboveWaterGravity = 20f; //Counteract the strong upward force
                worldForces.waterDepth = -5f;

                //Determines the places the little build bots point their laser beams at.
                var buildBots = prefab.AddComponent<BuildBotBeamPoints>();

                Transform beamPointsParent = GameObjectHelpers.FindGameObject(prefab, "Buildbot_BeamPoints").transform;

                //These are arbitrarily placed.
                buildBots.beamPoints = new Transform[beamPointsParent.childCount];
                for (int i = 0; i < beamPointsParent.childCount; i++)
                {
                    buildBots.beamPoints[i] = beamPointsParent.GetChild(i);
                }

                //The path the build bots take to get to the ship to construct it.
                Transform pathsParent = GameObjectHelpers.FindGameObject(prefab, "BuildBotPaths").transform;

                //4 paths, one for each build bot to take.
                CreateBuildBotPath(prefab, pathsParent.GetChild(0));
                CreateBuildBotPath(prefab, pathsParent.GetChild(1));
                CreateBuildBotPath(prefab, pathsParent.GetChild(2));
                CreateBuildBotPath(prefab, pathsParent.GetChild(3));

                //The effects for the constructor.
                var vfxConstructing = prefab.AddComponent<VFXConstructing>();
                var rocketPlatformVfx = rocketPlatformReference.GetComponentInChildren<VFXConstructing>();
                vfxConstructing.ghostMaterial = rocketPlatformVfx.ghostMaterial;
                vfxConstructing.surfaceSplashSound = rocketPlatformVfx.surfaceSplashSound;
                vfxConstructing.surfaceSplashFX = rocketPlatformVfx.surfaceSplashFX;
                vfxConstructing.rBody = rigidbody;
                vfxConstructing.Regenerate();

                //Don't want it tipping over...
                var stabilizier = prefab.AddComponent<Stabilizer>();
                stabilizier.uprightAccelerationStiffness = 600f;

                //Add VFXSurfaces to adjust footstep sounds. This is technically not necessary for the interior colliders, however.
                foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
                {
                    var vfxSurface = col.gameObject.AddComponent<VFXSurface>();
                    vfxSurface.surfaceType = VFXSurfaceTypes.metal;
                }

                ////Sky appliers to make it look nicer. Not sure if it even makes a difference, but I'm sticking with it.
                //var skyApplierInterior = interiorModels.gameObject.AddComponent<SkyApplier>();
                //skyApplierInterior.renderers = interiorModels.GetComponentsInChildren<Renderer>();
                //skyApplierInterior.anchorSky = Skies.BaseInterior;
                //skyApplierInterior.SetSky(Skies.BaseInterior);

                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, prefab, Color.black);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, prefab, 4f);

                var pc = prefab.AddComponent<PlatformController>();
                pc.Ports = new[]
                {
                    GameObjectHelpers.FindGameObject(prefab, "Port_1"),
                    GameObjectHelpers.FindGameObject(prefab, "Port_2"),
                    GameObjectHelpers.FindGameObject(prefab, "Port_3"),
                    GameObjectHelpers.FindGameObject(prefab, "Port_4"),
                };

                //The SubRoot component needs a lighting controller. Works nice too. A pain to setup in script.
                var lights = prefab.FindChild("LightsParent").AddComponent<LightingController>();
                lights.lights = new MultiStatesLight[0];
                foreach (Transform child in lights.transform)
                {
                    var newLight = new MultiStatesLight();
                    newLight.light = child.GetComponent<Light>();
                    newLight.intensities =
                        new[]
                        {
                            1f, 0.5f, 0f
                        }; //Full power: intensity 1. Emergency : intensity 0.5. No power: intensity 0.
                    lights.RegisterLight(newLight);
                }


                //Necessary for SubRoot class Update behaviour so it doesn't return an error every frame.
                var lod = prefab.AddComponent<BehaviourLOD>();

                var sr = prefab.AddComponent<WindSurferOperatorSubroot>();

                var pr = prefab.AddComponent<BasePowerRelay>();
                pr.maxOutboundDistance = 0;
                pr.subRoot = sr;
                
                UWEHelpers.CreateStorageContainer(prefab.FindChild("InternalStorage"), prefab.FindChild("StorageRoot"), ClassID, "Storage Locker", 6, 8);

                prefab.AddComponent<WindSurferOperatorController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseBeaconLightEmissiveController, prefab, 6);
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

            prefab.AddComponent<TechTag>().type = TechType;
            prefab.AddComponent<WindSurferOperatorController>();


            Resources.UnloadAsset(solarPowerRelay);

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