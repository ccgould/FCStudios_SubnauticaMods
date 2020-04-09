using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvCleanerManager : MonoBehaviour, IFCSStorage
    {
        private HydroHarvController _mono;

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
        }

        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }
    }
}
