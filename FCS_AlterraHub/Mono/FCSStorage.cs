using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSStorage : MonoBehaviour, IItemsContainer
    {
        private GameObject _storageRoot;
        private Dictionary<TechType, ItemGroup> _items;
        private bool _unsorted;
        private int _count;
        private byte[] _storageRootBytes;
        private int _slots;
        public event OnAddItem onAddItem;
        public event OnRemoveItem onRemoveItem;

        public void Initialize(int slots,GameObject storageRoot)
        {
            _storageRoot = storageRoot;
            _slots = slots;
            _items = new Dictionary<TechType, ItemGroup>();
        }

        public bool Contains(TechType techType)
        {
            return _items.ContainsKey(techType);
        }

        public bool Contains(InventoryItem item)
        {
            return Contains(item.item.GetTechType());
        }

        public void UnsafeAdd(InventoryItem item)
        {
            TechType techType = item.item.GetTechType();
            ItemGroup itemGroup;
            if (_items.TryGetValue(techType, out itemGroup))
            {
                itemGroup.items.Add(item);
            }
            else
            {
                Vector2int itemSize = CraftData.GetItemSize(techType);
                itemGroup = new ItemGroup((int)techType, itemSize.x, itemSize.y);
                itemGroup.items.Add(item);
                _items.Add(techType, itemGroup);
            }

            QuickLogger.Debug($"ABOUT TO REPARENT {_storageRoot.name}",true);
            if (item.item.gameObject.activeSelf)
            {
                item.item.gameObject.SetActive(false);
            }
            item.item.SetVisible(false);
            item.item.Reparent(_storageRoot.transform);
            item.item.onTechTypeChanged += UpdateItemTechType;
            _count += 1;
            _unsorted = true;
            NotifyAddItem(item);
        }

        public Pickupable RemoveItem(TechType techType)
        {
            if (!_items.TryGetValue(techType, out var itemGroup))
            {
                return null;
            }
            List<InventoryItem> items = itemGroup.items;
            int index = items.Count - 1;
            InventoryItem inventoryItem = items[index];
            Pickupable item = inventoryItem.item;

            items.RemoveAt(index);
            if (items.Count == 0)
            {
                _items.Remove(techType);
            }
            inventoryItem.container = null;
            item.onTechTypeChanged -= UpdateItemTechType;
            _count -= 1;
            _unsorted = true;
            NotifyRemoveItem(inventoryItem);
            return item;
        }
        
        public bool DestroyItem(TechType techType)
        {
            Pickupable pickupable = RemoveItem(techType);
            if (pickupable == null)
            {
                return false;
            }
            Destroy(pickupable.gameObject);
            return true;
        }

        private void NotifyAddItem(InventoryItem item)
        {
            onAddItem?.Invoke(item);
        }
        
        private void NotifyRemoveItem(InventoryItem item)
        {
            onRemoveItem?.Invoke(item);
        }

        private void UpdateItemTechType(Pickupable pickupable, TechType oldTechType)
        {
            TechType techType = pickupable.GetTechType();
            if (techType == oldTechType)
            {
                return;
            }
            ItemGroup itemGroup;
            if (_items.TryGetValue(oldTechType, out itemGroup))
            {
                List<InventoryItem> items = itemGroup.items;
                for (int i = 0; i < items.Count; i++)
                {
                    InventoryItem inventoryItem = items[i];
                    if (inventoryItem.item == pickupable)
                    {
                        items.RemoveAt(i);
                        if (items.Count == 0)
                        {
                            _items.Remove(oldTechType);
                        }
                        if (_items.TryGetValue(techType, out itemGroup))
                        {
                            items = itemGroup.items;
                            items.Add(inventoryItem);
                        }
                        else
                        {
                            Vector2int itemSize = CraftData.GetItemSize(techType);
                            itemGroup = new ItemGroup((int)techType, itemSize.x, itemSize.y);
                            itemGroup.items.Add(inventoryItem);
                            _items.Add(techType, itemGroup);
                        }
                        _unsorted = true;
                        return;
                    }
                }
            }
        }
        
        private class ItemGroup
        {
            public ItemGroup(int id, int width, int height)
            {
                this.id = id;
                this.width = width;
                this.height = height;
                items = new List<InventoryItem>();
            }
            public void SetGhostDims(int width, int height)
            {
                this.width = width;
                this.height = height;
            }
            public int id;
            public int width;
            public int height;
            public List<InventoryItem> items;
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            QuickLogger.ModMessage($"Getting Storage Bytes: {serializer}");
            if (serializer == null || _storageRoot == null)
            {
                QuickLogger.DebugError($"Failed to save: Serializer: {serializer} || Root {_storageRoot?.name}",true);
                return null;
            }

            _storageRootBytes = StorageHelper.Save(serializer, _storageRoot);
            QuickLogger.ModMessage($"Storage Bytes Returned: {_storageRootBytes.Length}");
            return _storageRootBytes;
        }

        public bool Contains(Pickupable pickupable)
        {
            TechType techType = pickupable.GetTechType();
            ItemGroup itemGroup;
            if (!this._items.TryGetValue(techType, out itemGroup))
            {
                return false;
            }
            List<InventoryItem> items = itemGroup.items;
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem inventoryItem = items[i];
                if (pickupable == inventoryItem.item)
                {
                    return true;
                }
            }
            return false;
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
                Destroy(gObj);
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
            Transform hostTransform = transform;
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
                    Destroy(storeInformationIdentifier.gameObject);
                    QuickLogger.Debug($"Destroyed Duplicate", true);
                }
                num = i;
            }
        }

        public int GetCount(TechType techType)
        {
            return !_items.TryGetValue(techType, out var itemGroup) ? 0 : itemGroup.items.Count;
        }

        public int GetCount()
        {
            int i = 0;
            foreach (KeyValuePair<TechType, ItemGroup> item in _items)
            {
                foreach (var invItem in item.Value.items)
                {
                    i++;
                }
            }

            return i;
        }

        public IList<InventoryItem> GetItems(TechType techType)
        {
            return !_items.TryGetValue(techType, out var itemGroup) ? null : itemGroup.items.AsReadOnly();
        }

        public Dictionary<TechType,int> GetItems()
        {
            //TODO get items
            var keys =  new List<TechType>(_items.Keys);
            var lookup = keys?.Where(x => x != TechType.None).ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => GetCount(count.Key));
        }

        public bool RemoveItem(InventoryItem item, bool forced, bool verbose)
        {
            Pickupable item2 = item.item;
            if (!forced && !((IItemsContainer)this).AllowedToRemove(item2, verbose))
            {
                return false;
            }
            TechType techType = item2.GetTechType();
            ItemGroup itemGroup;
            if (this._items.TryGetValue(techType, out itemGroup))
            {
                List<InventoryItem> items = itemGroup.items;
                if (items.Remove(item))
                {
                    if (items.Count == 0)
                    {
                        _items.Remove(techType);
                    }
                    
                    item.container = this;
                    item.item.onTechTypeChanged -= this.UpdateItemTechType;
                    _count -= 1;
                    _unsorted = true;
                    NotifyRemoveItem(item);
                    return true;
                }
            }
            return false;
        }

        public bool AddItem(InventoryItem item)
        { 
            UnsafeAdd(item);
          return true;
        }

        public bool AllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return HasRoomFor(pickupable,null);
        }

        public bool AllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public void UpdateContainer()
        {
            
        }

        public bool HasRoomFor(Pickupable pickupable, InventoryItem ignore)
        {
            return GetCount() + 1 <= _slots;
        }

        public IEnumerator<InventoryItem> GetEnumerator()
        {
            return null;
        }

        public string label => "FCSStorage";
    }


}
