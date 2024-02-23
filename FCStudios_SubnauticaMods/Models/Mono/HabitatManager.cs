using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Managers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseModuleRack.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;
public partial class HabitatManager : MonoBehaviour, IFCSDumpContainer
{

    private HashSet<FCSDevice> _registeredDevices = new();
    private readonly Dictionary<string,FCSDevice> _connectedDevices = new();
    private Dictionary<string, WorkUnit> _workUnits = new();
    private Dictionary<string,List<object>> _automatedOperations = new();
    private string _baseFriendlyID => GetBaseFormatedID();

    private DumpContainerSimplified _dumpContainer;
    private SubRoot _habitat;
    private Base _baseComponent;
    private string _prefabID;
    private int _baseID = -1;
    private string _baseName;
    private float timeLeft;
    private int _failedConnectionAttempts;
    private const int _transceiverMaxCount = 5;
    private const int _transmitterMaxCount = 10;
    private const int _deviceLimitIncrement = 10;

    internal int ConnectedDevicesLimit => DetermineDeviceLimit();
    public Action<InventoryItem> OnItemTransferedToBase;
    public Action OnTransferActionCompleted;
    public Action<TechType> OnModuleRemoved;
    public Action<TechType> OnModuleAdded;
    public bool PullFromDockedVehicles { get; set; } = true;
    private readonly List<TechType> _dockingBlackList = new();

    internal void LoadSaveData(SaveData.BaseInfoSaveData value)
    {
        _baseName = value.FriendlyName;
        _baseID = value.BaseId;
        QuickLogger.Debug($"Loaded Base ID: {_baseID} with friendlyName {_baseName} at PrefabID {GetBasePrefabID()}");

    }

    public void AddDockingBlackList(TechType techType)
    {
        _dockingBlackList.Add(techType);
        HabitatService.main.GlobalNotifyByID("SST", "AddToBlackList");
    }

    public void RemoveDockingBlackList(TechType techType)
    {
        _dockingBlackList.Remove(techType);
        HabitatService.main.GlobalNotifyByID("SST", "RemoveFromBlackList");

    }

    public  ReadOnlyCollection<TechType> GetDockingBlackList()
    {
        return _dockingBlackList.AsReadOnly();
    }

    internal PortManager GetPortManager()
    {
        return _portManager;
    }

    internal void SetPortManager(PortManager portManager)
    {
        _portManager = portManager;
    }

    internal int DetermineDeviceLimit()
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if(device.AffectsHabitatDeviceLimit())
            {
                count += _deviceLimitIncrement;
            }
        }

        return count;
    }

    public bool IsCyclops()
    {
        return _habitat.isCyclops;
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

    private void Awake()
    {
        _vehicleDockingBayManager = gameObject.GetComponent<VehicleDockingBayManager>();
    }

    public VehicleDockingBayManager GetDockingManager()
    {
        return _vehicleDockingBayManager;
    }

    private void Update()
    {
        timeLeft -= DayNightCycle.main.deltaTime;

        if (timeLeft <= 0)
        {
            PowerConsumption();
            //PerformOperations();
            timeLeft = 1f;
        }

        //PowerStateCheck();
    }

    /// <summary>
    /// Returns the base ID formated in the format of (Base 000) or (Cyclops 000)
    /// </summary>
    /// <returns></returns>
    public string GetBaseFormatedID()
    {
        var baseType = string.Empty;

        if (_habitat is not null)
        {
            baseType = _habitat.isBase ? "Base" : "Cyclops";
        }
        return $"{baseType} {_baseID:D3}";
    }

    /// <summary>
    /// Returns the base ID formated in the format of (BS 000) or (SUB 000)
    /// </summary>
    /// <returns></returns>
    public string GetBaseShortHandFormatedID()
    {
        var baseType = string.Empty;

        if (_habitat is not null)
        {
            baseType = _habitat.isBase ? "BS" : "SUB";
        }
        return $"{baseType} {_baseID:D3}";
    }

    /// <summary>
    /// Gets the stored base Name from the
    /// </summary>
    /// <returns></returns>
    public string GetBaseFriendlyName()
    {
        return _baseName;
    }

    /// <summary>
    /// Sets the base name field
    /// </summary>
    /// <param name="baseName"></param>
    public void SetBaseName(string baseName)
    {
        _baseName = baseName;
        HabitatService.main.GlobalNotifyByID(string.Empty, "BaseUpdate");
    }

    internal bool HasDevice(string prefabID)
    {
        return _registeredDevices.Any(x => x.GetPrefabID().Equals(prefabID));
    }

    internal bool HasDevice(TechType techType)
    {
        return _registeredDevices.Any(x => x.GetTechType().Equals(techType));
    }

    public void RegisterDevice(FCSDevice device)
    {
        _registeredDevices.Add(device);
        AttemptToConnectDevice(device);
    }

    internal bool AttemptToConnectDevice(FCSDevice device)
    {
        _failedConnectionAttempts = 0;
        // return true because the device is in the list or bypasses the connection
        if (_connectedDevices.ContainsKey(device.GetPrefabID()) || device.GetBypassConnection()) return true;

        if(_connectedDevices.Count < ConnectedDevicesLimit)
        {
            _connectedDevices.Add(device.GetPrefabID(), device);
            return true;
        }

        _failedConnectionAttempts++;
        return false;
    }

    public int GetFailedConnectionAttemptsCount()
    {
        return _failedConnectionAttempts;
    }

    public int DevicesThatCanConnectCount()
    {
        return _registeredDevices.Count(x=>x.GetBypassConnection() == false);
    }

    public void UnRegisterDevice(FCSDevice device)
    {
        _registeredDevices.Remove(device);
        AdjustConnectedDevices(device);
        DisconnectDevice(device);
         if(!HasBaseManager())
        {
            _connectedDevices.Clear();
        }
    }

    private bool HasBaseManager()
    {
        return HasDevice(BaseManagerBuildable.PatchedTechType);
    }

    private void DisconnectDevice(FCSDevice device)
    {
        if (device.GetBypassConnection()) return;
        DisconnectDevice(device.GetPrefabID());
    }

    private void DisconnectDevice(string devicePrefabID)
    {        
        _connectedDevices.Remove(devicePrefabID);
    }

    private void AdjustConnectedDevices(FCSDevice device)
    {
        if (device.AffectsHabitatDeviceLimit())
        {
            if(_connectedDevices.Count > ConnectedDevicesLimit)
            {
                var amountToRemove = _connectedDevices.Count - ConnectedDevicesLimit;

                for (int i = 0; i < amountToRemove; i++)
                {
                    var currentDevicePrefabID = _connectedDevices.Last();
                    DisconnectDevice(currentDevicePrefabID.Key);
                }
            }
        }
    }

    public bool IsDeviceConnected(string deviceID)
    {
        return _connectedDevices.ContainsKey(deviceID);
    }


    internal void SetSubRoot(SubRoot instance)
    {
        _habitat = instance;
        _baseComponent = _habitat.GetComponent<Base>();
        _prefabID = _habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
    }

    internal void Initialize()
    {
        if (_dumpContainer == null)
        {
            _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
            _dumpContainer.Initialize(gameObject.transform, $"Add item to base: {GetBaseFormatedID()}", this, 6, 8, gameObject.name);
            _dumpContainer.OnDumpContainerClosed += OnTransferCompleted;
        }
    }


    private void Start()
    {
        HabitatService.main.onBaseCreated?.Invoke(this);
    }

    private void OnDestroy()
    {
        HabitatService.main.onBaseDestroyed?.Invoke(this);
    }


    public string GetBasePrefabID() => _prefabID;

    public int GetBaseID() => _baseID;

    internal string GetBaseIDFormatted()
    {
        return $"BS{GetBaseID():D3}";
    }

    internal void SetBaseID(int id)
    {
        if (_baseID != -1) return;
        
        _baseID = id;
    }

    internal bool HasEnoughPower(float power)
    {
        if (_habitat.powerRelay == null)
        {
            //QuickLogger.DebugError("Habitat is null");
        }

        if (!UWEHelpers.RequiresPower()) return true;

        if (_habitat.powerRelay == null || _habitat.powerRelay.GetPower() < power) return false;
        return true;
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

    internal int GetConnectedDevicesCount()
    {
        return _connectedDevices.Count();
    }

    public Dictionary<string,FCSDevice> GetConnectedDevices(bool inWorkGroup = false)
    {
        if(inWorkGroup)
        {
            var devices = new Dictionary<string,FCSDevice>();

            foreach (var device in _connectedDevices)
            {
                if(!IsInWorkGroup(device.Value.GetPrefabID()))
                {
                    devices.Add(device.Key,device.Value);
                }
            }

            return devices;
        }


        return _connectedDevices;
    }

    public IEnumerable<FCSDevice> GetCount<T>() where T : new()
    {
        return _registeredDevices.Where(x => x is T);
    }

    private bool IsInWorkGroup(string prefabID)
    {
       return _workUnits.Any(x=>x.Value.Devices.Any(x=>x.GetPrefabID().Equals(prefabID)));
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

    public bool IsDssIntegration()
    {
        foreach (var device in _registeredDevices)
        {
            if (device is BaseManagerRackController)
            {
                var rack = (BaseManagerRackController)device;
                if (rack.HasModule(FCSModsAPI.PublicAPI.GetDssInterationTechType()/*BaseManagerBuildable.DSSIntegrationModuleTechType*/))
                {
                    return true;
                }
            }
        }

        return false; ;
    }

    public string CreateWorkUnit(List<IWorkUnit> devices, string groupName)
    {
        var guid = Guid.NewGuid().ToString();
        
        _workUnits.Add(guid, new WorkUnit(guid,groupName,devices));

        return guid;
    }

    public bool AddToWorkUnit(string guid, IWorkUnit workUnit)
    {
        if (_workUnits.ContainsKey(guid))
        {
            var device = workUnit as FCSDevice;


            _workUnits[guid].Devices.Add(workUnit);
            return true;
        }

        return false;
    }

    public void OnDeviceUIClosed(IWorkUnit obj)
    {

        var g = _workUnits.FirstOrDefault(x=>x.Value.Devices.Contains(obj));
        QuickLogger.Debug($"Work Group {g.Key}", true);

        if (g.Value.Devices is not null)
        {
            foreach (var device in g.Value.Devices)
            {
                QuickLogger.Debug("Syning Device",true);
                device.SyncDevice(obj);
            }
        }
    }

    internal bool DeleteFromWorkUnit(string guid, IWorkUnit device)
    {
        if(_workUnits.ContainsKey(guid))
        {
            _workUnits[guid].Devices.Remove(device);
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

    internal Dictionary<string, WorkUnit> GetWorkUnits()
    {
        return _workUnits;
    }

    internal int GetFaultsCount(FaultType fault)
    {
        int count = 0;
        foreach (var device in _registeredDevices)
        {
            if(_connectedDevices.ContainsKey(device.GetPrefabID()))
            {
                count += device.GetWarningsCount(fault);
            }
        }

        return count;
    }

    internal bool HasDeviceSlotsAvailable()
    {
        return _connectedDevices.Count < ConnectedDevicesLimit;
    }

    public bool HasDevicesNotConnected()
    {
        return GetFailedConnectionAttemptsCount() > 0;
    }

    public bool AddItemToContainer(InventoryItem item)
    {
       // QuickLogger.Debug($"Add Items to base: {item.item.GetTechName()}", true);
        OnItemTransferedToBase?.Invoke(item);
        return true;
    }

    internal void OnTransferCompleted()
    {
        OnTransferActionCompleted?.Invoke();
    }

    public bool IsAllowedToAdd(Pickupable pickupable, int containerTotal = 0)
    {
        return (IsAllowedToAddToBase?.Invoke(pickupable.GetTechType(), containerTotal) ?? false);
    }

    internal PowerSystem.Status GetPowerState()
    {
        if (_habitat?.powerRelay?.GetPowerStatus() == null)
        {
            QuickLogger.DebugError("GetPowerState returned null");
        }
        return _habitat?.powerRelay?.GetPowerStatus() ?? PowerSystem.Status.Offline;
    }

    public Func<TechType,int,bool> IsAllowedToAddToBase;
    public Func<int,bool> HasSpace;
    private PortManager _portManager;


    public SubRoot GetSubRoot()
    {
        return _habitat;
    }

    public bool IsDockingFilterAddedWithType(TechType techType)
    {
        return _dockingBlackList.Contains(techType);
    }
    public void OpenItemTransfer()
    {
        _dumpContainer.OpenStorage();
    }

    public void RegisterDockingBay(VehicleDockingBay instance)
    {
        _vehicleDockingBayManager.RegisterDockingBay(instance);
    }

    public void UnRegisterDockingBay(VehicleDockingBay instance)
    {
        _vehicleDockingBayManager.UnRegisterDockingBay(instance);
    }

    private VehicleDockingBayManager _vehicleDockingBayManager;

    public float GetPower()
    {
        return GetSubRoot().powerRelay.GetPower();
    }

    public float GetBasePowerCapacity()
    {
        return GetSubRoot().powerRelay.GetMaxPower();
    }

    private void PowerConsumption()
    {
        if (_registeredDevices == null) return;
        //Take power from the base
        for (int i = _registeredDevices.Count - 1; i >= 0; i--)
        {
            var device = _registeredDevices.ElementAt(i);

            if (device.IsOperational() && _habitat.powerRelay != null)
            {
                _habitat.powerRelay.ConsumeEnergy(device.GetPowerUsage(), out float amountConsumed);
            }
        }
    }
}

public struct WorkUnit
{
    public WorkUnit(string guid, string friendlyName, List<IWorkUnit> devices)
    {
        FriendlyName = friendlyName;
        Guid = guid;
        Devices = devices;
    }

    public string FriendlyName { get; set; }
    public string Guid { get; set; }
    public List<IWorkUnit> Devices { get; set; }
}
