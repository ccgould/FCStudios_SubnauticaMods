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
        private readonly HashSet<Pickupable> _container = new HashSet<Pickupable>();
        private Action _updateDisplay;
        private GameObject _root;
        private int _storageLimit;

        public int GetContainerFreeSpace => _storageLimit - _container.Count;
        public bool IsFull => _container.Count >= _storageLimit;
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
            if(_container.Contains(pickupable)) return;

            _container.Add(pickupable);
        }
        
        public bool CanBeStored(int amount, TechType techType)
        {
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

                return filterPass;
            }

            return amount <= _storageLimit;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            if (CanBeStored(_container.Count + 1,item.item.GetTechType()) && !_container.Contains(item.item))
            {
                _container.Add(item.item);
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
            var item = _container.FirstOrDefault(x => x.GetTechType() == techType);
            
            if (item == null) return null;
            _container.Remove(item);
            ForceUpdateDisplay();
            return item;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        { 
            //TODO return a Dictionary<TechType, int> that is filled on item additon instead of using the ToLookup just a thought to improve performance.
            var lookup = _container?.Where(x => x != null).ToLookup(x => x.GetTechType()).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
        }

        public Action<int, int> OnContainerUpdate { get; set; }

        public bool ContainsItem(TechType techType)
        {
            return _container.Any(x => x.GetTechType() == techType);
        }

        public bool HasFilters()
        {
           return Filters.Count > 0;
        }

        public bool HasItem(TechType techType)
        {
            return _container.Any(x => x.GetTechType() == techType);
        }

        public IEnumerable<string> GetItemsPrefabID()
        {
            foreach (Pickupable pickupable in _container)
            {
                yield return pickupable.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }
        }
    }
}
