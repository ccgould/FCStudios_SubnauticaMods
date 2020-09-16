using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FCSTechFabricator.Objects
{
    public class FCSFilteredStorage : MonoBehaviour
    {
        private HashSet<ObjectData> _items = new HashSet<ObjectData>();
        private List<Filter> _filters = new List<Filter>();
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
            if (filters != null)
            {
                _filters = filters;
            }
            _updateDisplay = updateDisplay;
        }

        public string FormatFiltersData()
        {
            var sb = new StringBuilder();

            foreach (Filter filter in _filters)
            {
                sb.Append($"{filter.GetString()},");
            }

            return sb.ToString();
        }
        public void ForceUpdateDisplay()
        {
            _updateDisplay?.Invoke();
        }
    }
}
