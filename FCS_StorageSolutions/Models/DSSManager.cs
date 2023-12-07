using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Enumerator;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;
using FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HandReticle;

namespace FCS_StorageSolutions.Models;
internal class DSSManager : MonoBehaviour
{
    private Dictionary<RackBase,HashSet<FCSStorage>> dssRacks = new();
    private HashSet<DSSAntennaController> dssAntennas = new();
    //private HashSet<StorageContainer> storageContainers = new();

    private    readonly Dictionary<TechType, TrackedResource> trackedResources = new();

    internal Action OnServerAdded;
    internal Action OnServerRemoved;
    internal Action OnRackRemoved;
    internal Action<DSSManager, bool> OnNetworkConnectionChanged;
    private HabitatManager habitatManager;

    internal void Initialize(HabitatManager habitatManager)
    {
        this.habitatManager = habitatManager;

        QuickLogger.Info($"DSS Manager Initialized! Is habitat found:{habitatManager is not null}");
    }

    /// <summary>
    /// Checks if the base and rack conditions allow transferring
    /// </summary>
    /// <returns></returns>
    internal bool IsOperational()
    {
        return true;
    }

    internal bool IsVisible()
    {
        return dssAntennas.Any();
    }

    internal void RegisterAntenna(DSSAntennaController antenna)
    {
        dssAntennas.Add(antenna);
        if(dssAntennas.Count == 1)
        {
            OnNetworkConnectionChanged?.Invoke(this,true);
        }
    }

    internal void UnRegisterAntenna(DSSAntennaController antenna)
    {
        dssAntennas.Remove(antenna);
        if (dssAntennas.Count == 0)
        {
            OnNetworkConnectionChanged?.Invoke(this,false);
        }
    }

    internal void RegisterServer(RackBase parentRack, FCSStorage storage)
    {
        if (dssRacks.ContainsKey(parentRack))
        {
            dssRacks[parentRack].Add(storage);
        }
        else
        {
            dssRacks.Add(parentRack, new HashSet<FCSStorage>() { storage });
        }

        RegisterStorage(DSSServerSpawnable.PatchedTechType, storage.ItemsContainer);


        OnServerAdded?.Invoke();
    }

    internal void UnRegisterServer(RackBase parentRack,FCSStorage storage)
    {
        if(dssRacks.ContainsKey(parentRack))
        {
            dssRacks[parentRack].Remove(storage);
            UnRegisterStorage(DSSServerSpawnable.PatchedTechType, storage.ItemsContainer);
            OnServerRemoved?.Invoke();
        }
    }

    internal void RegisterStorage(TechType deviceTechType, ItemsContainer storage)
    {
        //storageContainers.Add(storage);


        foreach (var item in storage)
        {
            TrackResource(deviceTechType, item);
        }
        storage.onAddItem += StorageContainer_ItemAdded;
        storage.onRemoveItem += StorageContainer_ItemRemoved;
        OnServerAdded?.Invoke();
    }

    private void TrackResource(TechType deviceTechType, InventoryItem item)
    {
        var techType = item.item.GetTechType();

        if (trackedResources.TryGetValue(techType, out var resource))
        {
            resource.TrackItem(deviceTechType, item);
        }
        else
        {
            trackedResources.Add(techType, new TrackedResource(deviceTechType, item));
        }
    }

    private void UnTrackResource(TechType deviceTechType, InventoryItem item)
    {
        var techType = item.item.GetTechType();

        if (trackedResources.TryGetValue(techType, out var resource))
        {
            resource.UnTrackItem(deviceTechType, item);
            if(resource.GetCount(DSSTerminalFilterOptions.ShowAll, habitatManager.IsDssIntegration()) <= 0)
            {
                trackedResources.Remove(techType);
            }
        }
    }

    private void StorageContainer_ItemRemoved(InventoryItem item)
    {
        var techTag = item.item.gameObject.GetComponentInParent<TechTag>(true);
        QuickLogger.Debug($"StorageContainer_ItemRemoved: {item.item.GetTechName()}", true);
        if (techTag is not null)
        {
            UnTrackResource(techTag.type, item);
            OnServerAdded?.Invoke();
        }
    }

    private void StorageContainer_ItemAdded(InventoryItem item)
    {
        var techTag = item.item.gameObject.GetComponentInParent<TechTag>(true);
        QuickLogger.Debug($"StorageContainer_ItemAdded: {item.item.GetTechName()} |  Item Parent Type:  {techTag?.type.AsString()}", true);

        if (techTag is not null)
        {
            TrackResource(techTag.type, item);
            OnServerRemoved?.Invoke();
        }
    }


    internal void UnRegisterStorage(TechType deviceTechType, ItemsContainer storage)
    {
        //storageContainers.Remove(storage);

        foreach (var item in storage)
        {
            UnTrackResource(deviceTechType, item);
        }

        storage.onAddItem -= StorageContainer_ItemAdded;
        storage.onRemoveItem -= StorageContainer_ItemRemoved;
        OnServerRemoved?.Invoke();
    }

    internal void OnRackDestroyed(RackBase parentRack)
    {
        dssRacks.Remove(parentRack);
        OnRackRemoved?.Invoke();
    }

    private bool AnyItemsInNetwork()
    {
        return trackedResources.Any(); //dssRacks.Any() || storageContainers.Any();
    }

    internal bool HasSpace(int amount)
    {

        if(dssRacks.Any())
        {
            return SpaceAvaliable() >= amount + 1;
        }

        return false;
    }

    internal int SpaceAvaliable()
    {
        int sum = 0;
        foreach (var storage in dssRacks.Values)
        {
            foreach (var server in storage)
            {
                sum += server.GetFreeSpace();
            }
        }
        return sum;
    }

    internal bool AddItem(InventoryItem inventoryItem)
    { 
        foreach (var result in from storage in dssRacks.Values
                               from server in storage
                               where server.GetFreeSpace() >= 1
                               let result = server.AddItemToContainer(inventoryItem)
                               select result)
        {

            //TrackResource(DSSServerSpawnable.PatchedTechType, inventoryItem);
            OnServerAdded?.Invoke();
            return result;
        }

        return false;
    }

    internal Pickupable RemoveItem(TechType techType)
    {
        if(AnyItemsInNetwork())
        {
            foreach (var result in from servers in dssRacks.Values
                                   from server in servers
                                   where server.ContainsItem(techType)
                                   let result = server.RemoveItemFromContainer(techType)
                                   select result)
            {
                UnTrackResource(DSSServerSpawnable.PatchedTechType, result.inventoryItem);
                OnServerRemoved?.Invoke();
                return result;
            }

            var pickupable = UnTrackResourceAndReturn(TechType.None, techType);
            OnServerRemoved?.Invoke();

            return pickupable;
        }

        return null;
    }

    private Pickupable UnTrackResourceAndReturn(TechType patchedTechType, TechType techType)
    {
        if (trackedResources.TryGetValue(techType, out var resource))
        {
            QuickLogger.Debug($"[UnTrackResourceAndReturn] Retreive Pickable. {resource is null}");

            var pickupable = resource.UnTrackItemAndReturn(patchedTechType).item;

            if (resource.GetCount(DSSTerminalFilterOptions.ShowAll, habitatManager.IsDssIntegration()) <= 0)
            {
                trackedResources.Remove(techType);
            }

            QuickLogger.Debug($"[UnTrackResourceAndReturn] Received Pickable: {pickupable is not null}");

            return pickupable;
        }

        return null;
    }

    internal void LoadFromSave(string baseID, FCSStorage fcsStorage)
    {
        if(fcsStorage.container is not null)
        {
            foreach (InventoryItem item in fcsStorage.container)
            {
                //TODO Change from slot id to prefabid

                var storage = item.item.GetComponent<FCSStorage>();

                var parent = storage.GetComponentInParent<RackBase>();

                if(parent is not null)
                {
                    if(dssRacks.ContainsKey(parent))
                    {
                        dssRacks[parent].Add(storage);
                    }
                    else
                    {
                        dssRacks.Add(parent,new HashSet<FCSStorage>() { storage });
                    }
                }
            }
        }        
    }


    internal Dictionary<TechType,int> GetBaseItems(DSSTerminalFilterOptions filter)
    {
        var dict = new Dictionary<TechType, int>();

        switch (filter)
        {
            case DSSTerminalFilterOptions.Other:
                break;
            case DSSTerminalFilterOptions.ShowAll:
                foreach (var item in trackedResources)
                {
                    var count = item.Value.GetCount(filter, habitatManager.IsDssIntegration());
                    if(count > 0)
                        dict.Add(item.Key, count);
                }
                //GetServerItems(dict);
                //GetStorageContainerItems(dict);
                break;
            case DSSTerminalFilterOptions.Servers:
                GetServerItems(dict);
                break;
            case DSSTerminalFilterOptions.AlterraStorage:
                if (habitatManager.IsDssIntegration())
                {
                    GetItemsCount(dict, RemoteStorageBuildable.PatchedTechType);
                }
                break;
            case DSSTerminalFilterOptions.StorageLocker:
                if (habitatManager.IsDssIntegration())
                {
                    GetItemsCount(dict,TechType.Locker);
                    GetItemsCount(dict,TechType.SmallLocker);
                }        
                break;
            case DSSTerminalFilterOptions.SeaBreeze:
                if (habitatManager.IsDssIntegration())
                {

                }
                break;
            case DSSTerminalFilterOptions.Harvesters:
                if (habitatManager.IsDssIntegration())
                {

                }
                break;
            case DSSTerminalFilterOptions.Replicator:
                if (habitatManager.IsDssIntegration())
                {

                }
                break;
        }


        return dict;
    }

    private void GetItemsCount(Dictionary<TechType, int> dict,TechType device)
    {

        foreach (var trackedItem in trackedResources)
        {
            var count = trackedItem.Value.GetCount(device);
            if (count > 0) {
                dict.Add(trackedItem.Key, count);
            }
        }
    }

    private void GetServerItems(Dictionary<TechType, int> dict)
    {
        foreach (var servers in dssRacks.Values)
        {
            foreach (var storage in servers)
            {
                var server = storage.GetItemsWithin();

                foreach (var item in server)
                {
                    if (dict.ContainsKey(item.Key))
                    {
                        dict[item.Key] += item.Value;
                    }
                    else
                    {
                        dict.Add(item.Key, item.Value);
                    }
                }
            }
        }
    }

    internal bool IsAllowedToAdd(TechType item)
    {
        return HasSpace(1);
    }

    internal int GetRackCount()
    {
        return dssRacks.Count;
    }

    internal int GetServerCount()
    {
        return dssRacks.Sum( x => x.Value.Count);
    }

    internal string GetBaseName()
    {
        return habitatManager.GetBaseFriendlyName();
    }

    internal SubRoot GetSubRoot()
    {
        return habitatManager.GetSubRoot();
    }

    internal HabitatManager GetHabitatManager()
    {
        return habitatManager;
    }

    internal bool IsConnectedToNetwork()
    {
        return dssAntennas.Any(x => x.IsOperational());
    }

    internal int GetDeviceItemTotal(TechType techType)
    {
        int amount = 0;
        
        foreach (var item in trackedResources)
        {
            if(item.Value.HasDevice(techType,out int result))
            {
                amount += result;
            }
        }
        
        return amount;
    }

    internal int GetItemTotal(TechType currentItem)
    {
        if(trackedResources.TryGetValue(currentItem, out var result))
        {
            return result.GetCount(DSSTerminalFilterOptions.ShowAll, habitatManager.IsDssIntegration());
        }

        return 0;
    }
}