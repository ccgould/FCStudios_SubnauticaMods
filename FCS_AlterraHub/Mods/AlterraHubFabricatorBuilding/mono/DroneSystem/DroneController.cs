﻿using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DroneController : MonoBehaviour
    {
        private IDroneDestination _baseController;
        private List<Transform> _flightPath = new();
        private IEnumerable<GameObject> _airThrusters;
        private MotorHandler _waterTurbine;
        private Transform _trans;
        private DroneStates _droneState;
        private List<CartItem> _order;
        private Transform _nextPos;
        private float speed = 5;
        private int _nextPosIndex;
        private string _id;
        private IDroneDestination _destinationPort;
        private IDroneDestination _departurePort;
        private IDroneDestination _dockedPort;
        private bool _alignDrone;
        //private Animator _animator;
        private int State;
        private AlterraTransportDroneEntry _saveData;
        private bool _isDocking;
        private bool _returningToStation;

        internal enum DroneStates
        {
            Docking=0,
            Transitioning=1,
            Docked=2,
            Departing=3,
        }

        private void Awake()
        {
            _trans = transform;
            //_animator = GetComponent<Animator>();
            State = Animator.StringToHash("State");
        }

        private void Start()
        {
            AlterraFabricatorStationController.Main.RegisterDrone(this);
            
            _saveData = Mod.GetAlterraTransportDroneSaveData(GetId());

            if(_saveData?.Cargo != null)
                _order = _saveData.Cargo;

            InvokeRepeating(nameof(TryFindBase),1f,1f);
            InvokeRepeating(nameof(TryFindDeparturePort),1f,1f);
            InvokeRepeating(nameof(TryFindDestinationPort),1f,1f);
            InvokeRepeating(nameof(CanReturn),1f,1f);
        }

        private void TryFindBase()
        {
            if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DockedPortID) && _dockedPort == null)
            {
                var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DockedPortID);

                if (port.Value == null) return;
                _dockedPort = (IDroneDestination)port.Value;
                CancelInvoke(nameof(TryFindBase));
            }
            else
            {
                CancelInvoke(nameof(TryFindBase));
            }
        }

        private void TryFindDeparturePort()
        {
            if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DeparturePortID) && _departurePort == null)
            {
                var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DeparturePortID);

                if (port.Value == null) return;
                _departurePort = (IDroneDestination)port.Value;
                CancelInvoke(nameof(TryFindDeparturePort));
            }
            else
            {
                CancelInvoke(nameof(TryFindDeparturePort));
            }
        }

        private void TryFindDestinationPort()
        {
            if (_saveData != null && !string.IsNullOrWhiteSpace(_saveData.DestinationPortID) && _destinationPort == null)
            {
                var port = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(_saveData.DestinationPortID);

                if (port.Value == null) return;
                _destinationPort = (IDroneDestination)port.Value;
                CancelInvoke(nameof(TryFindDestinationPort));
                
                if (_droneState == DroneStates.Docking)
                {
                    IsDocking(true);
                    _destinationPort.DockDrone(this);
                    DockDrone();
                }
            }
            else
            {
                CancelInvoke(nameof(TryFindDestinationPort));
            }
        }

        internal bool IsNavigating()
        {
            return _droneState != DroneStates.Docked && _droneState != DroneStates.Docking;
        }

        private void Update()
        {
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

                //UpdateEngine();
            }

            //if (_nextPosIndex == _flightPath.Count - 1)
            //{
            //    //We have arrived
            //    DockDrone();
            //}

        }

        private void UpdateDockState()
        {

            if (_isDocking)
            {
                _droneState = DroneStates.Docking;
            }
            else
            {
                _droneState = _dockedPort != null ? DroneStates.Docked : DroneStates.Transitioning;
            }
        }

        internal void IsDocking(bool value)
        {
            _isDocking = value;
        }

        private void DockDrone()
        {
            _dockedPort = GetComponentInParent<IDroneDestination>();
            _flightPath.Clear();
            _returningToStation = false;
        }

        internal void UnDock()
        {
            _dockedPort = null;
            QuickLogger.Debug("Drone Undocked",true);
        }

        public void OnOffloadCompleted()
        {
            QuickLogger.ModMessage($"Order Dropped off at: {_dockedPort.GetBaseName()}");
            _order = null;
            _departurePort = null;
            _destinationPort = null;
        }

        private void UpdateEngine()
        {
            if (IsUnderwater())
            {
                _waterTurbine.RPMByPass(300);
                _waterTurbine.StartMotor();
            }
            else
            {
                _waterTurbine.StopMotor();
            }
        }

        private void Move()
        {
            if (_trans == null || _flightPath == null || _nextPos == null) return;

            if (_trans.position == _nextPos.position)
            {
                //_nextPosIndex++;
                _alignDrone = true;
                _trans.parent = _nextPos.transform;
                return;
                //if (_nextPosIndex >= _flightPath.Count)
                //{
                //    _nextPosIndex = -1;
                //    _alignDrone = true;
                //}

                //if (_nextPosIndex != -1)
                //    _nextPos = _flightPath[_nextPosIndex];
            }

            _trans.position = Vector3.MoveTowards(_trans.position, _nextPos.position, speed * Time.deltaTime);
            var rotation = Quaternion.LookRotation(_nextPos.position - _trans.position);
            _trans.rotation = Quaternion.Slerp(_trans.rotation, rotation, 1 * Time.deltaTime);
        }

        internal void Initialize(IDroneDestination baseController)
        {
            _trans = gameObject.transform;
            _baseController = baseController;
            _airThrusters = GameObjectHelpers.FindGameObjects(gameObject, "Thruster",SearchOption.StartsWith);
            foreach (GameObject thruster in _airThrusters)
            {
                thruster.EnsureComponent<AirThrusterController>();
            }
            _dockedPort = baseController;
            var lodGroup = gameObject.GetComponentInChildren<LODGroup>();
            lodGroup.size = 1.2f;
            //_waterTurbine = GameObjectHelpers.FindGameObject(gameObject, "propeller").AddComponent<MotorHandler>();
            //_waterTurbine.Initialize(0);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            //var stablizer = gameObject.AddComponent<Stabilizer>(); Not sure I need a stabilizer at this point.
        }

        private bool IsUnderwater()
        {
            return _trans.position.y < Ocean.main.GetOceanLevel();
        }

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
            _order = order;
            
            StartCoroutine(StartFlight());

            return true;
        }

        private void Reverse()
        {
            _flightPath.Reverse();
        }

        internal Transform GetTargetWayPoint()
        {
            return _flightPath[_nextPosIndex];
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

        //public void SetAnimator(bool value)
        //{
        //    _animator.enabled = value;
        //}
        
        internal DroneStates GetState()
        {
            return _droneState;
        }

        private IEnumerator StartFlight()
        {
            _dockedPort = _departurePort;
            _departurePort.OpenDoors();
            yield return new WaitForSeconds(5);
            _departurePort.Depart(this);

            while (_droneState != DroneStates.Transitioning)
            {
                yield return null;
            }

            _flightPath.AddRange(_destinationPort.GetPaths());
            _nextPos = _flightPath[0];
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
            return new AlterraTransportDroneEntry
            {
                Cargo = _order,
                DockedPortID = _dockedPort.BaseId,
                DestinationPortID = _destinationPort.BaseId,
                DeparturePortID = _departurePort.BaseId,
                DroneState = _droneState,
                Id = GetId()
            };
        }

        private void CanReturn()
        {
            if (_order == null && _dockedPort != _baseController && !_returningToStation && _droneState == DroneStates.Docked)
            {
                _returningToStation = true;
                StartCoroutine(ReturnToStation());
            }
        }

        private IEnumerator ReturnToStation()
        {
            QuickLogger.Debug($"Returning {GetId()} to Station",true);
            yield return new WaitForSeconds(5);
            ShipOrder(_order, _dockedPort, _baseController);
            
        }

        public List<CartItem> GetOrder()
        {
            return _order;
        }
    }
}
