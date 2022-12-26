using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubDepot.Mono;
using FCS_AlterraHub.Mods.Common.DroneSystem.Enums;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem
{
    internal class AlterraDronePortController : FcsDevice, IFCSSave<SaveData>, IDroneDestination, IHandTarget
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AlterraDronePortEntry _savedData;
        private List<Transform> _paths;
        private List<PortDoorController> _doors = new();
        private DroneController _assignedDrone;
        private GameObject _spawnPoint;
        private GameObject _entryPoint;
        private int _portID;
        private PortManager _portManager;
        private bool _isPlaying;
        private Animator _animator;
        private int _stateHash;
        private bool _hasTransportInbound => _droneInbound != null;
        private DroneController _droneInbound;

        public override bool IsOperational => IsInitialized && IsConstructed;
        public Transform BaseTransform { get; set; }
        public bool IsFull => GetIsFull();
        public override bool BypassRegisterCheck => true;

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_portManager != null)
            {
                _portManager.UnRegisterDronePort(this);
            }
        }

        private bool GetIsFull()
        {
            var devices = Manager.GetDevices(Mod.AlterraHubDepotTabID);

            if (devices == null) return true;

            foreach (FcsDevice device in devices)
            {
                if (device == null) continue;

                if (device is AlterraHubDepotController controller && !controller.IsFull()) return false;
            }

            return true;
        }

        public bool IsOccupied => _assignedDrone != null;

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

        public string GetBaseName()
        {
            return Manager?.GetBaseName() ?? "Station PortManager";
        }

        public void Depart(DroneController droneController)
        {
            SetDockedDrone(null);
        }

        internal void ClearInbound()
        {
            _droneInbound = null;
        }

        public void Dock(DroneController droneController)
        {
            SetDockedDrone(droneController);
        }

        public Transform GetEntryPoint()
        {
            return _entryPoint == null ? null : _entryPoint?.transform;
        }

        public int GetPortID()
        {
            return _portID;
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
            if (GetCurrentOrder(out var order)) return;

            if (FindHubDepot(order, out var devices)) return;

            OffloadItemsInBase(order, devices);
            
            VoiceNotificationSystem.main.DisplayMessage($"{AlterraHub.OrderHasBeenShipped()} {GetBaseName()}");
        }

        private static void OffloadItemsInBase(Shipment order, IEnumerable<FcsDevice> devices)
        {
            var pendingItems = new List<TechType>();

            foreach (CartItemSaveData cartItem in order.CartItems)
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
                    pendingItems.Add(cartItem.ReceiveTechType);
                }
            }

            foreach (FcsDevice device in devices)
            {
                if (device == null || device is not AlterraHubDepotController depot) continue;

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

            StoreManager.main.CompleteOrder(order);
        }

        private bool FindHubDepot(Shipment order, out IEnumerable<FcsDevice> devices)
        {
            devices = Manager?.GetDevices(Mod.AlterraHubDepotTabID);

            if (devices == null)
            {
                QuickLogger.ModMessage($"Failed to find any Hub Depots giving a refund.");
                VoiceNotificationSystem.main.DisplayMessage($"Your order has been refunded. Please report to FCStudios");
                foreach (CartItemSaveData cartItem in order.CartItems)
                {
                    cartItem.Refund();
                }

                return true;
            }

            return false;
        }

        internal bool HasDepot()
        {
            return Manager?.GetDevices(Mod.AlterraHubDepotTabID)?.Any() ?? false;
        }

        private static bool GetCurrentOrder(out Shipment order)
        {
            order = null;
            var temp = StoreManager.main.PendingShipments.FirstOrDefault();

            if (temp?.CartItems == null)
            {
                QuickLogger.Error("Order was null when offloading.");
                return true;
            }

            order = temp;
            return false;
        }

        public override void Awake()
        {
            base.Awake();

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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                }

                _runStartUpOnEnable = false;
            }
        }

        #endregion

        public override void Initialize()
        {
            BaseTransform = transform;
            _stateHash = Animator.StringToHash("State");
            _animator = GetComponent<Animator>();

            _paths = GameObjectHelpers.FindGameObject(gameObject, "DronePort_DockingPaths").GetChildrenT().ToList();

            _entryPoint = _paths[0].gameObject;

            _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPoint");

            var doors = GameObjectHelpers.FindGameObjects(gameObject, "DockingPadDoor", SearchOption.StartsWith);

            var door1 = doors.ElementAt(0).AddComponent<PortDoorController>();
            door1.OpenPosX = -1.25f;
            _doors.Add(door1);

            var door2 = doors.ElementAt(1).AddComponent<PortDoorController>();
            door2.OpenPosX = 1.25f;
            _doors.Add(door2);

            CreateLadders();

            if (_portManager == null)
            {
                _portManager = gameObject.GetComponentInParent<PortManager>();
                _portManager.RegisterDronePort(this);

                var droneDoorTrigger = GameObjectHelpers.FindGameObject(gameObject, "DockTrigger").AddComponent<DroneDoorTrigger>();
                droneDoorTrigger.PortManager = _portManager;
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, AlterraHub.BaseDecalsEmissiveController);
            }

            var antenna = GameObjectHelpers.FindGameObject(gameObject, "AntennaMeshController")?.AddComponent<MotorHandler>();
            antenna?.Initialize(30);

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, ColorList.GetColor(14));
            IsInitialized = true;
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }



#if SUBNAUTICA_STABLE
        public DroneController SpawnDrone()
        {
            //TODO V2 Fix
            //var drone = Instantiate(CraftData.GetPrefabForTechType(Mod.AlterraTransportDroneTechType), _spawnPoint.transform.position, _spawnPoint.transform.rotation);
            //_assignedDrone = drone.GetComponent<DroneController>();
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
            if (!IsConstructed) return;
            var ladder = GameObjectHelpers.FindGameObject(gameObject, "Trigger_ladder01");
            if (ladder == null) return;
            var t01 = ladder.AddComponent<CinematicLadderController>();
            t01.Set(GameObjectHelpers.FindGameObject(gameObject, "lockingPoint").transform, "use_ladder", "rocketship_outLadder", 3);
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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
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
            QuickLogger.Debug($"In OnProtoSerialize - PortManager controller");

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

            if (_hasTransportInbound)
            {
                reason = "PortManager has a transport inbound please wait until delivery is complete";
                return false;
            }



            return true;
        }

        public bool HasRoomFor(List<Vector2int> sizes)
        {
            var devices = Manager.GetDevices(Mod.AlterraHubDepotTabID);

            //var totalsizeReq = sizes.Sum(x => x.x) + sizes.Sum(x => x.y);

            int total = 0;

            foreach (FcsDevice fcsDevice in devices)
            {
                if (fcsDevice == null || fcsDevice is not AlterraHubDepotController device) continue;
                total += device.GetFreeSlotsCount();
            }

            return total >= sizes.Count;
        }
        
        public string GetBaseID()
        {
            return _portManager.GetBaseID();
        }

        public void SetInboundDrone(DroneController droneController)
        {
            throw new NotImplementedException();
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
            base.OnHandHover(hand);
            var data = new string[] { };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {

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

        void IDroneDestination.ClearInbound()
        {
            ClearInbound();
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
    }
}