using System;
using System.Collections;
using System.Collections.Generic;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace FCS_AlterraHub.Helpers
{
    public static class SpawnHelper
    {
        private static Dictionary<string, string> PlantResourceDictionary = new Dictionary<string, string>
        {
            {"[CORAL_REEF_PLANT_MIDDLE]", "WorldEntities/Doodads/Coral_reef/coral_reef_plant_middle_03"},
            {"[CORAL_REEF_G3]", "WorldEntities/Doodads/Coral_reef/coral_reef_grass_03"},
            {"[CORAL_REEF_SMALL_DECO]", "WorldEntities/Doodads/Coral_reef/coral_reef_small_deco_14"},
            {"[PURPLE_FAN]", "WorldEntities/Doodads/Coral_reef/Coral_reef_purple_fan"},
        };

        private static Dictionary<UWEPrefabID, string> UWEClassIDDictionary = new Dictionary<UWEPrefabID, string>
        {
            {UWEPrefabID.UnderwaterElecSourceMedium, "ff8e782e-e6f3-40a6-9837-d5b6dcce92bc"},
            {UWEPrefabID.FloatingPapers, "b4ec5044-5519-4743-b61b-92a8b6fe4a32"},
            {UWEPrefabID.BubbleColumnSmall, "5ec8b8a6-b9b1-412b-9048-62701346cca2"},
            {UWEPrefabID.BubbleColumnBig, "0dbd3431-62cc-4dd2-82d5-7d60c71a9edf"},
            {UWEPrefabID.StarshipGirder10, "99c0da07-a612-4cb7-9e16-e2e6bd3d6207"},
            {UWEPrefabID.DataBoxLight, "08e6c2a8-76df-41de-87fd-5cba315a8aa4"},
            {UWEPrefabID.Platform, "e9b75112-f920-45a9-97cc-838ee9b389bb"}
        };

        public static bool ContainsPlant(string plantKey)
        {
            return PlantResourceDictionary.ContainsKey(plantKey);
        }



        public static IEnumerator SpawnUWEPrefab(UWEPrefabID uwePrefab,MonoBehaviour owner, Transform transform, Action<GameObject> callBack = null, bool removeComponents = true)
        {
            string key;
            if (!PrefabDatabase.TryGetPrefabFilename(UWEClassIDDictionary[uwePrefab], out key))
            {
                Debug.LogErrorFormat("Failed to request prefab for '{0}'", new object[]
                {
                    UWEClassIDDictionary[uwePrefab]
                });
                //UnityEngine.Object.Destroy(base.gameObject);
                yield break;
            }
            Transform parent = transform.parent;
            DeferredSpawner.Task task = DeferredSpawner.instance.InstantiateAsync(key, owner, parent, transform.localPosition, transform.localRotation, false, false);
            yield return task;
            GameObject result = task.GetResult();
            if (result != null)
            {
                result.transform.localScale = transform.localScale;
                result.SetActive(true);
                if (removeComponents)
                {
                    GameObject.DestroyImmediate(result.GetComponent<PrefabIdentifier>());
                    GameObject.DestroyImmediate(result.GetComponent<LargeWorldEntity>());
                }

                callBack?.Invoke(result);
                //Action onInstantiate = this.OnInstantiate;
                //if (onInstantiate != null)
                //{
                //    onInstantiate();
                //}
            }
            //UnityEngine.Object.Destroy(base.gameObject);
            //this.spawnCoroutine = null;
            yield break;
        }


        public static IEnumerator SpawnTechTypeAsync(TechType techType, Vector3 position, Quaternion rotation,
            bool spawnGlobal, IOut<GameObject> gameObject)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType, true);
            yield return request;

            if (request.GetResult() != null)
            {
                GameObject go = Utils.CreatePrefab(request.GetResult());
                LargeWorldEntity.Register(go);
                CrafterLogic.NotifyCraftEnd(go, techType);
                go.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                go.transform.position = position;
                go.transform.localRotation = rotation;
                if (spawnGlobal) go.transform.parent = null;

                gameObject.Set(go);
                yield break;
            }

            ErrorMessage.AddDebug("Could not find prefab for TechType = " + techType);
            gameObject.Set(null);
        }

        public static GameObject SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation,
            bool spawnGlobal = false)
        {
            if (prefab != null)
            {
                GameObject gameObject = Utils.CreatePrefab(prefab);
                LargeWorldEntity.Register(gameObject);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                gameObject.transform.position = position;
                gameObject.transform.localRotation = rotation;
                if (spawnGlobal) gameObject.transform.parent = null;
                return gameObject;
            }

            return null;
        }
    }

    public enum UWEPrefabID
    {
        UnderwaterElecSourceMedium = 0,
        FloatingPapers = 1,
        BubbleColumnSmall = 2,
        BubbleColumnBig = 3,
        StarshipGirder10 = 4,
        DataBoxLight = 5,
        Platform = 6
    }
}