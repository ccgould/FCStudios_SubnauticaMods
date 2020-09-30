using System.Collections.Generic;
using DataStorageSolutions.Buildables;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Model
{
    internal class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;

        internal class Pool
        {
            public int Tag { get; set; }
            public GameObject Prefab { get; set; }
            public int Size { get; set; }
        }

        public Dictionary<int, Queue<GameObject>> PooledDictionary = new Dictionary<int, Queue<GameObject>>();
        private bool _isBeingDestroyed;

        public void Initialize()
        {
            Instance = this;
        }

        private void Start()
        {
            var pools = new List<Pool>
            {
                new Pool{Tag = 0, Prefab = DSSModelPrefab.ItemPrefab, Size = 44}
            };

            foreach (Pool pool in pools)
            {
                var objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.Size; i++)
                {
                    GameObject obj = Instantiate(pool.Prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                PooledDictionary.Add(pool.Tag, objectPool);
            }
        }

        public GameObject SpawnFromPool(int tag, GameObject parent)
        {
            if (_isBeingDestroyed) return null;
            if (!PooledDictionary.ContainsKey(tag))
            {
                QuickLogger.Debug($"Pool with tag {tag} doesn't exist;");
                return null;
            }

            GameObject objectToSpawn = PooledDictionary[tag].Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.SetParent(parent.transform, false);
            PooledDictionary[tag].Enqueue(objectToSpawn);
            return objectToSpawn;
        }

        public void Reset(int tag)
        {
            if(PooledDictionary == null || _isBeingDestroyed) return;
            if (!PooledDictionary.ContainsKey(tag))
            {
                QuickLogger.Error($"Pool with tag {tag} doesn't exist;");
                return;
            }
            
            foreach (GameObject obj in PooledDictionary[tag])
            {
                if(obj == null) continue;
                obj.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }
    }
}
