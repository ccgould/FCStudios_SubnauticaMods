using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AlterraFabricatorStationController : MonoBehaviour
    {
        public static AlterraFabricatorStationController Main;
        private const int PowercellReq = 5;
        internal bool IsPowerOn => Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count >= PowercellReq && Mod.GamePlaySettings.BreakerOn;
        public Transform BaseTransform { get; set; }

        public void Offload(Dictionary<TechType, int> order, Action onOffloadCompleted)
        {
            onOffloadCompleted?.Invoke();
        }

        private Light[] _lights;
        private List<KeyPadAccessController> _keyPads = new();
        private List<SecurityScreenController> _screens = new();
        private GeneratorController _generator;
        private MotorHandler _motor;
        private AntennaController _antenna;

        private readonly GameObject[] _electList = new GameObject[6];
        private Dictionary<string, IDroneDestination> _ports = new();
        private Dictionary<AlterraDronePortController, List<CartItem>> _pendingPurchase = new();
        private HashSet<DroneController> _drones = new();
        private string _st;
        private PortManager _portManager;
        private string _baseID;

        private void Awake()
        {
            Main = this;
            Mod.OnGamePlaySettingsLoaded += OnGamePlaySettingsLoaded;
            BaseTransform = transform;
            var lodGroup = gameObject.GetComponentInChildren<LODGroup>();
            lodGroup.size = 4f;
        }
        
        private void Start()
        {
            

            CreatePorts();

            _generator = GameObjectHelpers.FindGameObject(gameObject, "Anim_Generator").AddComponent<GeneratorController>();
            _generator.Initialize(this);            
            
            _antenna = GameObjectHelpers.FindGameObject(gameObject, "antenna_controller").AddComponent<AntennaController>();
            _antenna.Initialize(this);
            _antenna.OnBoxFixedAction += index => {
                Destroy(_electList[index]);
            };

            var antennaDoor = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor02").AddComponent<DoorController>();
            antennaDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;

            var fabricatorDoor = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor01").AddComponent<DoorController>();
            fabricatorDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;

            var antennaDialPad = GameObjectHelpers.FindGameObject(gameObject, "AntennaDialpad").AddComponent<KeyPadAccessController>();
            antennaDialPad.Initialize("3491", antennaDoor,2);
            _keyPads.Add(antennaDialPad);
            var fabrictorDialPad = GameObjectHelpers.FindGameObject(gameObject, "FabricationDialpad").AddComponent<KeyPadAccessController>();
            fabrictorDialPad.Initialize("8964", fabricatorDoor,1);
            _keyPads.Add(fabrictorDialPad);

            _motor = GameObjectHelpers.FindGameObject(gameObject, "AlternatorMotor").AddComponent<MotorHandler>();
            _motor.Initialize(100);
            _motor.StopMotor();

            FindScreens();
            
            FindLights();

            //ToggleIsKinematic(); Disabled for dev use only

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 2f);
            MaterialHelpers.ChangeSpecSettings(AlterraHub.BaseDecalsExterior, AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);
            FindPortManager();
            InvokeRepeating(nameof(TryShip),1f,1f);
        }

        public IDroneDestination GetAvailablePort(DroneController approachingDrone)
        {
            var port = _ports.FirstOrDefault(x => !x.Value.HasDroneDocked()).Value;

            if (port != null)
            {
                port.SetDockedDrone(approachingDrone);
            }

            return port;
        }

        //For dev use only
        private void MoveToPlayer()
        {
            Mod.FCSStationSpawn.transform.parent.transform.position = Player.main.transform.position;
            Mod.FCSStationSpawn.transform.parent.transform.localRotation = Player.main.transform.localRotation;
        }

        //For dev use only
        private void ToggleIsKinematic()
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody.isKinematic = !rigidBody.isKinematic;
        }
        
        private void FindScreens()
        {
            var screens = GameObjectHelpers.FindGameObject(gameObject, "Screens").transform;
            foreach (Transform screen in screens)
            {
                var securityScreenController = screen.gameObject.AddComponent<SecurityScreenController>();
                _screens.Add(securityScreenController);
            }
        }

        private void FindLights()
        {
            _lights = gameObject.GetComponentsInChildren<Light>();
            foreach (Light light in _lights)
            {
                var tracker = light.gameObject.AddComponent<PlayerDistanceTracker>();

                var manager = light.gameObject.EnsureComponent<LightManager>();
                manager.DistanceTracker = tracker;
                manager.Light = light;
            }
        }
        
        private void OnGamePlaySettingsLoaded(FCSGamePlaySettings settings)
        {
            QuickLogger.Info($"On Game Play Settings Loaded {JsonConvert.SerializeObject(settings, Formatting.Indented)}");
            if (settings.AlterraHubDepotDoors.KeyPad1)
            {
                _keyPads[0].Unlock();
            }

            if (settings.AlterraHubDepotDoors.KeyPad2)
            {
                _keyPads[1].Unlock();
            }

            _generator.LoadSave();

            _antenna.LoadSave();

            if(IsPowerOn)
            {
                TurnOnBase();
            }
            Mod.GamePlaySettings.IsStationSpawned = true;
        }

        private void TurnOnLights()
        {
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.white);
            foreach (Light light in _lights)
            {
                light.color = Color.white;
            }
        }

        internal void AddPowercell(string slot)
        {
            if (IsPowerOn) return;
            Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Add(slot);
        }

        internal void TurnOnBase()
        {
            TurnOnLights();
            TurnOnScreens();
            TurnOnKeyPads();
            _motor.StartMotor();
            Mod.GamePlaySettings.BreakerOn = true;
        }

        private void TurnOnKeyPads()
        {
            foreach (KeyPadAccessController controller in _keyPads)
            {
                controller.TurnOn();
            }
        }

        private void TurnOnScreens()
        {
            foreach (SecurityScreenController screen in _screens)
            {
                screen.TurnOn();
            }
        }

        public void PendAPurchase(AlterraDronePortController port, CartItem cartItem)
        {
            if (_pendingPurchase.ContainsKey(port))
            {
                _pendingPurchase[port].Add(cartItem);
            }
            else
            {
                _pendingPurchase.Add(port, new List<CartItem>{ cartItem });
            }
        }

        internal void RegisterDrone(DroneController drone)
        {
            QuickLogger.Debug($"Registered Drone: {drone.GetId()}");
            _drones.Add(drone);
        }

        internal IDroneDestination GetAssignedPort(string prefabID)
        {
            return Mod.GamePlaySettings.DronePortAssigns.ContainsKey(prefabID) ? _ports.ElementAt(Mod.GamePlaySettings.DronePortAssigns[prefabID]).Value : null;
        }

        public void TryShip()
        {
            if (_drones.Count < 3)
            {
                if (!Mod.GamePlaySettings.TransDroneSpawned)
                {
                    foreach (KeyValuePair<string, IDroneDestination> port in _ports)
                    {
                        var drone = port.Value.SpawnDrone();
                        _drones.Add(drone);
                        //Mod.GamePlaySettings.DronePortAssigns.Add(drone.GetId(), port.Value.GetPortID());
                    }
                    //transform.parent = _drones.ElementAt(0)?.transform.parent;
                    Mod.GamePlaySettings.TransDroneSpawned = true;
                }
            }

            if (!LargeWorldStreamer.main.IsWorldSettled() || _pendingPurchase.Count <= 0) return;
            
            for (int i = _pendingPurchase.Count - 1; i >= 0; i--)
            {
                foreach (DroneController drone in _drones)
                {
                    var purchase = _pendingPurchase.ElementAt(i);
                    
                    if(purchase.Key.HasIncomingFlight() || purchase.Key.HasDroneDocked()) continue;

                    if (drone.GetDestinationID() == purchase.Key.UnitID) continue;

                    var state = drone.GetState();

                    if (state is DroneController.DroneStates.Docked or DroneController.DroneStates.None)
                    {
                        if (drone.ShipOrder(purchase.Value, drone.GetPort(), purchase.Key))
                        {
                            _pendingPurchase.Remove(purchase.Key);
                        }
                    }

                    if (_pendingPurchase.Count <= 0) break;
                }
            }
        }

        internal void ResetDrones()
        {
            foreach (DroneController controller in _drones)
            {
                if (controller != null)
                {
                    if (controller.IsNavigating())
                    {
                        controller.RefundShipment();
                    }
                    DestroyImmediate(controller.gameObject);
                }
            }

            _drones.Clear();

            foreach (KeyValuePair<string, IDroneDestination> dronePortController in _ports)
            {
                dronePortController.Value.SpawnDrone();
            }
        }


        public IEnumerable<AlterraTransportDroneEntry> SaveDrones()
        {
            foreach (DroneController drone in _drones)
            {
                yield return drone.Save();
            }
        }

        public bool IsStationPort(IDroneDestination dockedPort)
        {
            foreach (var port in _ports)
            {
                if (port.Value.GetPrefabID().Equals(dockedPort.GetPrefabID()))
                {
                    return true;
                }
            }
            return false;
        }

        public IDroneDestination FindPort(int port)
        {
            if (_portManager == null)
            {
                FindPortManager();
            }

            CreatePorts();

            return _portManager.FindPort(port);
        }

        private void CreatePorts()
        {

            if (_ports?.Count > 0) return;
                var ports = GameObjectHelpers.FindGameObjects(gameObject, "DronePortPad_HubWreck", SearchOption.StartsWith).ToArray();
            for (int i = 0; i < ports.Length; i++)
            {
                var portController = ports.ElementAt(i).AddComponent<AlterraDronePortController>();
                portController.SetPortID(i);
                _ports.Add($"Port_{i}", portController);
                portController.Initialize();
            }
        }

        private void FindPortManager()
        {
            if (_portManager == null)
            {
                _portManager = gameObject.GetComponent<PortManager>();
            }
        }

        public string GetPrefabId()
        {
            if (string.IsNullOrWhiteSpace(_baseID))
            {
                _baseID = gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }



            return _baseID;
        }
    }
}
