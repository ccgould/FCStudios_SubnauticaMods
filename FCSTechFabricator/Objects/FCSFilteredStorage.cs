using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTechFabricator.Objects
{
    public class FCSFilteredStorage : MonoBehaviour
    {
        private HashSet<ObjectData> _items;
        private List<Filter> _filters;
        private Action _updateDisplay;

        public Action<HashSet<ObjectData>> OnItemsUpdate { get; set; }
        public Action<List<Filter>> OnFiltersUpdate { get; set; }
        
        public HashSet<ObjectData> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnItemsUpdate?.Invoke(_items);
            }
        }

        public List<Filter> Filters
        {
            get => _filters;
            set
            {
                _filters = value;
                OnFiltersUpdate?.Invoke(_filters);
            }
        }

        public void Initialize(List<Filter> filters, Action updateDisplay)
        {
            _filters = filters;
            _updateDisplay = updateDisplay;
        }

        public void ForceUpdateDisplay()
        {
            _updateDisplay?.Invoke();
        }
    }
}
