using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class AlterraDronePortController : FcsDevice, IDroneDestination
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AlterraDronePortEntry _savedData;
        private List<Transform> _paths;
        private List<PortDoorController> _doors = new List<PortDoorController>();
        private DroneController _assignedDrone;
        public Transform BaseTransform { get; set; }

        public List<Dictionary<string, List<EncyclopediaEntryData>>> _enData =>
            FCSAlterraHubService.InternalAPI.EncyclopediaEntries;

        public List<Transform> GetPaths()
        {
            return _paths;
        }

        public void Offload(Dictionary<TechType, int> order, Action onOffloadCompleted)
        {
            onOffloadCompleted?.Invoke();
        }

        public override void Awake()
        {
            base.Awake();
            BaseTransform = transform;
        }

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DronePortPadHubNewTabID, Mod.ModPackID);
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor());
                }

                _runStartUpOnEnable = false;
            }
        }

        #endregion

        public override void Initialize()
        {
            _paths = GameObjectHelpers.FindGameObject(gameObject, "DronePort_DockingPaths").GetChildrenT().ToList();

            var doors = GameObjectHelpers.FindGameObjects(gameObject, "DockingPadDoor", SearchOption.StartsWith);

            var door1 = doors.ElementAt(0).AddComponent<PortDoorController>();
            door1.OpenPosX = -1.25f;
            _doors.Add(door1);

            var door2 = doors.ElementAt(1).AddComponent<PortDoorController>();
            door2.OpenPosX = 1.25f;
            _doors.Add(door2);

            CreateLadders();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
        }

        internal void SpawnDrone()
        {
            var lastWayPoint = _paths.GetLast();
            var drone = GameObject.Instantiate(CraftData.GetPrefabForTechType(Mod.AlterraTransportDroneTechType),lastWayPoint.position,lastWayPoint.rotation);
            _assignedDrone = drone.GetComponent<DroneController>();
            _assignedDrone.Initialize(this);
        }
        
        internal void OpenDoors()
        {
            foreach (PortDoorController door in _doors)
            {
                door.Open();
            }
        }

        internal void CloseDoors()
        {
            foreach (PortDoorController door in _doors)
            {
                door.Close();
            }
        }

        private void TestTransfer()
        {
            _assignedDrone.ShipOrder(new Dictionary<TechType, int>{{TechType.Copper,1}}, this,AlterraFabricatorStationController.main.GetAvailablePort());
        }

        private void CreateLadders()
        {
            if(!IsConstructed) return;
            var t01 = GameObjectHelpers.FindGameObject(gameObject, "Trigger_ladder01").AddComponent<LadderController>();
            t01.Set(GameObjectHelpers.FindGameObject(gameObject, "ladder01_spawnpoint"));

            var t02 = GameObjectHelpers.FindGameObject(gameObject, "Trigger_ladder02").AddComponent<LadderController>();
            t02.Set(GameObjectHelpers.FindGameObject(gameObject, "ladder02_spawnpoint"));
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetAlterraDronePortSaveData(id);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraDronePortEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            newSaveData.AlterraDronePortEntries.Add(_savedData);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }
    }

    internal class PortDoorController : MonoBehaviour
    {
        private Vector3 closedPos;
        private Vector3 openPos;
        private bool doorOpen;
        private FMOD_CustomEmitter _openSound;

        private void Start()
        {
            closedPos = transform.position;
            openPos = transform.TransformPoint(new Vector3(OpenPosX, 0f, 0f));

            _openSound = gameObject.AddComponent<FMOD_CustomEmitter>();
            var openDoor = ScriptableObject.CreateInstance<FMODAsset>();
            openDoor.id = "keypad_door_open";
            openDoor.path = "event:/env/keypad_door_open";
            _openSound.asset = openDoor;
            _openSound.restartOnPlay = true;

            if (StartDoorOpen || doorOpen)
            {
                doorOpen = true;
                transform.position = openPos;
            }
        }

        public float OpenPosX { get; set; }

        public bool StartDoorOpen { get; set; }

        private void Update()
        {
            Vector3 vector = transform.position;
            vector = Vector3.Lerp(vector, doorOpen ? openPos : closedPos, Time.deltaTime * 2f);
            transform.position = vector;
        }

        public void Open()
        {
            doorOpen = true;
            PlaySound();
        }

        private void PlaySound()
        {
            if (_openSound)
            {
                _openSound.Play();
            }
        }

        public void Close()
        {
            doorOpen = false;
            PlaySound();
        }
    }
}
