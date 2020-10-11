using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Utilities;
using System.Linq;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCSTechFabricator.Objects
{
    public class FCSFilteredStorage : IFCSStorage
    {
        private ItemsContainer _container;
        private Action _updateDisplay;
        private GameObject _root;
        private int _storageLimit;
        private FCSConnectableDevice _fcsConnectableDevice;

        public int GetContainerFreeSpace => _storageLimit - _container.count;
        public bool IsFull => _container.count >= _storageLimit;
        public HashSet<Filter> Filters { get; set; } = new HashSet<Filter>();

        public void Initialize(FCSConnectableDevice fcsConnectable, GameObject root, Action updateDisplay,int storageLimit)
        {
            _root = root;
            _fcsConnectableDevice = fcsConnectable;
            _container = new ItemsContainer(1, storageLimit,root.transform ,"Filtered Storage",null);
            _container.onAddItem += this.OnAddItem;
            _container.onRemoveItem += this.OnRemoveItem;
            _storageLimit = storageLimit;
            _updateDisplay = updateDisplay;
        }

        private void OnRemoveItem(InventoryItem item)
        {

        }

        private void OnAddItem(InventoryItem item)
        {
            if (item.item.gameObject.activeSelf)
            {
                item.item.gameObject.SetActive(false);
            }
        }

        public string FormatFiltersData()
        {
            var sb = new StringBuilder();

            foreach (Filter filter in Filters)
            {
                sb.Append($"{filter.GetString()},");
            }

            return sb.ToString();
        }

        public void ForceUpdateDisplay()
        {
            _updateDisplay?.Invoke();
        }

        public void TrackItem(Pickupable pickupable)
        {
            if(_container.Contains(pickupable)) return;

            _container.AddItem(pickupable);
        }
        
        public bool CanBeStored(int amount, TechType techType)
        {
            if (IsFull) return false;

            if (HasFilters())
            {
                var filterPass = false;
                foreach (Filter filter in Filters)
                {
                    if (filter.IsTechTypeAllowed(techType))
                    {
                        filterPass = true;
                    }
                }

                return filterPass && amount <= _storageLimit;
            }

            return amount <= _storageLimit;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            if (!CanBeStored(_container.count + 1, item.item.GetTechType())) return false;

            _container.UnsafeAdd(item);
            OnContainerAddItem?.Invoke(_fcsConnectableDevice, item.item.GetTechType());
            item.item.Reparent(_root.transform);
            ForceUpdateDisplay();
            return true;

        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            var item = _container.FirstOrDefault(x => x.item.GetTechType() == techType);
            
            if (item == null) return null;
            _container.RemoveItem(item.item);
            OnContainerRemoveItem?.Invoke(_fcsConnectableDevice,techType);
            ForceUpdateDisplay();
            return item.item;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        { 
            //TODO return a Dictionary<TechType, int> that is filled on item additon instead of using the ToLookup just a thought to improve performance.
            var lookup = _container?.Where(x => x != null).ToLookup(x => x.item.GetTechType()).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
        }

        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerRemoveItem { get; set; }

        public bool ContainsItem(TechType techType)
        {
            return _container.Any(x => x.item.GetTechType() == techType);
        }

        public bool HasFilters()
        {
           return Filters.Count > 0;
        }

        public bool HasItem(TechType techType)
        {
            return _container.Any(x => x.item.GetTechType() == techType);
        }

        public IEnumerable<string> GetItemsPrefabID()
        {
            foreach (InventoryItem inventoryItem in _container)
            {
                yield return inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }
        }

        public ItemsContainer GetContainer()
        {
            return _container;
        }

        public void Clear()
        {
            _container.Clear();
        }

        public int GetItemCount(TechType item)
        {
           return _container.GetCount(item);
        }
    }
}
