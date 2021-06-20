using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DroneController : MonoBehaviour, IProtoTreeEventListener
    {
        private IEnumerable<GameObject> _airThrusters;
        private Transform _trans;
        private DroneStates _droneState = DroneStates.None;
        private List<CartItemSaveData> _order;
        private Transform _nextPos;
        private float speed = 5;
        private const float maxSpeed = 35f;
        private const float minSpeed = 5f;

        private int _nextPosIndex;
        private string _id;
        private IDroneDestination _destinationPort;
        private IDroneDestination _departurePort;
        private IDroneDestination _dockedPort;
        private bool _alignDrone;
        private AlterraTransportDroneEntry _saveData;
        private bool _isDocking;
        private bool _isDeparting;
        private PingInstance _beacon;
        private string _storeInformationIdentifier;
        private Transform _parent;
        private bool _isReturningToStation;
        private bool _fromSave;
        private bool _runOnEnabled = true;

        internal enum DroneStates
        {
            None=-1,
            Docking=0,
            Transitioning=1,
            Docked=2,
            Departing=3,
        }

        private void Awake()
        {
            _trans = transform;
        }

        private void OnEnable()
        {
            if (_runOnEnabled)
            {
                if (_fromSave)
                {
                    _saveData = Mod.GetAlterraTransportDroneSaveData(GetId());

                    if (_saveData != null)
                    {
                        QuickLogger.Debug("Loading Drone Save");
                        if (_saveData.Cargo != null)
                        {
                            _order = _saveData.Cargo.ToList();
                        }

                        _droneState = _saveData.DroneState;

                        StartCoroutine(AttemptInitialize());
                    }
                }

                _runOnEnabled = false;
            }
        }

        private IEnumerator AttemptInitialize()
        {
            while (AlterraFabricatorStationController.Main ==null)
            {
                yield return null;
            }
            var dockedBase = BaseManager.FindManager(_saveData.DockedPortBaseID);
            QuickLogger.Debug("1");

            Initialize(dockedBase?.GetPortManager()?.FindPort(_saveData.DockedPortID));
            QuickLogger.Debug("2");

            var destinationBase = BaseManager.FindManager(_saveData.DestinationBaseID);
            QuickLogger.Debug("3");

            if (destinationBase == null)
            {
                QuickLogger.Debug($"Checking if Station: {AlterraFabricatorStationController.Main.GetPrefabId()} / {_saveData.DestinationBaseID} = {AlterraFabricatorStationController.Main.GetPrefabId().Equals(_saveData.DestinationBaseID)}");
                if (AlterraFabricatorStationController.Main.GetPrefabId().Equals(_saveData.DestinationBaseID))
                {
                    QuickLogger.Debug("4");

                    _destinationPort = AlterraFabricatorStationController.Main.FindPort(_saveData.DestinationPortID);
                    QuickLogger.Debug("5");

                }
            }
            else
            {
                QuickLogger.Debug("6");

                _destinationPort = destinationBase?.GetPortManager()?.FindPort(_saveData.DestinationPortID);
                QuickLogger.Debug("7");

            }
            QuickLogger.Debug("8");

            var departureBase = BaseManager.FindManager(_saveData.DepartureBaseID);
            QuickLogger.Debug("9");

            _departurePort = departureBase?.GetPortManager()?.FindPort(_saveData.DeparturePortID);
            QuickLogger.Debug("10");

            _nextPos = _destinationPort?.GetEntryPoint();
            QuickLogger.Debug("11");
            yield break;
        }


        private void Start()
        {
            AlterraFabricatorStationController.Main.RegisterDrone(this);
            _parent = _trans.parent;
            _storeInformationIdentifier = _trans.parent.GetComponentInParent<StoreInformationIdentifier>()?.Id;
            InvokeRepeating(nameof(CanReturn),1f,1f);
        }

        //private void TryFindBase()
        //{
        //    if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DockedPortID) && _dockedPort == null)
        //    {
        //        var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DockedPortID);

        //        if (port.Value == null) return;
        //        _dockedPort = (IDroneDestination)port.Value;
        //        CancelInvoke(nameof(TryFindBase));
        //    }
        //    else
        //    {
        //        CancelInvoke(nameof(TryFindBase));
        //    }
        //}
        
        //private void TryFindDeparturePort()
        //{
        //    if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DeparturePortID) && _departurePort == null)
        //    {
        //        var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DeparturePortID);

        //        if (port.Value == null) return;
        //        _departurePort = (IDroneDestination)port.Value;
        //        CancelInvoke(nameof(TryFindDeparturePort));
        //    }
        //    else
        //    {
        //        CancelInvoke(nameof(TryFindDeparturePort));
        //    }
        //}

        //private void TryFindDestinationPort()
        //{
        //    if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DestinationPortID) && _destinationPort == null)
        //    {
        //        var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DestinationPortID);

        //        if (port.Value == null) return;
        //        _destinationPort = (IDroneDestination)port.Value;
        //        CancelInvoke(nameof(TryFindDestinationPort));
                
        //        if (_droneState == DroneStates.Docking)
        //        {
        //            IsDocking(true);
        //            _destinationPort.DockDrone(this);
        //            DockDrone();
        //        }
        //    }
        //    else
        //    {
        //        CancelInvoke(nameof(TryFindDestinationPort));
        //        StartCoroutine(ReturnToStation());
        //    }
        //}

        internal bool IsNavigating()
        {
            return _droneState == DroneStates.Transitioning;
        }

        private void Update()
        {
            if (_destinationPort != null && _departurePort != null)
            {
                float destDistance = Vector3.Distance(_trans.position, _destinationPort.GetEntryPoint().position);
                float depDistance = Vector3.Distance(_trans.position, _departurePort.GetEntryPoint().position);
                speed = destDistance > 20 && depDistance > 20 ? maxSpeed : minSpeed;
            }

            UpdateDockState();
            
            if (_droneState == DroneStates.Transitioning)
            {
                if (_alignDrone)
                {
                    AlignDrone();
                }
                else
                {
                    Move();
                }

            }

            if(_beacon != null)
            {
                _beacon.SetVisible(_droneState == DroneStates.Transitioning);
                _beacon.enabled = _droneState == DroneStates.Transitioning;
            }
        }

        private void UpdateDockState()
        {
            if (_isDocking)
            {
                _droneState = DroneStates.Docking;
            }
            else if(_isDeparting)
            {
                _droneState = DroneStates.Departing;
            }
            else
            {
                _droneState = _dockedPort != null ? DroneStates.Docked : DroneStates.Transitioning;
            }
        }

        internal void IsDocking(bool value)
        {
            _isDocking = value;
            if (value && !_isReturningToStation)
            {
                VoiceNotificationSystem.main.DisplayMessage($"Drone arrived at {_destinationPort.GetBaseName()}");
            }
        }

        internal void IsDeparting(bool value)
        {
            _isDeparting = value;
        }

        private void DockDrone()
        {
            _destinationPort = null;
            _destinationPort = null;
            _dockedPort = GetComponentInParent<IDroneDestination>();
            _isReturningToStation = false;
        }

        internal void UnDock()
        {
            _dockedPort = null;
            _trans.parent = _parent;
            QuickLogger.Debug("Drone Undocked",true);
        }

        public void OnOffloadCompleted()
        {
            _order = null;
            _departurePort = null;
            _destinationPort = null;
            if(!_isReturningToStation)
                VoiceNotificationSystem.main.DisplayMessage($"Order dropped off at: {_dockedPort.GetBaseName()}");
        }

        private void Move()
        {

            if (_nextPos == null && _destinationPort != null)
            {
                _nextPos = _destinationPort.GetEntryPoint();
            }


            if (_trans == null || _nextPos == null) return;

            if (_trans.position == _nextPos.position)
            {
                _alignDrone = true;
                _trans.parent = _nextPos.transform;
                return;
            }

            _trans.position = Vector3.MoveTowards(_trans.position, _nextPos.position, speed * Time.deltaTime);
            var rotation = Quaternion.LookRotation(_nextPos.position - _trans.position);
            _trans.rotation = Quaternion.Slerp(_trans.rotation, rotation, 1 * Time.deltaTime);
        }

        internal void Initialize(IDroneDestination dockedPort)
        {
            if (IsInitialize) return;
            _trans = gameObject.transform;
            _airThrusters = GameObjectHelpers.FindGameObjects(gameObject, "Thruster",SearchOption.StartsWith);
            var bubbles = GameObjectHelpers.FindGameObject(gameObject, "xSeamothTrail").EnsureComponent<ThrusterController>();
            bubbles.isWaterSensitive = true;
            foreach (GameObject thruster in _airThrusters)
            {
                thruster.EnsureComponent<ThrusterController>();
            }
            DockDrone();
            var lodGroup = gameObject.GetComponentInChildren<LODGroup>();
            lodGroup.size = 4f;
            _beacon =  WorldHelpers.CreateBeacon(gameObject, Mod.AlterraTransportDronePingType, $"Transport Drone - {_dockedPort?.GetPortID()}",false);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialize = true;
        }
        
        public bool IsInitialize { get; set; }

        private void AlignDrone()
        {
            var rot = Quaternion.LookRotation(_nextPos.forward, Vector3.up);
            _trans.rotation = Quaternion.Slerp(_trans.rotation, rot, 1f * Time.deltaTime);

            //Thanks to https://forum.unity.com/threads/checking-if-rotation-is-complete.515058/ user McDev02
            if (Quaternion.Angle(transform.rotation, rot) <= 0.01f)
            {
                _alignDrone = false;
                IsDocking(true);
                DockDrone();
            }
        }

        internal bool ShipOrder(List<CartItem> order, IDroneDestination departurePort, IDroneDestination destinationPort)
        {
            QuickLogger.Debug("Trying to ship");
            
            if (_order?.Count > 0)
            {
                QuickLogger.Error("Drone already has order. Request denied");
                return false;
            }

            QuickLogger.Debug($"DeparturePort: {departurePort} | DestinationPort: {destinationPort}",true);

            if (departurePort == null)
            {
                QuickLogger.Error("Departure Port location is null canceling shipping order");
                return false;
            }

            if (destinationPort == null)
            {
                QuickLogger.Error("Destination Port location is null canceling shipping order");
                return false;
            }
            
            _destinationPort = destinationPort;
            _departurePort = departurePort;
            _order = order?.ToCartItemSaveData().ToList();
            
            StartCoroutine(StartFlight());

            return true;
        }

        internal IDroneDestination GetDestination()
        {
            return _destinationPort;
        }

        internal string GetId()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = _id = gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }
            return _id;
        }

        internal DroneStates GetState()
        {
            return _droneState;
        }

        private IEnumerator StartFlight()
        {
            _dockedPort = _departurePort;
            _departurePort.OpenDoors();
            yield return new WaitForSeconds(5);
            IsDeparting(true);
            _destinationPort.SetIncomingFlight(true);
            _departurePort.Depart(this);

            while (_droneState != DroneStates.Transitioning)
            {
                yield return null;
            }

            if(!_isReturningToStation)
                VoiceNotificationSystem.main.DisplayMessage($"Your order is being delivered to base {_destinationPort.GetBaseName()}");
            _nextPos = _destinationPort.GetEntryPoint();
            yield break;
        }

        public string GetDestinationID()
        {
            return _destinationPort?.BaseId ?? String.Empty;
        }

        public IDroneDestination GetPort()
        {
            return _dockedPort;
        }

        internal void SetPort(IDroneDestination port)
        {
            _dockedPort = port;
        }

        internal AlterraTransportDroneEntry Save()
        {
            return new()
            {
                Cargo = _order,
                DockedPortID = _dockedPort?.GetPortID() ?? -1,
                DockedPortBaseID = _dockedPort?.GetBaseID(),
                DestinationBaseID = _destinationPort?.GetBaseID(),
                DepartureBaseID = _departurePort?.GetBaseID(),
                DeparturePortID = _departurePort?.GetPortID() ?? -1,
                DestinationPortID = _destinationPort?.GetPortID() ?? -1,
                DroneState = _droneState,
                Id = GetId(),
                ParentID = _storeInformationIdentifier
            };
        }

        private void CanReturn()
        {
            if (_order == null && !_isReturningToStation && _droneState == DroneStates.Docked)
            {
                if (!AlterraFabricatorStationController.Main.IsStationPort(_dockedPort))
                {
                    _isReturningToStation = true;
                    StartCoroutine(ReturnToStation());
                }
            }
        }

        private IEnumerator ReturnToStation()
        {
            yield return new WaitForSeconds(5);

            while (_destinationPort == null)
            {
                yield return new WaitForSeconds(2);
                _destinationPort ??= AlterraFabricatorStationController.Main.GetAvailablePort(this);
                yield return null;
            }

            ShipOrder(null, _dockedPort, _destinationPort);
            yield break;
        }

        public IEnumerable<CartItemSaveData> GetOrder()
        {
            return _order;
        }

        public void RefundShipment()
        {
            if (_order != null)
            {
                foreach (CartItemSaveData cartItem in _order)
                {
                    cartItem.Refund();
                }
            }
        }

        public void LinkToPort()
        {
            throw new NotImplementedException();
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }
    }
}
