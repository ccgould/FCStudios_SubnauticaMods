using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSStorage : ItemsContainer
    {
        private GameObject _storageRoot;
        private byte[] _storageRootBytes;
        

        public byte[] Save(ProtobufSerializer serializer)
        {
            if (serializer == null || _storageRoot == null)
            {
                QuickLogger.DebugError($"Failed to save: Serializer: {serializer} || Root {_storageRoot?.name}",true);
                return null;
            }

            _storageRootBytes = StorageHelper.Save(serializer, _storageRoot);
            return _storageRootBytes;
        }

        public void RestoreItems(ProtobufSerializer serializer, byte[] serialData)
        {
            StorageHelper.RenewIdentifier(_storageRoot);
            if (serialData == null)
            {
                return;
            }
            using (MemoryStream memoryStream = new MemoryStream(serialData))
            {
                QuickLogger.Debug("Getting Data from memory stream");
                GameObject gObj = serializer.DeserializeObjectTree(memoryStream, 0);
                QuickLogger.Debug($"Deserialized Object Stream. {gObj}");
                TransferItems(gObj);
                GameObject.Destroy(gObj);
            }
        }

        private void TransferItems(GameObject source)
        {
            QuickLogger.Debug("Attempting to transfer items");
            foreach (UniqueIdentifier uniqueIdentifier in source.GetComponentsInChildren<UniqueIdentifier>(true))
            {
                QuickLogger.Debug($"Processing {uniqueIdentifier.Id}");
                if (!(uniqueIdentifier.transform.parent != source.transform))
                {
                    QuickLogger.Debug($"{uniqueIdentifier.Id} is not the source continuing to process");

                    Pickupable pickupable = uniqueIdentifier.gameObject.EnsureComponent<Pickupable>();
                    if (!Contains(pickupable))
                    {
                        InventoryItem item = new InventoryItem(pickupable);
                        UnsafeAdd(item);
                        QuickLogger.Debug($"Adding {uniqueIdentifier.Id}");

                    }
                }
            }

            CleanUpDuplicatedStorageNoneRoutine();
        }

        private void CleanUpDuplicatedStorageNoneRoutine()
        {
            QuickLogger.Debug("Cleaning Duplicates", true);
            //TODO Check here

            Transform hostTransform = _storageRoot.transform;
            StoreInformationIdentifier[] sids = _storageRoot.GetComponentsInChildren<StoreInformationIdentifier>(true);
#if DEBUG
            QuickLogger.Debug($"SIDS: {sids.Length}", true);
#endif

            int num;
            for (int i = sids.Length - 1; i >= 0; i = num - 1)
            {
                StoreInformationIdentifier storeInformationIdentifier = sids[i];
                if (storeInformationIdentifier != null && storeInformationIdentifier.name.StartsWith("SerializerEmptyGameObject", StringComparison.OrdinalIgnoreCase))
                {
                    GameObject.Destroy(storeInformationIdentifier.gameObject);
                    QuickLogger.Debug($"Destroyed Duplicate", true);
                }
                num = i;
            }
        }

        public int GetCount()
        {
            int i = 0;
            foreach (TechType techType in GetItemTypes())
            {
                foreach (var invItem in GetItems(techType))
                {
                    i++;
                }
            }

            return i;
        }

        public Dictionary<TechType,int> GetItems()
        {
            //TODO get items
            List<TechType> keys = GetItemTypes();
            var lookup = keys?.Where(x => x != TechType.None).ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => GetCount(count.Key));
        }

        public bool AddItem(InventoryItem item)
        { 
            UnsafeAdd(item);
          return true;
        }

        public FCSStorage(int slots, GameObject storageRoot) : base(slots, slots, storageRoot.transform, "FCSStorage", null)
        {
            _storageRoot = storageRoot;
        }
    }

}
