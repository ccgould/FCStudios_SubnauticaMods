using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Spawnables;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHubFabricatorBuilding.Mono
{
    internal class AlterraFabricatorStationController : MonoBehaviour
    {
        private bool IsPowerOn => _powercellCount >= 3;
        private int _powercellCount;
        private List<GameObject> _doors = new List<GameObject>();
        private IEnumerable<GameObject> _oreConsumerSpawnPoints;
        private IEnumerable<GameObject> _alterraHubDepotSpawnPoints;
        private bool _openDoors;
        private const float Speed = 5;
        private const float OpenPos = 0.207f;
        private const float ClosePos = -0.1668553f;

        private void Awake()
        {
            
        }
        private void Start()
        {
            LoadSave();

            _alterraHubDepotSpawnPoints = GameObjectHelpers.FindGameObjects(gameObject, "_AlterraHubDepotSpawnPnt", SearchOption.StartsWith);
            _oreConsumerSpawnPoints = GameObjectHelpers.FindGameObjects(gameObject, "_OreConsumerSpawnPnt", SearchOption.StartsWith);

            SpawnFragments();
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeSpecSettings(Buildables.AlterraHub.BaseDecalsExterior, Buildables.AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);

        }

        private void Update()
        {
            MoveTray();
        }

        private void MoveTray()
        {
            if (!_openDoors) return;
            foreach (GameObject door in _doors)
            {
                if (door.transform.localPosition.x < OpenPos)
                {
                    door.transform.Translate(Vector3.right * Speed * DayNightCycle.main.deltaTime);
                }

                if (door.transform.localPosition.x > OpenPos)
                {
                    door.transform.localPosition = new Vector3(OpenPos, door.transform.localPosition.y, door.transform.localPosition.z);
                }
            }
        }

        private void LoadSave()
        {
            _powercellCount = Mod.GamePlaySettings.AlterraHubDepotPowercellCount;
        }

        private void SpawnFragments()
        {
            foreach (GameObject spawnPoint in _alterraHubDepotSpawnPoints)
            {
                StartCoroutine(SpawnAlterraHubDepotFrag(spawnPoint));
            }

            foreach (GameObject spawnPoint in _oreConsumerSpawnPoints)
            {
                StartCoroutine(SpawnOreConsumerFrag(spawnPoint));
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
        }

        private void AddPowercellAndCheckPower()
        {
            if(_powercellCount == 3) return;
            _powercellCount++;

            if(IsPowerOn)
            {
                OpenDoors();
                TurnOnLights();
            }

            Mod.GamePlaySettings.AlterraHubDepotPowercellCount = _powercellCount;
        }

        private void OpenDoors()
        {
            _openDoors = true;
        }
    }
}
