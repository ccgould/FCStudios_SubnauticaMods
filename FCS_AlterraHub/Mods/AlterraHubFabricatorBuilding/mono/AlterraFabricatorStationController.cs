using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using Story;
using UnityEngine;
using FCS_AlterraHub.Mods.Common;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AlterraFabricatorStationController : MonoBehaviour
    {
        public static AlterraFabricatorStationController Main;
        private const int PowercellReq = 5;
        internal bool IsPowerOn => _generator.IsAllSlotsOccupied() && _isBaseOn;
        public Transform BaseTransform { get; set; }

        private void OnBecameVisible()
        {
            QuickLogger.Debug("Station is now visible", true);
        }

        private void OnBecameInvisible()
        {
            QuickLogger.Debug("Station is now invisible", true);
        }

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
        private string _st;
        private string _baseID;
        private Transform _warpTrans;
        private float lodBias;
        private PingInstance _ping;
        private bool _gamePlaySettingsLoaded;
        private SecurityBoxTrigger _securityBoxTrigger;
        private SecurityGateController _securityGateController;
        private FCSGamePlaySettings _fcsGamePlaySettings;
        private bool _isBaseOn;
        private DroneDeliveryService _deliveryDroneService;

        private void Update()
        {
            lodBias = QualitySettings.lodBias;
        }

        private void Awake()
        {
            QuickLogger.Debug("FCS Station Awake", true);

            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }

            _warpTrans = GameObjectHelpers.FindGameObject(gameObject, "RespawnPoint").transform;
            BaseTransform = transform;
        }

        private IEnumerator Start()
        {
#if SUBNAUTICA
            CreatePorts();

            _generator = GameObjectHelpers.FindGameObject(gameObject, "Anim_Generator")
                .AddComponent<GeneratorController>();

            _generator.Initialize(this);

            _antenna = GameObjectHelpers.FindGameObject(gameObject, "antenna_controller")
                .AddComponent<AntennaController>();
            _antenna.Initialize(this);
            _antenna.OnBoxFixedAction += index => { Destroy(_electList[index]); };

            var antennaDoorMesh = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor02");
            var antennaDoor = GameObjectHelpers.FindGameObject(antennaDoorMesh, "anim_mesh_door")
                .AddComponent<DoorController>();
            antennaDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;

            var fabricatorDoorMesh = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor01");
            var fabricatorDoor = GameObjectHelpers.FindGameObject(fabricatorDoorMesh, "anim_mesh_door")
                .AddComponent<DoorController>();
            fabricatorDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;

            var transportDoorMesh = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor03");
            var transportDoor = GameObjectHelpers.FindGameObject(transportDoorMesh, "anim_mesh_door")
                .AddComponent<DoorController>();
            transportDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;
            transportDoor.ManualHoverText2 = "No Power.Please check generator";


            var securityDoorMesh = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor04");
            var securityDoor = GameObjectHelpers.FindGameObject(securityDoorMesh, "anim_mesh_door")
                .AddComponent<DoorController>();
            securityDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;
            securityDoor.ManualHoverText2 = "Access to security booth needed to unlock.";

            var antennaDialPad = GameObjectHelpers.FindGameObject(gameObject, "AntennaDialpad")
                .AddComponent<KeyPadAccessController>();
            antennaDialPad.Initialize("3491", antennaDoor, 2);
            _keyPads.Add(antennaDialPad);

            var fabrictorDialPad = GameObjectHelpers.FindGameObject(gameObject, "FabricationDialpad")
                .AddComponent<KeyPadAccessController>();
            fabrictorDialPad.Initialize("8964", fabricatorDoor, 1);
            _keyPads.Add(fabrictorDialPad);

            var transportDialPad = GameObjectHelpers.FindGameObject(gameObject, "TransportDialpad")
                .AddComponent<KeyPadAccessController>();
            transportDialPad.Initialize("4865", transportDoor, 3);
            _keyPads.Add(transportDialPad);

            var securityDialPad = GameObjectHelpers.FindGameObject(gameObject, "SecurityDialpad")
                .AddComponent<KeyPadAccessController>();
            securityDialPad.Initialize("####", securityDoor, 4);
            _keyPads.Add(securityDialPad);

            var securityDialPad2 = GameObjectHelpers.FindGameObject(gameObject, "SecurityDialpad_2")
                .AddComponent<KeyPadAccessController>();
            securityDialPad2.Initialize("####", securityDoor, 5);
            _keyPads.Add(securityDialPad2);

            _motor = GameObjectHelpers.FindGameObject(gameObject, "AlternatorMotor").AddComponent<MotorHandler>();
            _motor.Initialize(100);
            _motor.StopMotor();

            FindScreens();

            FindLights();

            //ToggleIsKinematic(); Disabled for dev use only

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 2f);
            MaterialHelpers.ChangeSpecSettings(AlterraHub.BaseDecalsExterior, AlterraHub.TBaseSpec, gameObject, 2.61f,
                8f);
            MaterialHelpers.ChangeMaterialColor(AlterraHub.BaseSecondaryCol, gameObject, Color.gray);
            FindPortManager();

            var bp1Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt1_");
            var bp2Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt2_");
            var bp3Pos = GameObjectHelpers.FindGameObject(gameObject, "_BlueprintBoxPnt3_");

#if SUBNAUTICA_STABLE
            var dataBox1 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp1Pos.transform.position, bp1Pos.transform.rotation, true);
#else
            var task1 = new TaskResult<GameObject>();
            yield return SpawnHelper.SpawnTechTypeAsync(Mod.FCSDataBoxTechType, bp1Pos.transform.position,
                bp1Pos.transform.rotation,
                true, task1);
            var dataBox1 = task1.Get();
#endif

            var dataBox1C = dataBox1.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox1C, dataBox1, Mod.AlterraHubDepotTechType);

#if SUBNAUTICA_STABLE
            var dataBox2 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp2Pos.transform.position,
                bp2Pos.transform.rotation, true);
#else
            var task2 = new TaskResult<GameObject>();
            yield return SpawnHelper.SpawnTechTypeAsync(Mod.FCSDataBoxTechType, bp2Pos.transform.position,
                bp2Pos.transform.rotation, true, task2);
            var dataBox2 = task1.Get();
#endif
            var dataBox2C = dataBox2.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox2C, dataBox2, Mod.OreConsumerTechType);

#if SUBNAUTICA_STABLE
            var dataBox3 = SpawnHelper.SpawnTechType(Mod.FCSDataBoxTechType, bp3Pos.transform.position,
                bp3Pos.transform.rotation, true);
#else
            var task3 = new TaskResult<GameObject>();
            yield return SpawnHelper.SpawnTechTypeAsync(Mod.FCSDataBoxTechType, bp3Pos.transform.position,
                bp3Pos.transform.rotation, true, task3);
            var dataBox3 = task1.Get();
#endif
            var dataBox3C = dataBox3.GetComponent<BlueprintHandTarget>();
            CreateBluePrintHandTarget(dataBox3C, dataBox3, Mod.DronePortPadHubNewTechType);

            _securityGateController = GameObjectHelpers.FindGameObject(gameObject, "AlterraHubFabStationSecurityGate")
                .AddComponent<SecurityGateController>();
            _securityGateController.Initialize();

            WorldHelpers.CreateBeacon(gameObject, Mod.AlterraHubStationPingType,
                "AlterraHub Fabrication Facility (320m)");
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.red);

            OnGamePlaySettingsLoaded(Mod.GamePlaySettings);

            InvokeRepeating(nameof(CheckIfSecurityDoorCanUnlock), 1f, 1f);
#endif
            _deliveryDroneService = gameObject.EnsureComponent<DroneDeliveryService>();

            if (!UWEHelpers.RequiresPower())
            {
                Main.CompleteStation();
                if (!CardSystem.main.HasBeenRegistered())
                {
                    CardSystem.main.CreateUserAccount("Ryley Robinson", "RyleyRobinson4546B", "planet4546B", "4546", 0);
                }
            }

            yield break;
        }

        private void CheckIfSecurityDoorCanUnlock()
        {
            if (_securityBoxTrigger != null && _securityBoxTrigger.IsPlayerInRange && IsPowerOn)
            {
                UnlockSecurityDoors();
            }
        }

        private void UnlockSecurityDoors()
        {
            if (_keyPads == null || _keyPads.Count < 5) return;

            if (!_keyPads[3].IsUnlocked() || !_keyPads[4].IsUnlocked())
            {
                _keyPads[3].Unlock();
                _keyPads[3].ForceOpen();
                _keyPads[4].Unlock();
                _keyPads[4].ForceOpen();
                CancelInvoke(nameof(CheckIfSecurityDoorCanUnlock));
            }
        }

        internal SecurityBoxTrigger GetSecurityBoxTrigger()
        {
            if (_securityBoxTrigger == null)
            {
                _securityBoxTrigger = GameObjectHelpers.FindGameObject(gameObject, "SecurityBoxTrigger")
                    .EnsureComponent<SecurityBoxTrigger>();
            }

            return _securityBoxTrigger;
        }

        private static void CreateBluePrintHandTarget(BlueprintHandTarget dataBox, GameObject go,
            TechType unlockTechType)
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

        private void FindScreens()
        {
            var screens = GameObjectHelpers.FindGameObject(gameObject, "SecurityComputers").transform;
            foreach (Transform screen in screens)
            {
                var securityScreenController = screen.gameObject.AddComponent<SecurityScreenController>();
                securityScreenController.Initialize(this);
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

        internal void UpdateBeaconState(bool value, int colorIndex = 0)
        {
            if (_ping == null)
            {
                _ping = gameObject?.GetComponentInChildren<PingInstance>();
            }


            QuickLogger.Debug($"Update Beacon State. Is not null Check: {_ping != null}", true);

            if (_ping != null)
            {
                _ping.SetVisible(value);
                _ping.SetColor(colorIndex);
            }
        }

        private void OnGamePlaySettingsLoaded(FCSGamePlaySettings settings)
        {
            _fcsGamePlaySettings = settings;
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

            if (settings.AlterraHubDepotDoors.KeyPad3)
            {
                _keyPads[2].Unlock();
            }

            if (settings.AlterraHubDepotDoors.KeyPad4)
            {
                _keyPads[3].Unlock();
                _keyPads[3].ForceOpen();
                _keyPads[4].Unlock();
                _keyPads[4].ForceOpen();
            }

            _generator.LoadSave();

            _antenna.LoadSave();

            _securityGateController.LoadSave();

            if (Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count >= PowercellReq &&
                Mod.GamePlaySettings.BreakerOn)
            {
                TurnOnBase();
            }

            _deliveryDroneService.SetCurrentOrder(settings.CurrentOrder);


            if (!string.IsNullOrWhiteSpace(_deliveryDroneService.GetCurrentOrder()?.OrderNumber))
            {
                FCSPDAController.Main.AddShipment(_deliveryDroneService.GetCurrentOrder());
                _deliveryDroneService.SetCompletePendingOrder(true);
            }

            _deliveryDroneService.SetPendingPurchase(settings.PendingPurchases);

            UpdateBeaconState(settings.FabStationBeaconVisible, settings.FabStationBeaconColorIndex);

            _gamePlaySettingsLoaded = true;


            if (DetermineIfUnlocked())
            {
                //KnownTech.Add(FCSAlterraHubService.PublicAPI.FcsUnlockTechType);
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
            _keyPads[2].Unlock();
            _keyPads[2].ForceOpen();
            _motor.StartMotor();
            _isBaseOn = true;
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
            Player.main.OnPlayerPositionCheat();
        }

        public PingInstance GetPing()
        {
            return _ping;
        }

        public void Save()
        {
            QuickLogger.Debug("Saving Station");
            Mod.GamePlaySettings.FabStationBeaconColorIndex = GetPing().colorIndex;
            Mod.GamePlaySettings.FabStationBeaconVisible = GetPing().visible;
            Mod.GamePlaySettings.AlterraHubDepotDoors.KeyPad1 = _keyPads[0].IsUnlocked();
            Mod.GamePlaySettings.AlterraHubDepotDoors.KeyPad2 = _keyPads[1].IsUnlocked();
            Mod.GamePlaySettings.AlterraHubDepotDoors.KeyPad3 = _keyPads[2].IsUnlocked();
            Mod.GamePlaySettings.AlterraHubDepotDoors.KeyPad4 = _keyPads[3].IsUnlocked();
            Mod.GamePlaySettings.AlterraHubDepotDoors.SecurityDoors = _securityGateController.GetHealth();
            Mod.GamePlaySettings.AlterraHubDepotPowercellSlot = _generator.Save().ToList();
            Mod.GamePlaySettings.IsPDAUnlocked = DetermineIfUnlocked();
            Mod.GamePlaySettings.CurrentOrder = _deliveryDroneService.GetCurrentOrder();
            Mod.GamePlaySettings.BreakerOn = _isBaseOn;
            Mod.GamePlaySettings.FixedPowerBoxes = _antenna.Save().ToHashSet();
            Mod.GamePlaySettings.TransDroneSpawned = _deliveryDroneService.GetDrones().Any();
            QuickLogger.Debug("Saving Station Complete");
        }

        public void CompleteStation()
        {
#if SUBNAUTICA
            UpdateBeaconState(false);
            foreach (var keyPadAccessController in _keyPads)
            {
                keyPadAccessController.Unlock();
            }

            _securityGateController.Unlock(true);
            _generator.CompleteObjective();
            _isBaseOn = true;
            _antenna.CompleteObjective();
            TurnOnBase();
#endif
            QuickLogger.Debug("Station Object Complete");
        }

        public bool DetermineIfUnlocked()
        {
#if SUBNAUTICA
                        return _isBaseOn && _keyPads[0].IsUnlocked() && _keyPads[1].IsUnlocked() && _keyPads[2].IsUnlocked() &&
                   _securityGateController.IsUnlocked() && _antenna.IsAllElectricalBoxesFixed() &&
                   _antenna.IsUnlocked();
#else
            return true;
#endif

        }

        public void MakeStationDirty()
        {
            UpdateBeaconState(true);
            foreach (var keyPadAccessController in _keyPads)
            {
                keyPadAccessController.Lock();
            }

            _securityGateController.Lock();
            _generator.MakeDirty();
            _isBaseOn = false;
            _antenna.MakeDirty();
            QuickLogger.Debug("Station Object UnComplete");
        }

        public DroneDeliveryService GetDeliveryService() => _deliveryDroneService;
    }
}