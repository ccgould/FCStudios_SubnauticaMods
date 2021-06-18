using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubDepot.Mono;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using UnityEngine;
using Valve.VR;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class AlterraDronePortController : FcsDevice, IFCSSave<SaveData>, IDroneDestination
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AlterraDronePortEntry _savedData;
        private List<Transform> _paths;
        private List<PortDoorController> _doors = new List<PortDoorController>();
        private DroneController _assignedDrone;
        private GameObject _spawnPoint;
        private DronePadEntryPoint _entryPoint;
        private int _portID;

        private DockTriggerController _dockTrigger;
        private bool _hasIncomingFlight;
        public override bool IsOperational => IsInitialized && IsConstructed;
        public Transform BaseTransform { get; set; }
        public bool IsFull => GetIsFull();
        public override bool BypassRegisterCheck { get; } = true;

        private bool GetIsFull()
        {
            var devices = Manager.GetDevices(Mod.AlterraHubDepotTabID);

            return devices == null || devices.All(x => ((AlterraHubDepotController) x).IsFull());
        }

        public List<Transform> GetPaths()
        {
            return _paths;
        }

        public Transform GetDockingPosition()
        {
            return _paths.Last();
        }

        void IDroneDestination.OpenDoors()
        {
            OpenDoors();
        }

        void IDroneDestination.CloseDoors()
        {
            CloseDoors();
        }

        public void DockDrone(DroneController droneController)
        {
            StartCoroutine(_entryPoint.DockDrone(droneController));
        }

        public string GetBaseName()
        {
            return Manager?.GetBaseName() ?? "Station Port";
        }

        public void Depart(DroneController droneController)
        {
            SetDockedDrone(null);
            SetIncomingFlight(false);
            _entryPoint.Depart(droneController);
        }


        public Transform GetEntryPoint()
        {
            return _entryPoint.transform;
        }

        public int GetPortID()
        {
            return _portID;
        }

        public bool HasDroneDocked()
        {
            if (_assignedDrone != null) return true;
            return _dockTrigger?.IsDocked ?? false;
        }

        public void SetDockedDrone(DroneController drone)
        {
            _assignedDrone = drone;
        }

        public int SetPortID(int id)
        {
            return _portID = id;
        }

        public void Offload(DroneController drone)
        {

            var order = drone.GetOrder();
            
            if (order == null)
            {
                QuickLogger.Error("Order was null when offloading.");
                return;
            };

            var devices = Manager.GetDevices(Mod.AlterraHubDepotTabID);

            if (devices == null)
            {
                QuickLogger.ModMessage( $"Failed to find any Hub Depots giving a refund.");
                foreach (CartItemSaveData cartItem in order)
                {
                    cartItem.Refund();
                }
                
                return;
            }

            var pendingItems = new List<TechType>();

            foreach (CartItemSaveData cartItem in order)
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
                    pendingItems.Add(cartItem.ReceiveTechType);
                }
            }

            foreach (FcsDevice device in devices)
            {
                var depot = (AlterraHubDepotController) device;
                //var avaliableSpace = depot.GetFreeSlotsCount();

                for (int i = pendingItems.Count - 1; i >= 0; i--)
                {
                    if (!depot.IsFull())
                    {
                        depot.AddItemToStorage(pendingItems[i]);
                        pendingItems.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

            }
            drone.OnOffloadCompleted();;
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
            _dockTrigger = GameObjectHelpers.FindGameObject(gameObject, "DockTrigger").AddComponent<DockTriggerController>();

           _entryPoint =  _paths[0].gameObject.AddComponent<DronePadEntryPoint>();

            _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPoint");

            var doors = GameObjectHelpers.FindGameObjects(gameObject, "DockingPadDoor", SearchOption.StartsWith);

            var door1 = doors.ElementAt(0).AddComponent<PortDoorController>();
            door1.OpenPosX = -1.25f;
            _doors.Add(door1);

            var door2 = doors.ElementAt(1).AddComponent<PortDoorController>();
            door2.OpenPosX = 1.25f;
            _doors.Add(door2);

            CreateLadders();

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,AlterraHub.BasePrimaryCol);
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            IsInitialized = true;
        }

        public DroneController SpawnDrone()
        {
            var drone = GameObject.Instantiate(CraftData.GetPrefabForTechType(Mod.AlterraTransportDroneTechType), _spawnPoint.transform.position, _spawnPoint.transform.rotation);
            _assignedDrone = drone.GetComponent<DroneController>();
            _assignedDrone.Initialize(this,this);
            return _assignedDrone;
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

            QuickLogger.Debug($"Closing {GetBaseName()} doors:");
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

        public bool HasRoomFor(List<Vector2int> sizes)
        {
            var devices = Manager.GetDevices(Mod.AlterraHubDepotTabID);

            var totalsizeReq = sizes.Sum(x => x.x) + sizes.Sum(x => x.y);

            int total = 0;

            foreach (FcsDevice fcsDevice in devices)
            {
                var device = (AlterraHubDepotController)fcsDevice;
                total += device.GetFreeSlotsCount();
            }

            return total >= totalsizeReq;
        }

        public string GetStatus()
        {
            return "N/A";
        }

        public bool HasIncomingFlight()
        {
            if (_dockTrigger?.IsDocked ?? true) return true;
            return _hasIncomingFlight;
        }

        public void SetIncomingFlight(bool value)
        {
            _hasIncomingFlight = value;
        }
    }
}
