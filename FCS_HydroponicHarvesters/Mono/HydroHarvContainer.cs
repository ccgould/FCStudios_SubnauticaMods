using System;
using System.Collections.Generic;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Enumerators;
using FCS_HydroponicHarvesters.Model;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvContainer : MonoBehaviour, IFCSStorage
    {
        private HydroHarvController _mono;
        internal int StorageLimit { get; private set; }
        internal Action OnContainerUpdate { get; set; }

        public int GetContainerFreeSpace => GetFreeSpace();
        public bool IsFull => CheckIfFull();
        internal Dictionary<TechType,int> Items = new Dictionary<TechType, int>();
        
        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;

            switch (mono.HydroHarvGrowBed.GetHydroHarvSize())
            {
                case HydroHarvSize.Unknown:
                    StorageLimit = 0;
                    break;
                case HydroHarvSize.Large:
                    StorageLimit = QPatch.Configuration.Config.LargeStorageLimit;
                    break;
                case HydroHarvSize.Medium:
                    StorageLimit = QPatch.Configuration.Config.MediumStorageLimit;
                    break;
                case HydroHarvSize.Small:
                    StorageLimit = QPatch.Configuration.Config.SmallStorageLimit;
                    break;
                default:
                    StorageLimit = 0;
                    break;
            }
        }
        
        private bool CheckIfFull()
        {
            return GetTotal() >= StorageLimit;
        }

        internal int GetTotal()
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

            return StorageLimit - amount;
        }

        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            //_mono.HydroHarvGrowBed.AddItemToContainer(item);
            return false;
        }

        internal void AddItemToContainer(TechType item,bool initializer = false)
        {
            QuickLogger.Debug("Adding to container", true);

            if (!IsFull)
            {
                QuickLogger.Debug($"Trying to add item to container {item}",true);

                if (Items.ContainsKey(item))
                {
                    Items[item] += 1;
                }
                else
                {
                    Items.Add(item, initializer ? 0 : 1);
                    QuickLogger.Debug($"Added item to container {item}",true);
                }
                OnContainerUpdate?.Invoke();
            }
        }

        internal void RemoveItemFromContainer(TechType item)
        {
            QuickLogger.Debug("Taking From Container",true);
            if (Items.ContainsKey(item))
            {
#if SUBNAUTICA
                var itemSize = CraftData.GetItemSize(item);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(item);
#endif
                if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
                {
                    if (Items[item] > 0)
                    {
                        Items[item] -= 1;
                        var pickup = CraftData.InstantiateFromPrefab(item).GetComponent<Pickupable>();
                        Inventory.main.Pickup(pickup);
                        OnContainerUpdate?.Invoke();
                        _mono?.Producer?.TryStartingNextClone();
                    }
                }
            }
        }

        internal void DeleteItemFromContainer(TechType item)
        {
            if (!Items.ContainsKey(item)) return;

            //TODO Add Message to confirm Delete;

            if (Items[item] > 0)
            {
                QuickLogger.Message(HydroponicHarvestersBuildable.CannotDeleteDNAItem(Language.main.Get(item)),true);
                return;
            }

            if (_mono.HydroHarvGrowBed.GetDnaCount(item) <= 1)
            {
                Items.Remove(item);
            }
            _mono.HydroHarvGrowBed.RemoveDNA(item);
            OnContainerUpdate?.Invoke();
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        internal void SpawnClone()
        {
           var samples = _mono.HydroHarvGrowBed.GetDNASamples();

           foreach (KeyValuePair<TechType, StoredDNAData> sample in samples)
           {
               for (int i = 0; i < sample.Value.Amount; i++)
               {
                   if (IsFull) break;
                    AddItemToContainer(sample.Key);
               }
           }
        }

        internal Dictionary<TechType, int> Save()
        {
            return Items;
        }

        internal void Load(Dictionary<TechType, int> savedDataContainer)
        {
            if(savedDataContainer == null) return;

            Items = savedDataContainer;

            OnContainerUpdate?.Invoke();
        }

        public bool HasItems()
        {
            return Items.Count > 0;
        }
    }
}
