using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubDepot.Spawnable;
using FCS_AlterraHub.Mods.OreConsumer.Spawnable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class AlterraFabricatorStationController : MonoBehaviour
    {
        private const int PowercellReq = 5;
        internal bool IsPowerOn => Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count >= PowercellReq && Mod.GamePlaySettings.BreakerOn;
        private List<GameObject> _doors = new List<GameObject>();
        private IEnumerable<GameObject> _oreConsumerSpawnPoints;
        private IEnumerable<GameObject> _alterraHubDepotSpawnPoints;
        private Light[] _lights;
        private List<KeyPadAccessController> _keyPads = new List<KeyPadAccessController>();
        private List<SecurityScreenController> _screens = new List<SecurityScreenController>();
        private GeneratorController _generator;
        private MotorHandler _motor;
        private AntennaController _antenna;

        private void FindGirder()
        {
            //starship_girder_10
            //99c0da07-a612-4cb7-9e16-e2e6bd3d6207
            StartCoroutine(FindPrefab("99c0da07-a612-4cb7-9e16-e2e6bd3d6207"));
        }

        private IEnumerator FindPrefab(string classId)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classId);
            yield return request;
            GameObject prefab;
            if (!request.TryGetPrefab(out prefab))
            {
                Debug.LogErrorFormat(this, "Failed to request prefab for '{0}'", new object[]
                {
                    classId
                });
                //Destroy(base.gameObject);
                yield break;
            }
            DeferredSpawner.Task deferredTask = DeferredSpawner.instance.InstantiateAsync(prefab, Vector3.zero, base.transform.localRotation, true);
            //DeferredSpawner.Task deferredTask = DeferredSpawner.instance.InstantiateAsync(prefab, base.transform.localPosition, base.transform.localRotation, true);
            yield return deferredTask;
            GameObject result = deferredTask.result;
            DeferredSpawner.instance.ReturnTask(deferredTask);
            result.transform.SetParent(base.transform.parent, false);
            result.transform.localScale = base.transform.localScale;
            result.SetActive(true);
            //Destroy(base.gameObject);
            yield break;
        }

        private void Awake()
        {
            Mod.OnGamePlaySettingsLoaded += OnGamePlaySettingsLoaded;
        }
        private void Start()
        {
            FindGirder();
            _alterraHubDepotSpawnPoints = GameObjectHelpers.FindGameObjects(gameObject, "_AlterraHubDepotSpawnPnt", SearchOption.StartsWith);
            _oreConsumerSpawnPoints = GameObjectHelpers.FindGameObjects(gameObject, "_OreConsumerSpawnPnt", SearchOption.StartsWith);
            
            _generator = GameObjectHelpers.FindGameObject(gameObject, "AlterraHubFabStationGenerator").AddComponent<GeneratorController>();
            _generator.Initialize(this);            
            
            _antenna = GameObjectHelpers.FindGameObject(gameObject, "AlterraHubFabStationAntenna").AddComponent<AntennaController>();
            _antenna.Initialize(this);

            var antennaDoor = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor02Controller").AddComponent<DoorController>();
            antennaDoor.doorOpenMethod = StarshipDoor.OpenMethodEnum.Manual;

            var fabricatorDoor = GameObjectHelpers.FindGameObject(gameObject, "LockedDoor01Controller").AddComponent<DoorController>();
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
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseLightsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(Buildables.AlterraHub.BaseLightsEmissiveController, gameObject, 2f);
            MaterialHelpers.ChangeSpecSettings(Buildables.AlterraHub.BaseDecalsExterior, Buildables.AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);
            Mod.LoadGamePlaySettings();
           
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

            SpawnFragments();
        }

        private void SpawnFragments()
        {
            if(Mod.GamePlaySettings.IsOreConsumerFragmentSpawned) return;

            foreach (GameObject spawnPoint in _alterraHubDepotSpawnPoints)
            {
                StartCoroutine(SpawnAlterraHubDepotFrag(spawnPoint));
            }

            foreach (GameObject spawnPoint in _oreConsumerSpawnPoints)
            {
                StartCoroutine(SpawnOreConsumerFrag(spawnPoint));
            }

            Mod.GamePlaySettings.IsOreConsumerFragmentSpawned = true;
        }

        private void CleanDuplicates()
        {
            var temp = new List<GameObject>();
            var currentOreConsumerFrags = Resources.FindObjectsOfTypeAll<OreConsumerFragmentSpawn>();
            var alterraHubDepotFragments = Resources.FindObjectsOfTypeAll<AlterraHubDepotFragmentSpawn>();

            foreach (OreConsumerFragmentSpawn consumerFrag in currentOreConsumerFrags)
            {
                QuickLogger.Debug($"Attempting to delete: {consumerFrag.gameObject.name}");
                if (gameObject.name.Contains("Clone"))
                {
                    temp.Add(consumerFrag.gameObject);
                    QuickLogger.Debug($"Deleted oreconsumer");
                }
            }

            foreach (AlterraHubDepotFragmentSpawn consumerFrag in alterraHubDepotFragments)
            {
                QuickLogger.Debug($"Attempting to delete: {consumerFrag.gameObject.name}");
                if (gameObject.name.Contains("Clone"))
                {
                    temp.Add(consumerFrag.gameObject);
                    QuickLogger.Debug($"Deleted oreconsumer");
                }
            }

            foreach (GameObject item in temp)
            {
                DestroyImmediate(item);
            }
        }

        internal static IEnumerator SpawnOreConsumerFrag(GameObject point)
        {

            QuickLogger.Debug("Spawn Frag");
            var prefabForTechType = CraftData.GetPrefabForTechTypeAsync(QPatch.OreConsumerFragTechType);
            yield return prefabForTechType;


            if (prefabForTechType.GetResult() != null)
            {
                GameObject gameObject = Utils.CreatePrefab(prefabForTechType.GetResult(), 1000);
                LargeWorldEntity.Register(gameObject);
                CrafterLogic.NotifyCraftEnd(gameObject, QPatch.OreConsumerFragTechType);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                gameObject.transform.parent = point.transform;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                ErrorMessage.AddDebug("Could not find prefab for TechType = " + QPatch.OreConsumerFragTechType);
            }
            yield break;
        }

        internal static IEnumerator SpawnAlterraHubDepotFrag(GameObject point)
        {

            QuickLogger.Debug("Spawn Frag");
            var prefabForTechType = CraftData.GetPrefabForTechTypeAsync(Mod.AlterraHubDepotFragmentTechType);
            yield return prefabForTechType;


            if (prefabForTechType.GetResult() != null)
            {
                GameObject gameObject = Utils.CreatePrefab(prefabForTechType.GetResult(), 1000);
                LargeWorldEntity.Register(gameObject);
                CrafterLogic.NotifyCraftEnd(gameObject, Mod.AlterraHubDepotFragmentTechType);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                gameObject.transform.parent = point.transform;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                ErrorMessage.AddDebug("Could not find prefab for TechType = " + Mod.AlterraHubDepotFragmentTechType);
            }
            yield break;
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
