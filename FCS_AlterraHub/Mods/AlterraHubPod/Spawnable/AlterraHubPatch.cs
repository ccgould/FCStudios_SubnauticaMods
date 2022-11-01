using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubPod.Mono;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Enums;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
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

        public override GameObject GetGameObject()
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
            //var skyApplierInterior = interiorModels.gameObject.AddComponent<SkyApplier>();
            //skyApplierInterior.renderers = interiorModels.GetComponentsInChildren<Renderer>();
            //skyApplierInterior.anchorSky = Skies.BaseInterior;
            //skyApplierInterior.SetSky(Skies.BaseInterior);

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, prefab, Color.black);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, prefab, 4f);

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


            var sr = prefab.AddComponent<AlterraHubPodController>();

            //Necessary for SubRoot class Update behaviour so it doesn't return an error every frame.
            var lod = prefab.AddComponent<BehaviourLOD>();

            var pr = prefab.AddComponent<BasePowerRelay>();
            pr.maxOutboundDistance = 0;
            pr.subRoot = sr;

            var portManager = prefab.AddComponent<PortManager>();
            var droneDoorTrigger = GameObjectHelpers.FindGameObject(prefab, "DockTrigger").AddComponent<DroneDoorTrigger>();
            droneDoorTrigger.Port = portManager;
            var dronePortController = prefab.AddComponent<AlterraHubLifePodDronePortController>();
            dronePortController.PortManager = portManager;
            return prefab;
        }
    }

    internal class AlterraHubLifePodDronePortController : MonoBehaviour, IDroneDestination
    {
        private Animator _animator;
        public Transform BaseTransform { get; set; }
        public string BaseId { get; set; }
        public bool IsOccupied { get; }


        private List<Transform> _paths;
        private int DOORSTATEHASH;
        private int _portID;
        private DroneController _assignedDrone;
        private GameObject _spawnPoint;
        private GameObject _entryPoint;
        private string _baseID;
        public static AlterraHubLifePodDronePortController main;
        public PortManager PortManager;
        private bool _isPlaying;
        private int _stateHash;


        private void Awake()
        {
            _stateHash = Animator.StringToHash("State");
            main = this;
            var prefabIdent = gameObject.GetComponent<PrefabIdentifier>().id;
            _baseID = BaseManager.FindManager(prefabIdent)?.BaseID;
            DOORSTATEHASH = Animator.StringToHash("DronePortDoorState");
            _animator = gameObject.GetComponentInChildren<Animator>();
            _paths = GameObjectHelpers.FindGameObject(gameObject, "DronePort_DockingPaths").GetChildrenT().ToList();
            _entryPoint = _paths[0].gameObject;
            PortManager.RegisterDronePort(this);
            _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPoint");
        }

        internal PortManager GetDronePortController()
        {
            return PortManager;
        }

        public List<Transform> GetPaths()
        {
            return _paths;
        }

        public void Offload(DroneController order)
        {

        }

        public Transform GetDockingPosition()
        {
            return _paths.Last();
        }

        public void OpenDoors()
        {
            _animator.SetBool(DOORSTATEHASH,true);
        }

        public void CloseDoors()
        {
            _animator.SetBool(DOORSTATEHASH, false);
        }

        public string GetBaseName()
        {
            return "Alterra Hub Pod";
        }

        public void Depart(DroneController droneController)
        {
            SetDockedDrone(null);
        }

        public void Dock(DroneController droneController)
        {
            SetDockedDrone(droneController);
        }

        public int GetPortID()
        {
            return _portID;
        }

        public string GetPrefabID()
        {
            return gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                   gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
        }

#if SUBNAUTICA_STABLE
        public DroneController SpawnDrone()
        {
            var drone = Instantiate(CraftData.GetPrefabForTechType(Mod.AlterraTransportDroneTechType), _spawnPoint.transform.position, _spawnPoint.transform.rotation);
            _assignedDrone = drone.GetComponent<DroneController>();
            return _assignedDrone;
        }
#endif

        public IEnumerator SpawnDrone(Action<DroneController> callback)
        {
            QuickLogger.Info("Spawn Drone");
            var task = CraftData.GetPrefabForTechTypeAsync(Mod.AlterraTransportDroneTechType, false);
            yield return task;

            QuickLogger.Info("Drone Prefab");
            var prefab = task.GetResult();
            if (prefab != null)
            {
                _assignedDrone = Instantiate(prefab).GetComponent<DroneController>();
                QuickLogger.Info("Invoke Spawn Drone");
                callback?.Invoke(_assignedDrone);
            }
        }


        public Transform GetEntryPoint()
        {
            return _entryPoint == null ? null : _entryPoint?.transform;
        }

        public string GetBaseID()
        {
            return _baseID;
        }

        public void SetInboundDrone(DroneController droneController)
        {
            
        }

        public void PlayAnimationState(DronePortAnimation state, Action callBack)
        {
            switch (state)
            {
                case DronePortAnimation.None:
                    PlayAnimation(0, "IDLE", callBack);
                    CloseDoors();
                    break;
                case DronePortAnimation.Docking:
                    PlayAnimation(2, "Drone_Approaching", callBack);
                    OpenDoors();
                    break;
                case DronePortAnimation.Departing:
                    PlayAnimation(1, "Drone_Departing", callBack);
                    OpenDoors();
                    break;
                default:
                    PlayAnimation(0, "IDLE", callBack);
                    CloseDoors();
                    break;
            }
        }

        public void ClearInbound()
        {
            
        }

        public void SetDockedDrone(DroneController drone)
        {
            _assignedDrone = drone;
        }

        private void PlayAnimation(int value, string animationName, Action callBack)
        {
            if (_isPlaying) return;

            StartCoroutine(PlayAndWaitForAnim(value, animationName, () =>
                {
                    _isPlaying = false;
                    callBack?.Invoke();
                    CloseDoors();
                    _animator.SetInteger(_stateHash, 0);
                }
            ));
        }

        public IEnumerator PlayAndWaitForAnim(int valueToSet, string stateName, Action onCompleted)
        {
            _isPlaying = true;
            _animator.SetInteger(_stateHash, valueToSet);

            //Wait until we enter the current state
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                //QuickLogger.Debug("Wait until we enter the current state.");
                yield return null;
            }

            //Now, Wait until the current state is done playing
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.99f)
            {
                //QuickLogger.Debug("Now, Wait until the current state is done playing");
                yield return null;
            }

            //Done playing. Do something below!
            onCompleted?.Invoke();

            yield break;
        }

    }
}
