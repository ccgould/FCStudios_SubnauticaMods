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
using Story;
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
        private Transform _warpTrans;
        private int MAXDRONECOUNT = 1;

        private void Awake()
        {
            Main = this;
            _warpTrans = GameObjectHelpers.FindGameObject(gameObject, "RespawnPoint").transform;
            //Mod.OnGamePlaySettingsLoaded += OnGamePlaySettingsLoaded;
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

            var bp1Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt1_");
            var bp2Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt2_");
            var bp3Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt3_");

            var dataBox1 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp1Pos.transform.position, bp1Pos.transform.rotation, true);
            var dataBox1C = dataBox1.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox1C, dataBox1, Mod.AlterraHubDepotTechType);

            var dataBox2 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp2Pos.transform.position, bp2Pos.transform.rotation, true);
            var dataBox2C = dataBox2.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox2C, dataBox2, Mod.OreConsumerTechType);

            var dataBox3 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp3Pos.transform.position, bp3Pos.transform.rotation, true);
            var dataBox3C = dataBox3.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox3C, dataBox3, Mod.DronePortPadHubNewTechType);

            OnGamePlaySettingsLoaded(Mod.GamePlaySettings);

            InvokeRepeating(nameof(TryShip),1f,1f);
        }

        private static void CreateBluePrintHandTarget(BlueprintHandTarget dataBox, GameObject go, TechType unlockTechType)
        {
            dataBox.animator = go.GetComponent<Animator>();
            dataBox.animParam = "databox_take";
            dataBox.viewAnimParam = "databox_lookat";
            dataBox.unlockTechType = unlockTechType;
            dataBox.useSound = QPatch.BoxOpenSoundAsset;
            dataBox.disableGameObject = GameObjectHelpers.FindGameObject(go, "BLUEPRINT_DATA_DISC");
            dataBox.inspectPrefab = AlterraHub.BluePrintDataDiscPrefab;
            dataBox.onUseGoal = new StoryGoal(string.Empty, Story.GoalType.PDA, 0);
            dataBox.unlockTechType = unlockTechType;

            var genericHandTarget = go.GetComponent<GenericHandTarget>();
            genericHandTarget.onHandHover.AddListener(_ => dataBox.HoverBlueprint());
            genericHandTarget.onHandClick.AddListener(_ => dataBox.UnlockBlueprint());
            dataBox.Start();
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

            if (_keyPads == null || _generator == null || _antenna == null) return;

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
            SpawnMissingDrones();

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

        private void SpawnMissingDrones()
        {
            if (_drones.Count < MAXDRONECOUNT)
            {
                if (!Mod.GamePlaySettings.TransDroneSpawned)
                {
                    if (MAXDRONECOUNT > _ports.Count)
                    {
                        MAXDRONECOUNT = _ports.Count;
                    }

                    for (int i = 0; i < MAXDRONECOUNT; i++)
                    {
                        var drone = _ports.ElementAt(i).Value.SpawnDrone();
                        _drones.Add(drone);
                    }

                    //foreach (KeyValuePair<string, IDroneDestination> port in _ports)
                    //{
                    //    var drone = port.Value.SpawnDrone();
                    //    _drones.Add(drone);
                    //    //Mod.GamePlaySettings.DronePortAssigns.Add(drone.GetId(), port.Value.GetPortID());
                    //}
                    //transform.parent = _drones.ElementAt(0)?.transform.parent;

                    Mod.GamePlaySettings.TransDroneSpawned = true;
                }
            }
        }

        internal void ResetDrones()
        {
            var drones = GameObject.FindObjectsOfType<DroneController>();

            foreach (DroneController controller in drones)
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

            Mod.GamePlaySettings.TransDroneSpawned = false;

            SpawnMissingDrones();
        }


        public IEnumerable<AlterraTransportDroneEntry> SaveDrones()
        {
            var drones = GameObject.FindObjectsOfType<DroneController>();

            foreach (DroneController drone in drones)
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
            for (int i = 0; i < 1; i++)  // forcing to only make one port
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

        public void OnConsoleCommand_warp()
        {
            Player.main.SetPosition(_warpTrans.position);

        }
    }
}
