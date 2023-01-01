using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubPod.Mono;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;
using static Constructor;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Spawnable
{
    internal class AlterraHubPatch : SMLHelper.V2.Assets.Spawnable
    {
        public AlterraHubPatch() : base(Mod.AlterraHubStationClassID, Mod.AlterraHubStationFriendly, Mod.AlterraHubStationDescription)
        {
            OnFinishedPatching += () =>
            {
                //var spawnLocation = new Vector3(-110f, -27.33f, 557.4f);
                var spawnLocation = new Vector3(-21.7f, -38.2f, 505.3f);
                var spawnRotation = Quaternion.Euler(Vector3.zero);
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType, spawnLocation, spawnRotation));
            };
        }

        public override WorldEntityInfo EntityInfo => new()
        {
            classId = ClassID,
            cellLevel = LargeWorldEntity.CellLevel.Global,
            localScale = Vector3.one,
            slotType = EntitySlot.Type.Large,
            techType = this.TechType
        };

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(AlterraHub.AlterraHubFabricatorPrefab);

            prefab.SetActive(false);

            prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            prefab.EnsureComponent<PrefabIdentifier>().classId = ClassID;
            prefab.EnsureComponent<ImmuneToPropulsioncannon>();
            prefab.EnsureComponent<TechTag>().type = TechType;
            prefab.EnsureComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>();

            var rb = prefab.EnsureComponent<Rigidbody>();
            rb.mass = 10000f;
            rb.isKinematic = true;

            foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
            {
                col.gameObject.EnsureComponent<ConstructionObstacle>();
            }

            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID, 1f, 2f, 0.2f);

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

            var interiorModels = GameObjectHelpers.FindGameObject(prefab, "static_interior_mesh");
            var skyApplierInterior = interiorModels.AddComponent<SkyApplier>();
            skyApplierInterior.renderers = interiorModels.GetComponentsInChildren<Renderer>();
            skyApplierInterior.anchorSky = Skies.BaseInterior;
            skyApplierInterior.SetSky(Skies.BaseInterior);

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, prefab, Color.black);
            //MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, prefab, 0f);
            //MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseFloor01Interior, prefab, 0f);
            //MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseOpaqueInterior, prefab, 0f);




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

            lights.emissiveController.intensities = new[] { 1f, 0.7f, 0f };


            var sr = prefab.AddComponent<AlterraHubPodController>();
            sr.skyappl = skyApplierInterior;



            //Necessary for SubRoot class Update behaviour so it doesn't return an error every frame.
            var lod = prefab.AddComponent<BehaviourLOD>();

            var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel);
            yield return taskResult;

            PowerRelay solarPowerRelay = taskResult.GetResult().GetComponent<PowerRelay>();


            var pFX = prefab.AddComponent<PowerFX>();
            pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            pFX.attachPoint = prefab.transform;

            var pr = prefab.AddComponent<PowerRelay>();
            pr.maxOutboundDistance = 0;


            var portManager = prefab.AddComponent<PortManager>();

            sr.PortManager = portManager;


            var droneDoorTrigger = GameObjectHelpers.FindGameObject(prefab, "DockTrigger").AddComponent<DroneDoorTrigger>();
            droneDoorTrigger.PortManager = portManager;

            var dronePortController = prefab.AddComponent<AlterraHubLifePodDronePortController>();
            dronePortController.PortManager = portManager;

            gameObject.Set(prefab);
            yield break;
        }
    }
}
