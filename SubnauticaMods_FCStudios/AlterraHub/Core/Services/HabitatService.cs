using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Diagnostics.ConfigurationManagerInternalFactory;
using static uGUI_TabbedControlsPanel;

namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class handles all habitat data for the FCStudios Mods
/// </summary>
internal class HabitatService : MonoBehaviour
{
    public static HabitatService main { get; private set; }
    private HashSet<KnownDevice> knownDevices { get; set; } = new();
    private HashSet<HabitatManager> knownBases { get; set; } = new();
    
    public Action<HabitatManager> onBaseDestroyed;

    public Action<HabitatManager> onBaseCreated;
    private  readonly HashSet<FCSDevice> _globalFCSDevices = new();

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.


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

    internal void RegisterDevice(FCSDevice device, TechType techType)
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
            var unitID = GenerateNewID(ModRegistrationService.GetModID(techType), prefabID);
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
}
