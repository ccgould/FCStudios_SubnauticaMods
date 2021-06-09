using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AlterraFabricatorStationController : MonoBehaviour, IDroneDestination
    {
        private const int PowercellReq = 5;
        internal bool IsPowerOn => Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count >= PowercellReq && Mod.GamePlaySettings.BreakerOn;
        public Transform[] StationDeparturePath { get; set; }
        public Transform BaseTransform { get; set; }
        public List<Transform> GetPaths()
        {
            return StationDeparturePath.ToList();
        }

        public void Offload(Dictionary<TechType, int> order, Action onOffloadCompleted)
        {
            onOffloadCompleted?.Invoke();
        }

        public string BaseId { get; set; }
        private Light[] _lights;
        private List<KeyPadAccessController> _keyPads = new List<KeyPadAccessController>();
        private List<SecurityScreenController> _screens = new List<SecurityScreenController>();
        private GeneratorController _generator;
        private MotorHandler _motor;
        private AntennaController _antenna;

        private readonly GameObject[] _electList = new GameObject[6];


        private void Awake()
        {
            BaseId = gameObject.GetComponent<PrefabIdentifier>()?.Id;
            Mod.OnGamePlaySettingsLoaded += OnGamePlaySettingsLoaded;
            BaseTransform = transform;
        }
        
        private void Start()
        {
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

            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseLightsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(Buildables.AlterraHub.BaseLightsEmissiveController, gameObject, 2f);
            MaterialHelpers.ChangeSpecSettings(Buildables.AlterraHub.BaseDecalsExterior, Buildables.AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);
            Mod.LoadGamePlaySettings();
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
        }

        private void TurnOnLights()
        {
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseLightsEmissiveController, gameObject, Color.white);
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
    }
}
