using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Utilities;
using System.Linq;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCSTechFabricator.Objects
{
    public class FCSFilteredStorage : IFCSStorage
    {
        private readonly HashSet<InventoryItem> _container = new HashSet<InventoryItem>();
        private Action _updateDisplay;
        private GameObject _root;
        private int _storageLimit;

        public HashSet<Filter> Filters { get; set; } = new HashSet<Filter>();

        public void Initialize(GameObject root, Action updateDisplay,int storageLimit)
        {
            _root = root;
            _storageLimit = storageLimit;
            _updateDisplay = updateDisplay;
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
            _container.Add(new InventoryItem(pickupable));
        }
        
        public int GetContainerFreeSpace => _storageLimit - _container.Count;
        public bool IsFull => _container.Count >= _storageLimit;

        public bool CanBeStored(int amount, TechType techType)
        {
            return amount + 1 <= _storageLimit;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            if (CanBeStored(_container.Count,item.item.GetTechType()) && !_container.Contains(item))
            {
                _container.Add(item);
                item.item.Reparent(_root.transform);
                ForceUpdateDisplay();
                return true;
            }

            return false;
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
            _container.Remove(item);
            ForceUpdateDisplay();
            return item.item;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            var lookup = _container?.ToLookup(x => x.item.GetTechType()).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
        }

        public Action<int, int> OnContainerUpdate { get; set; }

        public bool ContainsItem(TechType techType)
        {
            return _container.Any(x => x.item.GetTechType() == techType);
        }
    }
}
