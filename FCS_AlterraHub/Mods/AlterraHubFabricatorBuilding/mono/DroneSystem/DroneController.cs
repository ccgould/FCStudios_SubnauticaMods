using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Enums;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Factory;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DroneController : MonoBehaviour
    {
        [SerializeField]
        private AlterraDronePortController departurePort;

        [SerializeField]
        private AlterraDronePortController destinationPort;

        private Transform _trans;
        private StateMachine _stateMachine;
        private PingInstance _beacon;
        private IEnumerable<GameObject> _airThrusters;
        private bool _isDocking;
        private bool _isDeparting;
        private bool _dataLoaded;
        public bool IsInitialize { get; set; }
        public StateMachine StateMachine => GetStateMachine();

        private void UpdateBeacon()
        {

            if (_beacon != null)
            {
                var result = IsTransporting();
                _beacon.SetVisible(result);
                _beacon.enabled = result;
            }
        }

        internal void Initialize()
        {
            if (IsInitialize) return;
            _airThrusters = GameObjectHelpers.FindGameObjects(gameObject, "Thruster", SearchOption.StartsWith);
            var bubbles = GameObjectHelpers.FindGameObject(gameObject, "xSeamothTrail").EnsureComponent<ThrusterController>();
            bubbles.isWaterSensitive = true;
            foreach (GameObject thruster in _airThrusters)
            {
                thruster.EnsureComponent<ThrusterController>();
            }

            var lodGroup = gameObject.GetComponentInChildren<LODGroup>();
            lodGroup.size = 4f;
            _beacon = WorldHelpers.CreateBeacon(gameObject, Mod.AlterraTransportDronePingType, $"Transport Drone", false);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialize = true;
        }
        

        private void Awake()
        {
            _stateMachine = GetComponent<StateMachine>();
            Initialize();
            InitializeStateMachine();
            _trans = gameObject.transform;
            InvokeRepeating(nameof(UpdateBeacon),1f,1f);
        }

        private StateMachine GetStateMachine()
        {
            if (_stateMachine == null)
            {
                _stateMachine = GetComponent<StateMachine>();
            }

            return _stateMachine;
        }

        private void InitializeStateMachine()
        {
            var states = new Dictionary<Type, BaseState>()
        {
            {typeof(IdleState), new IdleState(this)},
            {typeof(TransportState), new TransportState(this)},
            {typeof(DockingState), new DockingState(this)},
            {typeof(DepartState), new DepartState(this)},
            {typeof(AlignState), new AlignState(this)},
        };

            gameObject.EnsureComponent<StateMachine>().SetStates(states);
        }


        internal bool ShipOrder(AlterraDronePortController destinationPort)
        {
            try
            {
                if (destinationPort == null)
                {
                    QuickLogger.Error("Destination Port returned null", true);
                    return false;
                }

                StartCoroutine(ShipOrderAsync(destinationPort));
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }
        }

        private IEnumerator ShipOrderAsync(AlterraDronePortController destinationPort)
        {
            yield return new WaitForSeconds(5f);
            QuickLogger.Debug("Trying to ship");
            this.destinationPort = destinationPort;
            Depart();
            yield break;
        }

        public void Dock()
        {
            if (_isDocking) return;

            QuickLogger.Debug("Docking", true);

            if(destinationPort == null) return;

            destinationPort.PlayAnimationState(DronePortAnimation.Docking, () =>
            {
                QuickLogger.Debug("Setting State To Idle",true);
                
                destinationPort.Offload(this);

                departurePort = destinationPort;

                StateMachine.SwitchToNewState(typeof(IdleState));

                _isDocking = false;
            });

            _isDocking = true;
        }

        public void Depart()
        {
            if (_isDeparting) return;

            QuickLogger.Debug("Departing", true);

            _isDeparting = true;

            departurePort.Depart(this);

            destinationPort.Dock(this);

            StateMachine.SwitchToNewState(typeof(DepartState));

            departurePort.PlayAnimationState(DronePortAnimation.Departing, (() =>
            {
                QuickLogger.Debug("Begining Transport.",true);
                StateMachine.SwitchToNewState(typeof(TransportState));
                _isDeparting = false;
            }));
        }

        public Transform GetTransform()
        {
            return _trans;
        }

        public AlterraDronePortController GetTargetPort()
        {
            if (destinationPort == null)
            {
                destinationPort = AlterraFabricatorStationController.Main.GetOpenPort();
            }
            return destinationPort;
        }

        public AlterraDronePortController GetCurrentPort()
        {
            return departurePort;
        }

        public AlterraTransportDroneEntry Save()
        {
            return new()
            {
                Position = transform.position.ToVec3(),
                Rotation = transform.rotation.QuaternionToVec4(),
                State = StateMachine.CurrentState.Name,
                DestinationBaseID = destinationPort?.GetBaseID(),
                DestinationPortID = destinationPort?.GetPortID() ?? 0,
                DeparturePortID = departurePort?.GetPortID() ?? 0,
                DepartureBaseID = departurePort?.GetBaseID()
            };
        }

        public BaseState GetState()
        {
            return StateMachine.CurrentState;
        }

        internal bool AvailableForTransport()
        {
            return !IsTransporting() && AlterraFabricatorStationController.Main.IsStationPort(GetCurrentPort());
        }

        private bool IsTransporting()
        {
            var currentType = GetState()?.GetType();

            if (currentType == null) return true; // returning true to prevent the transport from being used while not ready.

            return currentType != typeof(IdleState);
        }


        public void SetDeparturePort(AlterraDronePortController port)
        {
            departurePort = port;
        }

        public void LoadData()
        {
            if (_dataLoaded) return;

            if (Mod.GetSaveData().AlterraTransportDroneEntries.Any())
            {
                var data = Mod.GetSaveData().AlterraTransportDroneEntries[0];

                BaseManager.FindManager(data.DestinationBaseID, result =>
                {
                    //TODO Gets stuck in a loop when cant find the base best to stop checking after a certain amount of time
                    destinationPort = result.GetPortManager().GetOpenPort();
                });

                var state = StateFactory.GetState(data.State);

                StateMachine.SwitchToNewState(state.GetType());
            }
            _dataLoaded = true;
        }
    }
}