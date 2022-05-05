using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSStorage : StorageContainer, IFCSStorage
    {
        private byte[] _storageRootBytes;
        public int SlotsAssigned;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public int GetContainerFreeSpace => SlotsAssigned - GetCount();
        public bool IsFull => GetCount() >= SlotsAssigned;
        public List<TechType> InvalidTechTypes = new List<TechType>();
        private bool _isSubscribed;
        private FcsDevice _fcsDevice;

        public ItemsContainer ItemsContainer
        {
            get
            {
                if (container != null || storageRoot == null) return container;
                CreateContainer();
                Subscribe();
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
            container.onRemoveItem += OnRemoveItem;
            _isSubscribed = true;
        }

        private void OnRemoveItem(InventoryItem item)
        {
            QuickLogger.Debug("FCSStorage Container Item Removed",true);
        }


        public Action OnContainerClosed { get; set; }

        public int StorageCount()
        {
            return GetCount();
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

        public virtual bool AddItemToContainer(InventoryItem item)
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
            _fcsDevice = gameObject.GetComponentInChildren<FcsDevice>();
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
            storageLabel = storageName;
            Initialize(classID);
            Resize(storageWidth, storageHeight);

        }

        public override void Open(Transform useTransform)
        {
            if (!IsAllowedToOpen) return;
            base.Open(useTransform);
            OnContainerOpened?.Invoke();
        }

        public bool IsAllowedToOpen { get; set; } = true;

        public virtual bool CanBeStored(int amount, TechType techType)
        {
            QuickLogger.Debug($"GetCount: {GetCount()} | Amount {amount} | Slots: {SlotsAssigned}", true);

            if (InvalidTechTypes.Contains(techType) || IsFull || (container.allowedTech != null &&  container.allowedTech.Any() && !container.allowedTech.Contains(techType))) return false;
            
            return GetCount() + amount <= SlotsAssigned;
        }

        public virtual bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return !NotAllowedToAddItems && CanBeStored(1,pickupable.GetTechType());
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

        public virtual  bool ContainsItem(TechType techType)
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
