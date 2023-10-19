using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Enumerator;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_StorageSolutions.Models;
internal class DSSManager : MonoBehaviour
{
    private Dictionary<RackBase,HashSet<FCSStorage>> dssRacks = new();
    private HashSet<StorageContainer> storageContainers = new();

    private    readonly Dictionary<TechType, TrackedResource> TrackedResources = new();

    internal Action OnServerAdded;
    internal Action OnServerRemoved;
    internal Action OnRackRemoved;
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

    internal void RegisterStorage(RackBase parentRack, FCSStorage storage)
    {
        if (dssRacks.ContainsKey(parentRack))
        {
            dssRacks[parentRack].Add(storage);
        }
        else
        {
            dssRacks.Add(parentRack, new HashSet<FCSStorage>() { storage });
        }

        OnServerAdded?.Invoke();
    }

    internal void UnRegisterStorage(RackBase parentRack,FCSStorage storage)
    {
        if(dssRacks.ContainsKey(parentRack))
        {
            dssRacks[parentRack].Remove(storage);
            OnServerRemoved?.Invoke();
        }
    }

    internal void RegisterStorage(StorageContainer storage)
    {
        storageContainers.Add(storage);
        storage.container.onAddItem += StorageContainer_ItemAdded;
        storage.container.onRemoveItem += StorageContainer_ItemRemoved;
        OnServerAdded?.Invoke();
    }

    private void StorageContainer_ItemRemoved(InventoryItem item)
    {
        OnServerAdded?.Invoke();
    }

    private void StorageContainer_ItemAdded(InventoryItem item)
    {
        OnServerRemoved?.Invoke();
    }

    internal void UnRegisterStorage(StorageContainer storage)
    {
        storageContainers.Remove(storage);
        storage.container.onAddItem -= StorageContainer_ItemAdded;
        storage.container.onRemoveItem -= StorageContainer_ItemRemoved;
        OnServerRemoved?.Invoke();
    }

    internal void OnRackDestroyed(RackBase parentRack)
    {
        dssRacks.Remove(parentRack);
        OnRackRemoved?.Invoke();
    }

    private bool Any()
    {
        return dssRacks.Any() || storageContainers.Any();
    }

    internal bool HasSpace(int amount)
    {
        if(Any())
        {
            return SpaceAvaliable() >= amount + 1;
        }

        return false;
    }

    internal int SpaceAvaliable()
    {
        if(Any())
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

        return 0;
    }

    internal bool AddItem(InventoryItem inventoryItem)
    {
        foreach (var result in from storage in dssRacks.Values
                               from server in storage
                               where server.GetFreeSpace() >= 1
                               let result = server.AddItemToContainer(inventoryItem)
                               select result)
        {
            OnServerAdded?.Invoke();
            return result;
        }

        return false;
    }

    internal Pickupable RemoveItem(TechType techType)
    {
        if(Any())
        {
            foreach (var result in from servers in dssRacks.Values
                                   from server in servers
                                   where server.ContainsItem(techType)
                                   let result = server.RemoveItemFromContainer(techType)
                                   select result)
            {
                OnServerRemoved?.Invoke();
                return result;
            }

           foreach (var storage in storageContainers)
            {
                if(storage.container.Contains(techType))
                {
                    var item = storage.container.RemoveItem(techType);
                    return item;
                }
            }
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
                GetServerItems(dict);
                GetStorageContainerItems(dict);
                break;
            case DSSTerminalFilterOptions.Servers:
                GetServerItems(dict);
                break;
            case DSSTerminalFilterOptions.AlterraStorage:
                break;
            case DSSTerminalFilterOptions.StorageLocker:
                GetStorageContainerItems(dict);
                break;
            case DSSTerminalFilterOptions.SeaBreeze:
                break;
            case DSSTerminalFilterOptions.Harvesters:
                break;
            case DSSTerminalFilterOptions.Replicator:
                break;
        }


        return dict;
    }

    private void GetStorageContainerItems(Dictionary<TechType, int> dict)
    {
        foreach (var storageContainer in storageContainers)
        {
            foreach (var item in storageContainer.container)
            {
                var techType = item.item.GetTechType();
                if (dict.ContainsKey(techType))
                {
                    dict[techType] += 1;
                }
                else
                {
                    dict.Add(techType, 1);
                }
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
}