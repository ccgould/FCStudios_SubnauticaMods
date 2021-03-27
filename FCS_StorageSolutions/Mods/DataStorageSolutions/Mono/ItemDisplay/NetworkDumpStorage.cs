using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.ItemDisplay
{
    internal class NetworkDumpStorage : MonoBehaviour, IFCSDumpContainer
    {
        private DumpContainerSimplified _dumpContainer;
        private BaseManager _manager;

        internal void Initialize(BaseManager manager)
        {
            _manager = manager;
            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform, AuxPatchers.AddItemToItemDisplay(), this, 6, 8, "NetworkItemDisplayNetworkDump");
            }
        }
        
        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            if (_manager == null) return false;
            int availableSpace = 0;
            foreach (IDSSRack baseRack in _manager.BaseRacks)
            {
                availableSpace += baseRack.GetFreeSpace();
            }

            var result = availableSpace >= _dumpContainer.GetItemCount() + 1;
            return result;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return IsAllowedToAdd(pickupable.GetTechType(), verbose);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                var result = BaseManager.AddItemToNetwork(item,_manager);
                if (!result)
                {
                    PlayerInteractionHelper.GivePlayerItem(item);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message, true);
                QuickLogger.Debug(e.StackTrace);
                PlayerInteractionHelper.GivePlayerItem(item);
                return false;
            }
            return true;
        }

        public void OpenStorage()
        {
            _dumpContainer.OpenStorage();
        }
    }
}