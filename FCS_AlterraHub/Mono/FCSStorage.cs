using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSStorage : StorageContainer, IFCSStorage
    {
        private byte[] _storageRootBytes;
        public int SlotsAssigned { get; set; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public int GetContainerFreeSpace => SlotsAssigned - GetCount();
        public bool IsFull => GetCount() >= SlotsAssigned;
        public List<TechType> InvalidTechTypes = new List<TechType>();
        private bool _isSubscribed;

        public ItemsContainer ItemsContainer
        {
            get
            {
                if (container == null && storageRoot != null)
                {
                    CreateContainer();
                    Subscribe();
                }
                return container;
            }
        }

        public override void Awake()
        {
            base.Awake();
            Subscribe();
        }

        private void Subscribe()
        {
            if (container == null || _isSubscribed) return;
            container.isAllowedToAdd += IsAllowedToAdd;
            container.isAllowedToRemove += IsAllowedToRemoveItems;
            _isSubscribed = true;
        }

        public Action OnContainerClosed { get; set; }

        public int StorageCount()
        {
            return GetCount();
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            if (serializer == null || storageRoot == null)
            {
                QuickLogger.DebugError($"Failed to save: Serializer: {serializer} || Root {storageRoot?.name}",true);
                return null;
            }

            _storageRootBytes = StorageHelper.Save(serializer, storageRoot.gameObject);
            return _storageRootBytes;
        }

#if SUBNAUTICA_STABLE
        public void RestoreItems(ProtobufSerializer serializer, byte[] serialData)
        {
            QuickLogger.Debug("RestoreItems");
            StorageHelper.RenewIdentifier(storageRoot.gameObject);
            QuickLogger.Debug("RenewIdentifier Called");
            if (serialData == null)
            {
                return;
            }

            QuickLogger.Debug($"Storage root Position: {storageRoot.transform.position}");

            using (MemoryStream memoryStream = new MemoryStream(serialData))
            {
                QuickLogger.Debug("Getting Data from memory stream");
                GameObject gObj = serializer.DeserializeObjectTree(memoryStream, 0);
                QuickLogger.Debug($"De-serialized Object Stream. {gObj} | {gObj.name}");
                TransferItems(gObj);
                QuickLogger.Debug("Items Transferred");
                QuickLogger.Debug($"Object location: {gObj.transform.position}");
                Destroy(gObj);
                QuickLogger.Debug("Item destroyed");
            }
        }

        public void RestoreItems(ProtobufSerializer serializer, List<byte[]> serialData)
        {
            QuickLogger.Debug("RestoreItems");
            
            StorageHelper.RenewIdentifier(storageRoot.gameObject);
            
            QuickLogger.Debug("RenewIdentifier Called");
            
            if (serialData == null) return;

            QuickLogger.Debug($"Storage root Position: {storageRoot.transform.position}");

            for (var i = 0; i < serialData.Count; i++)
            {
                byte[] bytes = serialData[i];

                QuickLogger.Debug($"Loading Save data: {i}/{serialData.Count}");

                if (bytes == null) continue;

                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    QuickLogger.Debug("Getting Data from memory stream");
                    GameObject gObj = serializer.DeserializeObjectTree(memoryStream, 0);
                    QuickLogger.Debug($"De-serialized Object Stream. {gObj} | {gObj.name}");
                    TransferItems(gObj);
                    QuickLogger.Debug("Items Transferred");
                    QuickLogger.Debug($"Object location: {gObj.transform.position}");
                    Destroy(gObj);
                    QuickLogger.Debug("Item destroyed");
                }
            }
        }
#else
public IEnumerator RestoreItems(ProtobufSerializer serializer, byte[] serialData)
        {
            QuickLogger.Debug("RestoreItems");
            StorageHelper.RenewIdentifier(_storageRoot);
            QuickLogger.Debug("RenewIdentifier Called");
            if (serialData == null)
            {
                yield break;
            }
            
            QuickLogger.Debug($"Storage root Position: {_storageRoot.transform.position}");

            using (MemoryStream memoryStream = new MemoryStream(serialData))
            {
                QuickLogger.Debug("Getting Data from memory stream");
                CoroutineTask<GameObject> task = serializer.DeserializeObjectTreeAsync(memoryStream, false,false,0);
                yield return task;
                var gObj = task.GetResult();
                QuickLogger.Debug($"De-serialized Object Stream. {gObj} | {gObj.name}");
                TransferItems(gObj);
                QuickLogger.Debug("Items Transferred");
                QuickLogger.Debug($"Object location: {gObj.transform.position}");
                Destroy(gObj);
                QuickLogger.Debug("Item destroyed");
            }
            yield break;
        }
#endif

        private void TransferItems(GameObject source)
        {
            QuickLogger.Debug("Attempting to transfer items");

            foreach (UniqueIdentifier uniqueIdentifier in source.GetComponentsInChildren<UniqueIdentifier>(true))
            {
                if (!(uniqueIdentifier.transform.parent != source.transform))
                {
                    Pickupable pickupable = uniqueIdentifier.gameObject.EnsureComponent<Pickupable>();
                    if (!ItemsContainer.Contains(pickupable))
                    {
                        InventoryItem item = new InventoryItem(pickupable);
                        ChangePrefabId(item);
                        item.item.transform.parent = storageRoot.transform;
                        ItemsContainer.UnsafeAdd(item);
                    }
                }
            }

            CleanUpDuplicatedStorageNoneRoutine();
        }

        private static void ChangePrefabId(InventoryItem item)
        {
            var uniqueIdentifiers = item.item.gameObject.GetComponentsInChildren<UniqueIdentifier>(true);
            
            foreach (var identifier in uniqueIdentifiers)
            {
                identifier.Id = Guid.NewGuid().ToString();
            }
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
                    QuickLogger.Debug($"Duplicate: {storeInformationIdentifier.gameObject.transform.position} | {storeInformationIdentifier.gameObject.name}");
                    Destroy(storeInformationIdentifier.gameObject);
                    QuickLogger.Debug($"Destroyed Duplicate");
                }
                num = i;
            }
        }

        public int GetCount()
        {
            var i = 0;

            if (container == null) return i;

            foreach (TechType techType in ItemsContainer.GetItemTypes())
            {
                i += ItemsContainer.GetItems(techType).Count;
            }

            return i;
        }

        public int GetFreeSpace()
        {
            return SlotsAssigned - GetCount();
        }

        public Dictionary<TechType,int> GetItemsWithin()
        {
            List<TechType> keys = ItemsContainer.GetItemTypes();
            var lookup = keys?.Where(x => x != TechType.None).ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => ItemsContainer.GetCount(count.Key));
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            container.UnsafeAdd(item);
          return true;
        }
        
        /// <summary>
        /// *Note this must be attached to a child of the root gameObject.
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="classID"></param>
        /// <param name="storageRoot"></param>
        public void Initialize(string classID)
        {
            prefabRoot = transform.gameObject;
            var tempStorageRoot = transform.Find("StorageRoot")?.gameObject;

            if (tempStorageRoot == null)
            {
                tempStorageRoot = new GameObject("StorageRoot");
                tempStorageRoot.transform.parent = transform;
                UWE.Utils.ZeroTransform(tempStorageRoot);
            }

            var childObjectIdentifier = tempStorageRoot.EnsureComponent<ChildObjectIdentifier>();
            childObjectIdentifier.classId = classID; 
            storageRoot = childObjectIdentifier;
        }

        public virtual bool IsAllowedToRemoveItems(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public void Initialize(int slots, int storageWidth, int storageHeight,string storageName, string classID)
        {
            SlotsAssigned = slots;
            height = storageHeight;
            width = storageWidth;
            storageLabel = storageName;
            Initialize(classID);
        }

        public override void Open(Transform useTransform)
        {
            base.Open(useTransform);
            OnContainerOpened?.Invoke();
        }

        public virtual bool CanBeStored(int amount, TechType techType)
        {
            QuickLogger.Debug($"GetCount: {GetCount()} | Amount {amount} | Slots: {SlotsAssigned}", true);

            if (InvalidTechTypes.Contains(techType) || IsFull || (container.allowedTech != null &&  container.allowedTech.Any() && !container.allowedTech.Contains(techType))) return false;
            
            return GetCount() + amount <= SlotsAssigned;
        }

        public virtual bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (NotAllowedToAddItems) return false;

            return CanBeStored(1,pickupable.GetTechType());
        }

        public bool NotAllowedToAddItems { get; set; }

        public virtual bool IsAllowedToRemoveItems()
        {
            return IsAllowedToRemove;
        }

        public bool IsAllowedToRemove { get; set; } = true;
        public Action OnContainerOpened { get; set; }

        public virtual Pickupable RemoveItemFromContainer(TechType techType)
        {
           return ItemsContainer.RemoveItem(techType);
        }

        public bool ContainsItem(TechType techType)
        {
            return ItemsContainer.Contains(techType);
        }

        public override void OnClose()
        {
            OnContainerClosed?.Invoke();
        }
        

        /// <summary>
        /// To deactivate the storage container to prevent hover
        /// </summary>
        public void Deactivate()
        {
            enabled = false;
        }


        /// <summary>
        /// To activate the storage container to allow hover
        /// </summary>
        public void Activate()
        {
            enabled = true;
        }
    }
}
