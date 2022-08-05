using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Enums;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Factory;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

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
                _beacon.enabled = result;
                _beacon.SetVisible(result);
#if SUBNAUTICA_STABLE
                       PingManager.NotifyVisible(_beacon);          
#endif

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
            _beacon = gameObject.GetComponent<PingInstance>();
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialize = true;
        }
        

        private void Awake()
        {
            _stateMachine = GetComponent<StateMachine>();
            Initialize();
            InitializeStateMachine();
            _trans = gameObject.transform;
            AlterraFabricatorStationController._drones.Add(this);
            //InvokeRepeating(nameof(UpdateBeacon),1f,1f);
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
            {typeof(ClimbState), new ClimbState(this)},
            {typeof(DescendState), new DescendState(this)},
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
            this.destinationPort = destinationPort;
            yield return new WaitForSeconds(5f);
            QuickLogger.Debug("Trying to ship");
            destinationPort.SetInboundDrone(this);
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

                UpdateBeacon();
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
                departurePort.ClearInbound();
                StateMachine.SwitchToNewState(typeof(ClimbState));
                _isDeparting = false;

                UpdateBeacon();

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
            QuickLogger.Debug("Loading Drone Data",true);
            if (_dataLoaded) return;
            QuickLogger.Debug("Drone Data Check", true);
            if (Mod.GetSaveData().AlterraTransportDroneEntries != null && Mod.GetSaveData().AlterraTransportDroneEntries.Any())
            {
                var data = Mod.GetSaveData().AlterraTransportDroneEntries[0];
                QuickLogger.Debug($"Data found: {data?.DestinationBaseID}", true);

                if (!string.IsNullOrWhiteSpace(data.DestinationBaseID))
                {
                    QuickLogger.Debug("Destination Port not null", true);

                    if (!AlterraFabricatorStationController.Main.IsStationBaseID(data.DestinationBaseID))
                    {
                        BaseManager.FindManager(data.DestinationBaseID, result =>
                        {
                            destinationPort = result.GetPortManager().GetOpenPort();
                            QuickLogger.Debug($"Found Destination: {destinationPort?.GetPrefabID()}", true);

                            var state = StateFactory.GetState(data.State);
                            QuickLogger.Debug($"Setting State: {state.Name}", true);

                            if (state.GetType() == typeof(IdleState) && destinationPort != null && !AlterraFabricatorStationController.Main.IsStationPort(destinationPort))
                            {
                                ShipOrder(destinationPort);
                            }
                            else
                            {
                                StateMachine.SwitchToNewState(state.GetType());
                            }

                        });
                    }
                    else
                    {
                        StateMachine.SwitchToNewState(typeof(IdleState));
                    }


                    //destinationPort?.SetInboundDrone(this);
                }

                _trans.position = data.Position.ToVector3();
                _trans.rotation = data.Rotation.Vec4ToQuaternion();
            }
            _dataLoaded = true;
        }

        public float GetCompletionPercentage()
        {
            if (destinationPort?.BaseTransform == null || destinationPort?.BaseTransform == null) return 0f;
            return GetPercentageAlong(departurePort.BaseTransform.position, destinationPort.BaseTransform.position,
                transform.position);
        }

        public static float GetPercentageAlong(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            return Vector3.Dot(ac, ab) / ab.sqrMagnitude;
        }
    }
}