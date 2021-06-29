using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashRecycler.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.TrashReceptacle.Mono
{
    internal class TrashStorage: MonoBehaviour, IFCSStorage
    {
        private FcsDevice _mono;
        private DumpContainer _dumpContainer;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; } = false;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        internal void Initialize(FcsDevice mono, DumpContainer dumpContainer)
        {
            _mono = mono;
            _dumpContainer = dumpContainer;
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            foreach (var validRecycler in GetRecyclers())
            {
                QuickLogger.Debug($"Checking Recycler {validRecycler.UnitID} | Free Space: {validRecycler.GetFreeSpace()}",true);
                if (validRecycler.GetRecycler().CanPendItem(item.item))
                {
                    validRecycler.GetRecycler().AddItem(item);
                    validRecycler.TryStartRecycling();
                    QuickLogger.Debug($"Allowed to add: {Language.main.Get(item.item.GetTechType())} | Recycler: {validRecycler.UnitID}", true);

                    return true;
                }
            }

            PlayerInteractionHelper.GivePlayerItem(item);
            QuickLogger.ModMessage(AuxPatchers.FailedToRecycleItemFormat(Language.main.Get(item.item.GetTechType())));

            return true;
        }

        private TrashRecyclerController FindRecycler()
        {
            var recyclers = _mono.Manager.GetDevices(Mod.RecyclerTabID).ToArray();
            
            foreach (FcsDevice device in recyclers)
            {
                var trashRecycler = (TrashRecyclerController) device;
                if (device.IsOperational)
                {
                    return trashRecycler;
                }
            }

            return null;
        }

        private IEnumerable<TrashRecyclerController> GetRecyclers()
        {
            var recyclers = _mono.Manager.GetDevices(Mod.RecyclerTabID).ToArray();

            foreach (FcsDevice device in recyclers)
            {
                var trashRecycler = (TrashRecyclerController)device;
                if (device.IsOperational)
                {
                   yield return trashRecycler;
                }
            }
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            //Get the total space allowed
            var totalRecyclerSpace = GetTotal();
            var dumpContainerTotal = GetDumpContainerTotal();
            var itemIngredientCount = TechDataHelpers.GetIngredientCount(pickupable);

            QuickLogger.Debug($"[Trash Receptical] Total Recycler Space {totalRecyclerSpace} | Dump Container Total: {dumpContainerTotal} | Item Ingredient Count: {itemIngredientCount}",true);

            return dumpContainerTotal + itemIngredientCount <= totalRecyclerSpace;
        }

        private int GetDumpContainerTotal()
        {
            int amount = 0;
            foreach (InventoryItem item in _dumpContainer.GetItemsContainer())
            {
                amount += TechDataHelpers.GetIngredientCount(item.item);
            }

            return amount;
        }

        private int GetTotal()
        {
            int amount = 0;
            var recyclers = _mono.Manager.GetDevices(Mod.RecyclerTabID).ToArray();

            foreach (FcsDevice device in recyclers)
            {
                var trashRecycler = (TrashRecyclerController)device;
                if (device.IsOperational)
                {
                    amount += trashRecycler.GetFreeSpace();
                }
            }

            return amount;
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return GetTotal();
        }
    }
}