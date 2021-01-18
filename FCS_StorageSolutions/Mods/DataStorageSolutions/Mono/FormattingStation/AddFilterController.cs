using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation
{
    internal class AddFilterController : IFCSDumpContainer
    {
        private HashSet<Filter> _knownFilters = new HashSet<Filter>();
        internal Action<InventoryItem> ItemAdded { get; set; }

        public bool AddItemToContainer(InventoryItem item)
        {
            ItemAdded?.Invoke(item);
            return true;
        }

        internal void ResetFilters(HashSet<Filter> filters)
        {
            if (filters == null) return;
            _knownFilters = filters;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            foreach (Filter filter in _knownFilters)
            {
                if (filter.HasTechType(techType))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return IsAllowedToAdd(pickupable.GetTechType(), verbose);
        }
    }
}