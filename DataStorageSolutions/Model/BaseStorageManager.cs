using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Patches;
using DataStorageSolutions.Structs;
using Discord;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using rail;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataStorageSolutions.Model
{
    internal class BaseStorageManager : IFCSStorage
    {
        public class TrackedResource
        {
            public TechType TechType { get; set; }
            public int Amount { get; set; }
            public HashSet<StorageContainer> StorageContainers { get; set; } = new HashSet<StorageContainer>();
            public HashSet<DSSServerController> Servers { get; set; } = new HashSet<DSSServerController>();
            public HashSet<FCSConnectableDevice> FCSConnectableDevices { get; set; } = new HashSet<FCSConnectableDevice>();
        }

        internal readonly Dictionary<TechType, TrackedResource> TrackedResources = new Dictionary<TechType, TrackedResource>();
        internal readonly HashSet<DSSRackController> BaseRacks = new HashSet<DSSRackController>();
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerRemoveItem { get; set; }
        public DumpContainer DumpContainer { get; private set; }
        internal static readonly HashSet<DSSServerController> GlobalServers = new HashSet<DSSServerController>();
        internal readonly HashSet<DSSServerController> BaseServers = new HashSet<DSSServerController>();
        public static List<string> DONT_TRACK_GAMEOBJECTS { get; private set; } = new List<string>
        {
            "planterpot",
            "planterbox",
            "plantershelf",
            "alongplanter"
        };  
        internal static readonly HashSet<StorageContainer> BaseStorageLockers = new HashSet<StorageContainer>();
        private BaseManager _baseManager;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }


        #region Item Tracker Methods

        internal void AddItemsToTracker(DSSServerController server, TechType item, int amountToAdd = 1)
        {
            QuickLogger.Debug($"AddItemsToTracker: DSSServerController || {item.AsString()} || {amountToAdd} ");

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                TrackedResources[item].Servers.Add(server);
            }
            else
            {
                TrackedResources.Add(item, new TrackedResource()
                {
                    TechType = item,
                    Amount = amountToAdd,
                    Servers = new HashSet<DSSServerController>() { server }
                });

                BaseManager.SendNotification();
            }
        }

        internal void AddItemsToTracker(StorageContainer sc, TechType item, int amountToAdd = 1)
        {
            QuickLogger.Debug($"AddItemsToTracker: StorageContainer || {item.AsString()} || {amountToAdd} ");


            if (DONT_TRACK_GAMEOBJECTS.Contains(item.AsString().ToLower()))
            {
                return;
            }

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                TrackedResources[item].StorageContainers.Add(sc);
            }
            else
            {
                TrackedResources.Add(item, new TrackedResource()
                {
                    TechType = item,
                    Amount = amountToAdd,
                    StorageContainers = new HashSet<StorageContainer>()
                    {
                        sc
                    }
                });
                BaseManager.SendNotification();
            }
        }

        internal void AddItemsToTracker(FCSConnectableDevice device, TechType item, int amountToAdd = 1)
        {
            QuickLogger.Debug($"AddItemsToTracker: FCSConnectableDevice || {item.AsString()} || {amountToAdd} ");
            if (TrackedResources.ContainsKey(item))
            {
                TrackedResources[item].Amount += amountToAdd;
                TrackedResources[item].FCSConnectableDevices.Add(device);
            }
            else
            {
                TrackedResources.Add(item, new TrackedResource()
                {
                    TechType = item,
                    Amount = amountToAdd,
                    FCSConnectableDevices = new HashSet<FCSConnectableDevice>() { device }
                });

                BaseManager.SendNotification();
            }
        }

        internal void RemoveItemsFromTracker(DSSServerController server, TechType item, int amountToRemove = 1)
        {
            QuickLogger.Debug($"RemoveItemsFromTracker: DSSServerController || {item.AsString()} || {amountToRemove} ");


            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    TrackedResources.Remove(item);
                    BaseManager.SendNotification();
                }
                else
                {
                    int amountLeftInContainer = server.GetItemCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.Servers.Remove(server);
                    }
                }
            }
        }

        internal void RemoveItemsFromTracker(FCSConnectableDevice sc, TechType item, int amountToRemove = 1)
        {
            QuickLogger.Debug($"RemoveItemsFromTracker: FCSConnectableDevice || {item.AsString()} || {amountToRemove} ");


            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    TrackedResources.Remove(item);
                    BaseManager.SendNotification();
                }
                else
                {
                    int amountLeftInContainer = sc.GetItemCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.FCSConnectableDevices.Remove(sc);
                    }
                }
            }
        }

        internal void RemoveItemsFromTracker(StorageContainer sc, TechType item, int amountToRemove = 1)
        {
            QuickLogger.Debug($"RemoveItemsFromTracker: StorageContainer || {item.AsString()} || {amountToRemove} ");

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    TrackedResources.Remove(item);
                    BaseManager.SendNotification();
                }
                else
                {
                    int amountLeftInContainer = sc.container.GetCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.StorageContainers.Remove(sc);
                    }
                }
            }
        }

        #endregion

        internal void Initialize(BaseManager manager)
        {
            _baseManager = manager;
            if (DumpContainer == null)
            {
                DumpContainer = manager.Habitat.gameObject.EnsureComponent<DumpContainer>();
                DumpContainer.Initialize(manager.Habitat.transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.CannotBeStored(), this);
            }
            TrackExistingStorageContainers();
        }

        internal static void CleanServers()
        {
            //var keysToRemove = Servers.Keys.Except(TrackedServers).ToList();

            //foreach (var key in keysToRemove)
            //    Servers.Remove(key);
        }

        public static IEnumerable<ServerData> GetServersSaveData(ProtobufSerializer serializer)
        {
            foreach (DSSServerController baseServer in GlobalServers)
            {
                yield return new ServerData
                {
                    PrefabID = baseServer.GetPrefabID(),
                    ServerFilters = baseServer.GetFilters(),
                    SlotID = baseServer.GetSlotID(),
                    ServerItems = baseServer.GetItemsPrefabID(),
                    Bytes = baseServer.GetStorageBytes(serializer)
                };
            }
        }

        public static int FindSlotId(string prefabId)
        {
            return Mod.GetServerSaveData(prefabId).SlotID;
        }

        internal int GetItemCount(TechType techType,bool serverOnly = false)
        {
            if (!TrackedResources.ContainsKey(techType)) return 0;

            if (!serverOnly)
            {
                return TrackedResources.ContainsKey(techType) ? TrackedResources[techType].Amount : 0;
            }
            
            var i = 0;
            var device = TrackedResources[techType];

            foreach (DSSServerController controller in device.Servers)
            {
                i += controller.GetItemCount(techType);
            }

            return i;
        }
 

        public bool CanBeStored(int amount, TechType techType)
        {
            //Check if it can hold
            if (amount + GetBaseTotal() > GetBaseMaximumStorage()) return false;

            var isItemAllowed = CheckIfItemAllowed(amount, techType);

            return isItemAllowed;
        }

        private bool CheckIfItemAllowed(int amount, TechType techType)
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

        private int GetBaseTotal()
        {
            return BaseServers.Sum(x => x.GetTotal());
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var techType = item.item.GetTechType();

            QuickLogger.Debug($"Trying to add item {techType.AsString()}",true);

            foreach (DSSServerController baseServer in BaseServers)
            {
                if (!TryAddItemToServer(item, baseServer, techType)) continue;
                return true;
            }

            foreach (DSSServerController baseServer in BaseServers)
            {
                if (!TryAddItemToServer(item, baseServer, techType, false)) continue;
                return true;
            }

            //Inventory.main.Pickup(item.item);

            return false;
        }

        private static bool TryAddItemToServer(InventoryItem item, DSSServerController baseServer, TechType techType, bool filterCheck = true)
        {
            if (filterCheck)
            {
                if (!baseServer.HasFilters()) return false;
            }
            else
            {
                if (baseServer.HasFilters()) return false;
            }

            var result = baseServer.CanBeStored(1, techType);
            QuickLogger.Debug($"Can be stored result = {result}", true);
            if (!result) return false;
            baseServer.AddItemToContainer(item);
            QuickLogger.Debug($"Adding to server = {baseServer.GetPrefabID()}", true);
            return true;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return TrackedResources.ToDictionary(x => x.Key, x => x.Value.Amount);
        }

        public bool ContainsItem(TechType techType)
        {
            return TrackedResources.ContainsKey(techType);
        }

        public bool ServersContainsItem(TechType techType)
        {
           return BaseServers.Any(x => x.ContainsItem(techType));
        }

        public string GetTotalString()
        {
            return $"{GetBaseTotal()} / {GetBaseMaximumStorage()}";
        }

        private int GetBaseMaximumStorage()
        {
            return BaseServers?.Count * QPatch.Configuration.Config.ServerStorageLimit ?? 0;
        }

        public void OpenDump(TransferData currentData)
        {
            currentData.Manager.StorageManager.DumpContainer.OpenStorage();
        }

        #region Base Operations

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType());
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return RemoveItemFromBase(techType, false);
        }

        public Pickupable RemoveItemFromBase(TechType techType, bool givePlayerItem = true, bool destroy = false)
        {
            var itemSize = CraftData.GetItemSize(techType);
            if (givePlayerItem && !Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                QuickLogger.ModMessage(string.Format(Language.main.Get("InventoryOverflow"), Language.main.Get(techType)));
                return null;
            }

            var pickup = GetItemFromStorage(techType);

            if (pickup == null)
            {
                QuickLogger.Debug("Pickup returned null", true);
                return null;
            }

            if (givePlayerItem)
                DSSHelpers.GivePlayerItem(pickup);

            if (!destroy) return pickup;
            Object.Destroy(pickup.gameObject);
            return null;

        }

        public Pickupable RemoveItemFromBaseServersOnly(TechType techType, bool givePlayerItem = true, bool destroy = false)
        {
            var itemSize = CraftData.GetItemSize(techType);
            if (!Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                QuickLogger.ModMessage(string.Format(Language.main.Get("InventoryOverflow"), Language.main.Get(techType)));
                return null;
            }

            var pickup = GetItemFromStorage(techType,true);

            if (pickup == null)
            {
                QuickLogger.Debug("Pickup returned null", true);
                return null;
            }

            if (givePlayerItem)
                DSSHelpers.GivePlayerItem(pickup);

            if (!destroy) return pickup;
            Object.Destroy(pickup.gameObject);
            return null;

        }

        private Pickupable GetItemFromStorage(TechType item,bool serverOnly = false)
        {
            if (!TrackedResources.ContainsKey(item)) return null;
            var trackedResource = TrackedResources[item];

            QuickLogger.Debug($"Tracked Resource: {trackedResource}", true);

            if (trackedResource.Servers.Count >= 1)
            {
                DSSServerController sc = trackedResource.Servers.ElementAt(0);
                if (sc.HasItem(item))
                {
                    return sc.RemoveItemFromContainer(item, 1);
                }
            }

            if (!serverOnly)
            {
                if (trackedResource.StorageContainers.Count >= 1)
                {
                    StorageContainer sc = trackedResource.StorageContainers.ElementAt(0);
                    if (sc.container.Contains(item))
                    {
                        return sc.container.RemoveItem(item);
                    }
                }

                if (trackedResource.FCSConnectableDevices.Count >= 1)
                {
                    FCSConnectableDevice sc = trackedResource.FCSConnectableDevices.ElementAt(0);
                    if (sc.ContainsItem(item))
                    {
                        return sc.RemoveItemFromContainer(item,1);
                    }
                }
            }

            return null;
        }
        
        public void RemoveServerFromBase(DSSServerController server)
        {
            BaseManager.SetAllowedToNotify(false);
            foreach (KeyValuePair<TechType, int> item in server.GetItemsWithin())
            {
                RemoveItemsFromTracker(server, item.Key, item.Value);
            }

            server.OnAddItem -= OnServerAddItem;
            server.OnRemoveItem -= OnServerRemoveItem;
            BaseServers.Remove(server);
            BaseManager.SetAllowedToNotify(true);
        }

        public void RegisterServerInBase(DSSServerController server)
        {
            if (!BaseServers.Contains(server))
            {
                BaseManager.SetAllowedToNotify(false);
                foreach (KeyValuePair<TechType, int> item in server.GetItemsWithin())
                {
                    AddItemsToTracker(server, item.Key, item.Value);
                }
                
                server.OnAddItem += OnServerAddItem;
                server.OnRemoveItem += OnServerRemoveItem;
                BaseServers.Add(server);
                BaseManager.SetAllowedToNotify(true);
            }
        }

        private void OnServerRemoveItem(DSSServerController server, InventoryItem item)
        {
            RemoveItemsFromTracker(server, item.item.GetTechType());
        }
        
        private void OnServerAddItem(DSSServerController server, InventoryItem item)
        {
            AddItemsToTracker(server, item.item.GetTechType());
        }

        #endregion

        #region Storage Containers

        public void AlertNewStorageContainerPlaced(StorageContainer storageContainer)
        {
            if (BaseStorageLockers.Contains(storageContainer)) return;
            BaseStorageLockers.Add(storageContainer);
            TrackStorageContainer(storageContainer);
        }

        private void TrackExistingStorageContainers()
        {
            StorageContainer[] containers = _baseManager.Habitat.GetComponentsInChildren<StorageContainer>();
            foreach (StorageContainer sc in containers)
            {
                TrackStorageContainer(sc);
            }
        }

        private void TrackStorageContainer(StorageContainer sc)
        {
            if (sc == null || sc.container == null)
            {
                return;
            }

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return;
                }
            }

            foreach (var item in sc.container.GetItemTypes())
            {
                for (int i = 0; i < sc.container.GetCount(item); i++)
                {
                    AddItemsToTracker(sc, item);
                }
            }

            sc.container.onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
            sc.container.onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
        }

        #endregion

        #region FCSConnectables

        public void RefreshFCSConnectable(FCSConnectableDevice device,bool isVisible)
        {
            var items = device.GetItemsWithin();
            if (isVisible)
            {
                foreach (KeyValuePair<TechType, int> item in items)
                {
                    AddItemsToTracker(device,item.Key,item.Value);
                }
            }
            else
            {
                foreach (KeyValuePair<TechType, int> item in items)
                {
                    RemoveItemsFromTracker(device,item.Key,item.Value);
                }
            }
        }

        #endregion

        public IEnumerable<ItemsContainer> GetItemContiners()
        {
            foreach (DSSServerController baseServer in BaseServers)
            {
                yield return baseServer.GetItemsContainer();
            }
        }
    }
}
