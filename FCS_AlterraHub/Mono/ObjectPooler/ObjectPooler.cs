using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Mono.ObjectPooler
{

    public class Pool
    {
        public string Tag { get; set; }
        public GameObject Prefab { get; set; }
        public int Size { get; set; }
    }

    public class ObjectPooler : MonoBehaviour
    {
        public List<Pool> Pools { get; set; }
        public Dictionary<string,Queue<GameObject>> PoolDictionary { get; set; }
        public static ObjectPooler Instance;

        public void Initialize()
        {
            Instance = this;
            foreach (Pool pool in Pools)
            {
                var objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.Size; i++)
                {
                    var obj = Instantiate(pool.Prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                PoolDictionary.Add(pool.Tag, objectPool);
            }
        }

        public void AddPool(string objTag, int size, GameObject prefab)
        {
            if (PoolDictionary == null || Pools == null)
            {
                PoolDictionary = new Dictionary<string, Queue<GameObject>>();
                Pools = new List<Pool>();
            }

            Pools.Add(new Pool{Prefab = prefab, Size = size, Tag = objTag});
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!PoolDictionary.ContainsKey(tag)) return null;
            var objectToSpawn = PoolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            PoolDictionary[tag].Enqueue(objectToSpawn);
            return objectToSpawn;
        }

        public GameObject SpawnFromPool(string tag, GameObject parent)
        {
            if (!PoolDictionary.ContainsKey(tag)) return null;
            var objectToSpawn = PoolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.SetParent(parent.transform, false);
            PoolDictionary[tag].Enqueue(objectToSpawn);
            return objectToSpawn;
        }

        public void Reset(string tag)
        {
            if (PoolDictionary == null) return;
            if (!PoolDictionary.ContainsKey(tag))
            {
                return;
            }

            foreach (GameObject obj in PoolDictionary[tag])
            {
                if (obj == null) continue;
                obj.SetActive(false);
            }
        }

        public IEnumerable<GameObject> GetActive()
        {
            foreach (KeyValuePair<string, Queue<GameObject>> item in PoolDictionary)
            {
                if (item.Value.Peek().activeSelf)
                {
                    yield return item.Value.Peek();
                }
            }
        }
    }
}
