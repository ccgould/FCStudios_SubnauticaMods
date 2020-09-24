using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Structs;
using Discord;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using rail;

namespace DataStorageSolutions.Model
{
    internal class BaseStorageManager : IFCSStorage
    {
        internal readonly Dictionary<TechType, int> TrackedItems = new Dictionary<TechType, int>();
        internal readonly HashSet<DSSRackController> BaseRacks = new HashSet<DSSRackController>();
        public Action<int, int> OnContainerUpdate { get; set; }
        public DumpContainer DumpContainer { get; private set; }
        internal static readonly HashSet<DSSServerController> GlobalServers = new HashSet<DSSServerController>();
        internal static readonly HashSet<DSSServerController> BaseServers = new HashSet<DSSServerController>();
        //internal static readonly HashSet<StorageLo> BaseStorageLockers = new HashSet<DSSServerController>();
        private BaseManager _baseManager;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }


        #region Item Tracker Methods

        internal void AddToTrackedItems(TechType techType)
        {
            if (TrackedItems.ContainsKey(techType))
            {
                TrackedItems[techType] += 1;
#if DEBUG
                QuickLogger.Debug($"Added another {techType}", true);
#endif
            }
            else
            {
                TrackedItems.Add(techType, 1);
                BaseManager.UpdateGlobalTerminals();

#if DEBUG
                QuickLogger.Debug($"Item {techType} was added to the tracking list.", true);
#endif
            }
        }

        internal void RemoveFromTrackedItems(TechType techType)
        {
            if (!TrackedItems.ContainsKey(techType)) return;

            if (TrackedItems[techType] == 1)
            {
                TrackedItems.Remove(techType);
                BaseManager.UpdateGlobalTerminals();
#if DEBUG
                QuickLogger.Debug($"Removed another {techType}", true);
#endif
            }
            else
            {
                TrackedItems[techType] -= 1;
#if DEBUG
                QuickLogger.Debug($"Item {techType} is now {TrackedItems[techType]}", true);
#endif
            }
        }

        #endregion

        private void AddItemToBase(InventoryItem item)
        {

        }

        internal void Initialize(BaseManager manager)
        {
            _baseManager = manager;
            if (DumpContainer == null)
            {
                DumpContainer = manager.Habitat.gameObject.EnsureComponent<DumpContainer>();
                DumpContainer.Initialize(manager.Habitat.transform,AuxPatchers.BaseDumpReceptacle(),AuxPatchers.NotAllowed(),AuxPatchers.CannotBeStored(),this);
            }
        }

        internal static void CleanServers()
        {
            //var keysToRemove = Servers.Keys.Except(TrackedServers).ToList();

            //foreach (var key in keysToRemove)
            //    Servers.Remove(key);
        }

        public static IEnumerable<ServerData> GetServersSaveData()
        {
            foreach (DSSServerController baseServer in GlobalServers)
            {
                yield return new ServerData
                {
                    PrefabID = baseServer.GetPrefabID(),
                    ServerFilters = baseServer.GetFilters(),
                    SlotID = baseServer.GetSlotID()
                };
            }
        }

        public static int FindSlotId(string prefabId)
        {
            return Mod.GetServerSaveData(prefabId).SlotID;
        }

        public Pickupable RemoveItemFromBase(TechType techType,bool givePlayerItem =true)
        {
            var itemSize = CraftData.GetItemSize(techType);
            if (!Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                QuickLogger.ModMessage(string.Format(Language.main.Get("InventoryOverflow"), Language.main.Get(techType)));
                return null;
            }
            var pickup = GetPickupableFromRack(techType);
            if (pickup == null)
            {
                QuickLogger.Debug("Pickup returned null",true);
                return null;
            }

            if(givePlayerItem)
                DSSHelpers.GivePlayerItem(pickup);

            return pickup;
        }
        
        private Pickupable GetPickupableFromRack(TechType techType)
        {
            foreach (DSSRackController baseRack in BaseRacks)
            {
                if (baseRack.ContainsItem(techType))
                {
                    return baseRack.RemoveItemFromContainer(techType, 1);
                }
            }

            return null;
        }

        public int GetItemCount(TechType techType)
        {
            return TrackedItems.ContainsKey(techType) ? TrackedItems[techType] : 0;
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            //Check if it can hold
            if (amount + GetBaseTotal() > GetBaseMaximumStorage()) return false;

            var isItemAllowed = CheckIfItemAllowed(amount, techType);
            
            return isItemAllowed;
        }

        private static bool CheckIfItemAllowed(int amount, TechType techType)
        {
            //Check all filtered drives first to see if it can be stored
            foreach (DSSServerController server in BaseServers)
            {
                if (server.HasFilters() && server.CanBeStored(amount, techType))
                {
                    return true;
                }
            }

            //Check all unfiltered drives to see if it can be stored
            foreach (DSSServerController server in BaseServers)
            {
                if (!server.HasFilters() && server.CanBeStored(amount, techType))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType());
        }

        private int GetBaseTotal()
        {
            return BaseServers.Sum(x => x.GetTotal());
        }
        
        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            foreach (DSSRackController baseRack in BaseRacks)
            {
                if (baseRack.HasFilters() && baseRack.CanBeStored(1, item.item.GetTechType()))
                {
                    baseRack.AddItemToAServer(item);
                    return true;
                }
            }

            foreach (DSSRackController baseRack in BaseRacks)
            {
                if (!baseRack.HasFilters() && baseRack.CanBeStored(1, item.item.GetTechType()))
                {
                    baseRack.AddItemToAServer(item);
                    return true;
                }
            }

            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return RemoveItemFromBase(techType, false);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return TrackedItems;
        }
        
        public bool ContainsItem(TechType techType)
        {
            return TrackedItems.ContainsKey(techType);
        }

        public string GetTotalString()
        {
            return $"{GetBaseTotal()} / {GetBaseMaximumStorage()}";
        }

        private int GetBaseMaximumStorage()
        {
            return BaseServers?.Count * QPatch.Configuration.Config.ServerStorageLimit ?? 0;
        }

        public void RemoveServerFromBase(DSSServerController server)
        {
            BaseServers.Remove(server);
        }

        public void RegisterServerInBase(DSSServerController server)
        {
            if(!BaseServers.Contains(server))
                BaseServers.Add(server);
        }

        public void OpenDump(TransferData currentData)
        {
            currentData.Manager.StorageManager.DumpContainer.OpenStorage();
        }
    }
}
