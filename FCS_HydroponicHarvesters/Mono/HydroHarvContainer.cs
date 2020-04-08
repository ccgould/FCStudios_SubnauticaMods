using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvContainer : MonoBehaviour, IFCSStorage
    {
        private HydroHarvController _mono;
        private const int storageLimit = 50;
        internal Action OnContainerUpdate { get; set; }

        public int GetContainerFreeSpace => GetFreeSpace();
        public bool IsFull => CheckIfFull();
        //internal List<TechType>Items = new List<TechType>();
        internal Dictionary<TechType,int> Items = new Dictionary<TechType, int>();
        
        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
        }
        
        private bool CheckIfFull()
        {
            return GetTotal() > storageLimit;
        }

        private int GetTotal()
        {
            int amount = 0;
            foreach (KeyValuePair<TechType, int> item in Items)
            {
                amount += item.Value;
            }

            return amount;
        }

        private int GetFreeSpace()
        {
            int amount = 0;

            foreach (KeyValuePair<TechType, int> item in Items)
            {
                amount += item.Value;
            }

            return storageLimit - amount;
        }

        public bool CanBeStored(int amount)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _mono.HydroHarvGrowBed.AddItemToContainer(item);
            return true;
        }

        public void AddItemToContainer(TechType item,bool initializer = false)
        {
            if (!IsFull)
            {
                if (Items.ContainsKey(item))
                {
                    Items[item] += 1;
                }
                else
                {
                    Items.Add(item, initializer ? 0 : 1);
                }
                OnContainerUpdate?.Invoke();
            }
        }

        public void RemoveItemFromContainer(TechType item)
        {
            if (Items.ContainsKey(item))
            {
                if (Items[item] > 0)
                {
                    Items[item] -= 1;
                }
                OnContainerUpdate?.Invoke();
            }
        }

        public void DeleteItemFromContainer(TechType item)
        {
            if (Items.ContainsKey(item))
            {
                Items.Remove(item);
                _mono.HydroHarvGrowBed.RemoveDNA(item);
                OnContainerUpdate?.Invoke();
            }
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public void SpawnClone()
        {
           var samples = _mono.HydroHarvGrowBed.GetDNASamples();

           foreach (TechType sample in samples)
           {
               AddItemToContainer(sample);
           }
        }
    }
}
