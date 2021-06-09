using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DroneController : MonoBehaviour
    {
        private AlterraFabricatorStationController _baseController;
        private List<Transform> _flightPath = new List<Transform>();
        private IEnumerable<GameObject> _airThrusters;
        private MotorHandler _waterTurbine;
        private Transform _trans;
        private DroneStates _droneState;
        private IDroneDestination _destination;
        private Dictionary<TechType, int> _order;
        private Transform _nextPos;
        private float speed = 5;
        private int _nextPosIndex;
        private string _id;
        private IDroneDestination _departurePort;
        private IDroneDestination _destinationPort;

        private enum DroneStates
        {
            Idle=0,
            Transitioning=1,
            Docked=2
        }

        internal bool IsNavigating()
        {
            return _droneState != DroneStates.Docked && _droneState != DroneStates.Idle;
        }

        private void Update()
        {
            if (_droneState != DroneStates.Idle || _droneState != DroneStates.Docked)
            {
                Move();
                UpdateEngine();
            }

            if (_nextPosIndex == _flightPath.Count - 1)
            {
                //We have arrived
                DockDrone();
            }

        }

        private void DockDrone()
        {
            _droneState = DroneStates.Docked;

            //Offload
            _destination.Offload(_order, OnOffloadCompleted);
        }

        public void OnOffloadCompleted()
        {
            if (_destination.BaseId != _baseController.BaseId)
            {
                //Depart
                ShipOrder(null, _destination, _departurePort);
                return;
            }

            _droneState = DroneStates.Idle;
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
            if (_trans.position == _nextPos.position)
            {
                _nextPosIndex++;

                if (_nextPosIndex >= _flightPath.Count)
                {
                    _nextPosIndex = 0;
                }

                _nextPos = _flightPath[_nextPosIndex];
            }
            else
            {
                _trans.position = Vector3.MoveTowards(_trans.position, _nextPos.position, speed * DayNightCycle.main.deltaTime);
            }
        }

        internal void Initialize(AlterraFabricatorStationController baseController)
        {
            _trans = gameObject.transform;
            _baseController = baseController;
            _airThrusters = GameObjectHelpers.FindGameObjects(gameObject, "airThrusters");
            
            _waterTurbine = GameObjectHelpers.FindGameObject(gameObject, "propeller").AddComponent<MotorHandler>();
            _waterTurbine.Initialize(0);

            _id = gameObject.GetComponent<PrefabIdentifier>().Id;

            //var stablizer = gameObject.AddComponent<Stabilizer>(); Not sure I need a stabilizer at this point.
        }

        private bool IsUnderwater()
        {
            return _trans.position.y < Ocean.main.GetOceanLevel();
        }

        internal void ShipOrder(Dictionary<TechType, int> order, IDroneDestination departurePort, IDroneDestination destinationPort)
        {
            _departurePort = departurePort;
            _destinationPort = destinationPort;
            _flightPath.AddRange(departurePort.GetPaths());
            _flightPath.AddRange(destinationPort.GetPaths());
            _destination = destinationPort;
            _order = order;
            _nextPos = _flightPath[0];
            _droneState = DroneStates.Transitioning;
        }

        internal Transform GetTargetWayPoint()
        {
            return _flightPath[_nextPosIndex];
        }

        internal IDroneDestination GetDestination()
        {
            return _destination;
        }

        internal string GetId()
        {
            return _id;
        }
    }
}
