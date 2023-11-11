using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
using FCSCommon.Utilities;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class handles all habitat data for the FCStudios Mods
/// </summary>
public class HabitatService : MonoBehaviour
{
    public static HabitatService main { get; private set; }
    private HashSet<KnownDevice> knownDevices  = new();
    private HashSet<HabitatManager> knownBases  = new();
    
    public Action<HabitatManager> onBaseDestroyed;

    public Action<HabitatManager> onBaseCreated;
    private  readonly HashSet<FCSDevice> _globalFCSDevices = new();


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        SaveUtils.RegisterOnFinishLoadingEvent(() =>
        {
            QuickLogger.Debug("On Finished Loading RegisterOnFinishLoadingEvent");
            foreach(var device in knownDevices)
            {
                QuickLogger.Debug($"{device.ID}");
            }
        });


        if (main != null && main != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            main = this;
        }

        onBaseCreated += RegisterBase;
        onBaseDestroyed += UnRegisterBase;
    }

    private void OnDestroy()
    {
        onBaseCreated -= RegisterBase;
        onBaseDestroyed -= UnRegisterBase;
    }

    private void RegisterBase(HabitatManager baseManager)
    {
        if(!knownBases.Contains(baseManager))
        {
            var savedID = -1;

            if(savedID < 0)
            {
                baseManager.SetBaseID(GenerateNewBaseID());
            }
            else
            {
                baseManager.SetBaseID(savedID);
            }
            knownBases.Add(baseManager);
        }
    }

    private string GenerateNewID(string modID, string prefabID)
    {
        var newEntry = new KnownDevice();
        var id = 0;
        if (knownDevices.Any())
        {
            id = knownDevices.Where(x => x.DeviceTabId.Equals(modID)).DefaultIfEmpty().Max(x => x.ID);
            id++;
        }

        newEntry.ID = id;
        newEntry.PrefabID = prefabID;
        newEntry.DeviceTabId = modID;
        knownDevices.Add(newEntry);
        return newEntry.ToString();
    }

    private int GenerateNewBaseID()
    {
        if(!knownBases.Any())
        {
            return 0;
        }

        var result = knownBases.Max(x => x.GetBaseID());
        return ++result;
    }

    private void UnRegisterBase(HabitatManager baseManager)
    {
        knownBases.Remove(baseManager);
    }

    internal void RegisterDevice(FCSDevice device)
    {
        var prefabID = device.GetPrefabID();

        QuickLogger.Debug($"Attempting to Register: {prefabID} : Constructed {device.IsConstructed}");

        if (string.IsNullOrWhiteSpace(prefabID) || !device.IsConstructed) return;

        if (knownDevices.Any(x => x.PrefabID.Equals(prefabID)))
        {
            QuickLogger.Debug($"Found Saved Device with ID {prefabID}: Known Count{knownDevices.Count}");
            device.UnitID = knownDevices.FirstOrDefault(x => x.PrefabID.Equals(prefabID)).ToString();
        }
        else
        {
            QuickLogger.Debug($"Creating new ID: Known Count{knownDevices.Count}");
            var unitID = GenerateNewID(ModRegistrationService.GetModID(device.GetTechType()), prefabID);
            device.UnitID = unitID;

            //SaveDevices(knownDevices);
        }

        _globalFCSDevices.Add(device);

        var baseManager = device.GetComponentInParent<HabitatManager>();

        if(baseManager is not null) 
        {
            baseManager.RegisterDevice(device);
        }

        QuickLogger.Debug($"Registering Device: {device.UnitID}");

        foreach (var item in device.GetAllComponentsInChildren<Button>())
        {

        }
    }

    internal void UnRegisterDevice(FCSDevice device)
    {

        var result = knownDevices.FirstOrDefault(x => x.PrefabID.Equals(device.GetPrefabID()));
        if (!string.IsNullOrWhiteSpace(result.PrefabID))
        {
            knownDevices.Remove(result);
            _globalFCSDevices.Remove(device);
            var baseManager = knownBases.FirstOrDefault(x => x.HasDevice(result.PrefabID));
            baseManager?.UnRegisterDevice(device);
        }
    }

    internal HabitatManager GetPlayersCurrentBase()
    {
        return Player.main.GetCurrentSub()?.GetComponent<HabitatManager>();
    }

    internal Dictionary<string, List<FCSDevice>> GetDevicesInCurrentBase()
    {
        var currentBase = GetPlayersCurrentBase();

        return currentBase.GetDevicesList();
    }

    internal  HashSet<FCSDevice> GetRegisteredDevices()
    {
        return _globalFCSDevices;
    }

    internal HabitatManager GetHabitat(FCSDevice device)
    {
        foreach (var currentBase in knownBases)
        {
            if(currentBase.HasDevice(device.GetPrefabID()))
            {
                return currentBase;
            }
        }

        return null;
    }

    internal bool IsBaseManagerBuilt()
    {
        if (!Player.main?.IsInBase() ?? false) return false;
        return GetPlayersCurrentBase()?.HasDevice(BaseManagerBuildable.PatchedTechType) ?? false;
    }

    internal bool IsRegisteredInBase(string prefabID,out  HabitatManager manager)
    {
        var g = knownBases.Where(x=>x.HasDevice(prefabID)).FirstOrDefault();

        if (g != null)
        {
            manager = g;
            return true;
        }

        manager = null;
        return false;
    }

    internal HabitatManager GetBaseManager(FCSDevice baseManagerController)
    {
        return baseManagerController.GetComponentInParent<HabitatManager>();
    }

    public HabitatManager GetBaseManager(GameObject gameObject)
    {
        var subRoot = gameObject?.GetComponentInParent<SubRoot>() ?? gameObject?.GetComponentInChildren<SubRoot>();

        if (subRoot == null)
        {
            QuickLogger.Debug($"[BaseManager] SubRoot Returned null");
            return null;
        }
        return subRoot.GetComponentInChildren<HabitatManager>();
    }
    internal bool IsRemoteModuleInstalledInCurrentBase()
    {
        if (!Player.main?.IsInBase() ?? false) return false;
        return GetPlayersCurrentBase()?.IsRemoteLinkConnected() ?? false;
    }
}
