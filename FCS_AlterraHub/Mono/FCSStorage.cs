using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSStorage : MonoBehaviour
    {
        private GameObject _storageRoot;
        private byte[] _storageRootBytes;
        public ItemsContainer ItemsContainer;

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
            QuickLogger.Debug("RestoreItems");
            StorageHelper.RenewIdentifier(_storageRoot);
            QuickLogger.Debug("RenewIdentifier Called");
            if (serialData == null)
            {
                return;
            }
            
            QuickLogger.Debug($"Storage root Position: {_storageRoot.transform.position}");

            using (MemoryStream memoryStream = new MemoryStream(serialData))
            {
                QuickLogger.Debug("Getting Data from memory stream");
                GameObject gObj = serializer.DeserializeObjectTree(memoryStream, 1);
                QuickLogger.Debug($"Deserialized Object Stream. {gObj}");
                TransferItems(gObj);
                QuickLogger.Debug("Items Transfered");
                GameObject.Destroy(gObj);
                QuickLogger.Debug("Item destroyed");
            }
        }

        private void TransferItems(GameObject source)
        {
            QuickLogger.Debug("Attempting to transfer items");

            foreach (UniqueIdentifier uniqueIdentifier in source.GetComponentsInChildren<UniqueIdentifier>(true))
            {
                QuickLogger.Debug($"Processing {uniqueIdentifier.Id} :Position: {uniqueIdentifier.transform.position}: Parent: {uniqueIdentifier.transform.parent} Parent transform: {uniqueIdentifier.transform.parent.position}");
                if (!(uniqueIdentifier.transform.parent != source.transform))
                {
                    QuickLogger.Debug($"{uniqueIdentifier.Id} is not the source continuing to process");

                    Pickupable pickupable = uniqueIdentifier.gameObject.EnsureComponent<Pickupable>();
                    if (!ItemsContainer.Contains(pickupable))
                    {
                        QuickLogger.Debug("Creating new inventory item");
                        InventoryItem item = new InventoryItem(pickupable);
                        QuickLogger.Debug($"Is Active and Enabled: {item.item.gameObject.GetComponent<UniqueIdentifier>().isActiveAndEnabled}");
                        var h = item.item.gameObject.GetComponentsInChildren<UniqueIdentifier>(true);
                        foreach (var identifier in h)
                        {
                            identifier.Id = Guid.NewGuid().ToString();
                        }
                        
                        item.item.transform.parent = _storageRoot.transform;

                        QuickLogger.Debug($"Object posiiton = {item.item.transform.position}");

                        QuickLogger.Debug($"ItemsContainer position: {ItemsContainer.tr.position}");
                        ItemsContainer.UnsafeAdd(item);
                        QuickLogger.Debug($"Adding {uniqueIdentifier.Id}");
                    }
                }
            }

            CleanUpDuplicatedStorageNoneRoutine();
        }

        public void CleanUpDuplicatedStorageNoneRoutine()
        {
            QuickLogger.Debug("Cleaning Duplicates", true);

            StoreInformationIdentifier[] sids = gameObject.GetComponentsInChildren<StoreInformationIdentifier>(true);
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
            foreach (TechType techType in ItemsContainer.GetItemTypes())
            {
                foreach (var invItem in ItemsContainer.GetItems(techType))
                {
                    i++;
                }
            }

            return i;
        }

        public Dictionary<TechType,int> GetItems()
        {
            List<TechType> keys = ItemsContainer.GetItemTypes();
            var lookup = keys?.Where(x => x != TechType.None).ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => ItemsContainer.GetCount(count.Key));
        }

        public bool AddItem(InventoryItem item)
        {
            ItemsContainer.UnsafeAdd(item);
          return true;
        }

        public void Initialize(int slots)
        {
            _storageRoot = new GameObject("FCSStorage");
            _storageRoot.transform.parent = transform;
            //UWE.Utils.ZeroTransform(_storageRoot);
            _storageRoot.AddComponent<StoreInformationIdentifier>();
            ItemsContainer = new ItemsContainer(slots, slots, _storageRoot.transform, "FCSStorage", null);
        }
    }
}
