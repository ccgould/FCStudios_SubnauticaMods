using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Enumerator;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FCS_StorageSolutions.Models;
public class TrackedResource
{
    private Dictionary<TechType, List<InventoryItem>> trackedDevices = new();

    public TrackedResource()
    {
        
    }

    public TrackedResource(TechType deviceTechType, InventoryItem item)
    {
        TrackItem(deviceTechType, item);
    }

    internal int GetCount(DSSTerminalFilterOptions filter,bool isDssIntergrated)
    {
        var amount = 0;
        if (!isDssIntergrated)
        {
            var dssserverItems = trackedDevices.Where(x=>x.Key == DSSServerSpawnable.PatchedTechType).ToList();

            foreach (var item in dssserverItems)
            {

                amount += item.Value.Count();
            }
        }
        else
        {
            foreach (var item in trackedDevices.Values)
            {
                amount += item.Count();
            }
        }



        return amount;
    }

    internal int GetCount(TechType device)
    {
        if(trackedDevices.ContainsKey(device))
        {
            return trackedDevices[device].Count;
        }
        return 0;
    }

    internal Dictionary<TechType, List<InventoryItem>> GetTrackedDevices()
    {
        return trackedDevices;
    }

    internal bool HasDevice(TechType deviceTechType, out int amount)
    {
        amount = 0; 

        if(trackedDevices.TryGetValue(deviceTechType,out var inventoryItems))
        {
            amount = inventoryItems.Count;
            return true;
        }

        return false;
    }

    //public TechType DeviceTechType { get; set; }
    //public int Amount { get; set; }
    //public HashSet<StorageContainer> StorageContainers { get; set; } = new HashSet<StorageContainer>();
    //public HashSet<FCSStorage> Servers { get; set; } = new HashSet<FCSStorage>();
    //public HashSet<FCSDevice> OtherStorage { get; set; } = new HashSet<FCSDevice>();

    internal void TrackItem(TechType deviceTechType, InventoryItem item)
    {
       if(trackedDevices.TryGetValue(deviceTechType, out var value))
        {
            value.Add(item);
        }
       else
        {
            trackedDevices.Add(deviceTechType, new List<InventoryItem> { item });
        }
    }

    internal void UnTrackItem(TechType deviceTechType, InventoryItem item)
    {
        if(trackedDevices.TryGetValue(deviceTechType, out var value))
        {
            value.Remove(item);

            if(value.Count <= 0)
            {
                trackedDevices.Remove(deviceTechType);
            }
        }
    }

    internal InventoryItem UnTrackItemAndReturn(TechType deviceTechType)
    {
        InventoryItem item = null;

        if (deviceTechType == TechType.None)
        {
           
            foreach (var items in trackedDevices.Values)
            {
                foreach(var item2 in items)
                {
                    items.Remove(item2);

                    if (items.Count <= 0)
                    {
                        trackedDevices.Remove(deviceTechType);
                    }
                    return item2;
                }
            }
        }
        else
        {
            if (trackedDevices.TryGetValue(deviceTechType, out var value))
            {

                item = value.FirstOrDefault();
                value.Remove(item);
                if (value.Count <= 0)
                {
                    trackedDevices.Remove(deviceTechType);
                }
                return item;
            }
            else
            {
                QuickLogger.Warning($"[UnTrackItemAndReturn] Track device {deviceTechType.AsString()} not found.");
            }
        }

        return item;
    }
}
