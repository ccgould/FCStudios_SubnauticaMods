using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseModuleRack.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseTransmitter.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;
public partial class HabitatManager : MonoBehaviour
{

    private HashSet<FCSDevice> _registeredDevices = new();
    private HashSet<string> _connectedDevices = new();
    private Dictionary<string,List<object>> _workUnits = new();
    private Dictionary<string,List<object>> _automatedOperations = new();
    private string _baseFriendlyID => GetBaseFriendlyName();
    private SubRoot _habitat;
    private Base _baseComponent;
    private string _prefabID;
    private int _baseID = -1;
    private string _baseName;
    private float timeLeft;
    private const int _transceiverMaxCount = 5;
    private const int _transmitterMaxCount = 10;
    private const int _baseManagerMaxCount = 10;

    private int conenctedDevicesLimit => DetermineDeviceLimit();

    internal int DetermineDeviceLimit()
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if(device is BaseManagerController)
            {
                count += _baseManagerMaxCount;
            }

            if(device is BaseTransmitterController)
            {
                count += _transmitterMaxCount;
            }
        }

        return count;
    }

    internal int DetermineModuleLimit()
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if (device is BaseManagerRackController)
            {
                count += 6;
            }
        }

        return count;
    }

    internal int GetInstalledModulesCount()
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if (device is BaseManagerRackController)
            {
                var controller = (BaseManagerRackController)device;
                count += controller.GetItemCount();
            }
        }

        return count;
    }

    internal int GetAutomatedOperationsCount()
    {
        return _automatedOperations.Count;
    }

    internal object DetermineOperationsLimit()
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if (device is BaseManagerRackController)
            {
                var rack = (BaseManagerRackController)device;
                count += rack.GetItemCount(BaseManagerBuildable.TranceiverModuleTechType);
            }
        }

        return count * _transceiverMaxCount;
    }


    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            foreach (var device in _registeredDevices)
            {
                AttemptToConnectDevice(device);
            }
            timeLeft = 1;
        }
    }

    public string GetBaseFriendlyName()
    {
        var baseType = _habitat.isBase ? "Base" : "Cyclops";
        return $"{baseType} {_baseID:D3}";
    }

    internal bool HasDevice(string prefabID)
    {
        return _registeredDevices.Any(x => x.GetPrefabID().Equals(prefabID));
    }

    internal bool HasDevice(TechType techType)
    {
        return _registeredDevices.Any(x => x.GetTechType().Equals(techType));
    }

    internal void RegisterDevice(FCSDevice device)
    {
        _registeredDevices.Add(device);
        AttemptToConnectDevice(device);
    }

    private void AttemptToConnectDevice(FCSDevice device)
    {
        var prefabID = device.GetPrefabID();

        if (device.BypassConnection || _connectedDevices.Contains(prefabID)) return;
        if(_connectedDevices.Count < conenctedDevicesLimit)
        {
            _connectedDevices.Add(prefabID);
        }
    }

    internal void UnRegisterDevice(FCSDevice device)
    {
        if(device.BypassConnection) return;
        _registeredDevices.Remove(device);
        DisconnectDevice(device);
    }

    private void DisconnectDevice(FCSDevice device)
    {
        _connectedDevices.Remove(device.GetPrefabID());
    }

    private void Awake()
    {
        HabitatService.main.onBaseCreated?.Invoke(this);
        _habitat = gameObject.GetComponent<SubRoot>();
        _baseComponent = _habitat.GetComponent<Base>();
        _prefabID = _habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
    }

    private void OnDestroy()
    {
        HabitatService.main.onBaseDestroyed?.Invoke(this);
    }

    /// <summary>
    /// Sets the base name field
    /// </summary>
    /// <param name="baseName"></param>
    public void SetBaseName(string baseName)
    {
        _baseName = baseName;
        //GlobalNotifyByID(String.Empty, "BaseUpdate");
    }

    /// <summary>
    /// Gets the stored base Name from the
    /// </summary>
    /// <returns></returns>
    public string GetBaseName()
    {
        return _baseName;
    }

    public string GetBasePrefabID() => _prefabID;

    internal int GetBaseID() => _baseID;

    internal void SetBaseID(int id)
    {
        if (_baseID != -1) return;
        
        _baseID = id;
    }

    internal Dictionary<string, List<FCSDevice>> GetDevicesList()
    {
        var result = new Dictionary<string, List<FCSDevice>>();

        foreach (var device in _registeredDevices)
        {
            var modName = ModRegistrationService.GetModName(device);
            if(result.ContainsKey(modName))
            {
                result[modName].Add(device);
            }
            else
            {
                result.Add(modName,new List<FCSDevice>() { device});
            }
        }

        return result;
    }

    internal float GetTotalPowerUsage()
    {
        return _registeredDevices.Sum(x => x.GetPowerUsage());
    }

    internal int GetTotalDevices()
    {
        return _registeredDevices.Count();
    }

    internal int GetConnectedDevices()
    {
        return _connectedDevices.Count;
    }

    public bool IsRemoteLinkConnected()
    {
        foreach (var device in _registeredDevices)
        {
            if(device is BaseManagerRackController)
            {
                var rack = (BaseManagerRackController)device;
                if(rack.HasModule(BaseManagerBuildable.RemoteModuleTechType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal bool IsDssIntegration()
    {
        foreach (var device in _registeredDevices)
        {
            if (device is BaseManagerRackController)
            {
                var rack = (BaseManagerRackController)device;
                if (rack.HasModule(BaseManagerBuildable.DSSIntegrationModuleTechType))
                {
                    return true;
                }
            }
        }

        return false; ;
    }

    internal void CreateWorkUnit(object data)
    {
        var guid = Guid.NewGuid().ToString();
        
        _workUnits.Add(guid, new List<object>
        {
            data
        });
    }

    internal bool AddToWorkUnit(string guid, string id)
    {
        if (_workUnits.ContainsKey(guid))
        {
            _workUnits[guid].Add(guid);
            return true;
        }

        return false;
    }

    internal bool DeleteFromWorkUnit(string guid, object device)
    {
        if(_workUnits.ContainsKey(guid))
        {
            _workUnits[guid].Remove(device);
            return true;
        }
        return false;
    }

    internal void DeleteWorkUnit(string id)
    {
        _workUnits.Remove(id);
    }

    internal int GetWorkUnitsCount()
    {
        return _workUnits.Count();
    }

    internal int GetFaultsCount(FaultType fault)
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if(_connectedDevices.Contains(device.GetPrefabID()))
            {
                count += device.GetWarningsCount(fault);
            }
        }

        return count;
    }
}
