using Discord;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Interfaces;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono;
using FCSCommon.Utilities;
using Nautilus.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;
internal class BaseTelepowerPylonManager : MonoBehaviour, IPylonPowerManager
{
    public Dictionary<string, BaseTelepowerPylonManager> _connections = new();
    public SubRoot SubRoot;

    private static List<BaseTelepowerPylonManager> _globalTelePowerPylonManagers = new();
    private HashSet<TelepowerPylonController> _basePylons = new();
    private HabitatManager _manager;
    private bool _isInitialized;
    private PowerRelay _connectedPowerSource;
    private bool _pauseUpdates;
    private TelepowerPylonMode _mode = TelepowerPylonMode.NONE;
    private TelepowerPylonUpgrade _currentUpgrade = TelepowerPylonUpgrade.MK1;
    internal static TechType Mk2UpgradeTechType;
    internal static TechType Mk3UpgradeTechType;
    private int _maxConnectionLimit;
    private const int DEFAULT_CONNECTIONS_LIMIT = 6;
    private BaseTelePowerSave _savedData;
    private bool _hasBeenSet => _mode == TelepowerPylonMode.PULL || _mode == TelepowerPylonMode.PUSH;


    private void Awake()
    {
        _maxConnectionLimit = DEFAULT_CONNECTIONS_LIMIT;

        if (Mk2UpgradeTechType == TechType.None || Mk3UpgradeTechType == TechType.None)
        {
            Mk2UpgradeTechType = "TelepowerMk2Upgrade".ToTechType();
            Mk3UpgradeTechType = "TelepowerMk3Upgrade".ToTechType();
        }

        InvokeRepeating(nameof(UpdateConnections), 1f, 1f);

        Plugin.TelepowerPylonSaveData.OnStartedSaving += OnBeforeSave;


    }

    internal void LoadSave()
    {
        FindManager();
        if (Plugin.TelepowerPylonSaveData.TelepowerPylonBaseData.TryGetValue(_manager.GetBasePrefabID(), out var value))
        {
            QuickLogger.Debug($"Loading Save for telepowerpylon base : {_manager.GetBaseFormatedID()}", true);

            _mode = value.PylonMode;
            StartCoroutine(LoadSavedBases(value));
        }
    }

    private IEnumerator LoadSavedBases(TelepowerPylonBaseDataEntry save)
    {
        while (_globalTelePowerPylonManagers.Count < save.GlobalBaseCount && _basePylons.Count < save.BasePylonCount)
        {
            yield return null;
        }

        foreach (var connection in save.CurrentConnections)
        {
            _connections.Add(connection, _globalTelePowerPylonManagers.FirstOrDefault(x => x.GetBaseID().Equals(connection)));
        }

        if (save.CurrentUpgrade == TelepowerPylonUpgrade.MK2)
        {
            AttemptUpgrade(Mk2UpgradeTechType);
        }

        if (save.CurrentUpgrade == TelepowerPylonUpgrade.MK3)
        {
            AttemptUpgrade(Mk3UpgradeTechType);
        }

        yield break;
    }

    private IEnumerable<string> GetCurrentConnectionIDs()
    {
        foreach (var connection in _connections)
        {
            yield return connection.Key;
        }
    }

    private void OnBeforeSave(object sender, JsonFileEventArgs e)
    {
        if(Plugin.TelepowerPylonSaveData.TelepowerPylonBaseData.TryGetValue(_manager.GetBasePrefabID(), out var value))
        {
            value.CurrentConnections = GetCurrentConnectionIDs().ToList();
            value.PylonMode = _mode;
            value.CurrentUpgrade = _currentUpgrade;
            value.GlobalBaseCount = _globalTelePowerPylonManagers.Count;
            value.BasePylonCount = _basePylons.Count;
        }
        else
        {
            Plugin.TelepowerPylonSaveData.TelepowerPylonBaseData.Add(_manager.GetBasePrefabID(), new TelepowerPylonBaseDataEntry()
            {
                CurrentConnections = GetCurrentConnectionIDs().ToList(),
                PylonMode = _mode,
                CurrentUpgrade = _currentUpgrade,
                GlobalBaseCount = _globalTelePowerPylonManagers.Count,
                BasePylonCount = _basePylons.Count
            });
        }
    }

    internal bool GetHasBeenSet()
    {
        return _hasBeenSet;
    }

    internal int GetMaxConnectionLimit()
    {
        return _maxConnectionLimit;
    }

    private void Start()
    {
        FindManager();
    }

    internal bool AttemptUpgrade(TechType techType)
    {
        bool result = false;

        if (_currentUpgrade != ConvertUpgradeTechType(techType))
        {
            if (techType == Mk2UpgradeTechType)
            {
                _currentUpgrade = TelepowerPylonUpgrade.MK2;
                _maxConnectionLimit = 8;
                result = true;
            }

            if (techType == Mk3UpgradeTechType)
            {
                _currentUpgrade = TelepowerPylonUpgrade.MK3;
                _maxConnectionLimit = 10;
                result =  true;
            }
        }

        if(result)
        {
            ChangePylonColors(GetUpgradeColor());
            return true;
        }
        

        return false;
    }

    
    private TelepowerPylonUpgrade ConvertUpgradeTechType(TechType techType)
    {
        QuickLogger.Debug($"[ConvertUpgradeTechType] : {techType}");

        if (techType == Mk2UpgradeTechType)
        {
            QuickLogger.Debug($"Returned MK2");
            return TelepowerPylonUpgrade.MK2;
        }

        if (techType == Mk3UpgradeTechType)
        {
            QuickLogger.Debug($"Returned MK3");
            return TelepowerPylonUpgrade.MK3;
        }

        QuickLogger.Debug($"Returned MK1");
        return TelepowerPylonUpgrade.MK1;
    }

    private void FindManager()
    {
        if (_manager == null)
        {
            if (SubRoot == null)
            {
                QuickLogger.DebugError($"SubRoot is null for {nameof(BaseTelepowerPylonManager)}.");
                return;
            }

            var manager = HabitatService.main.GetBaseManager(gameObject);

            if (manager == null)
            {
                QuickLogger.DebugError($"Failed to find base manager for {nameof(BaseTelepowerPylonManager)}.");
                return;
            }

            _manager = manager;
        }
    }

    private void UpdateConnections()
    {
        if (_manager != null && !_pauseUpdates)
        {

            if (_connectedPowerSource == null)
            {
                _connectedPowerSource = _manager.GetSubRoot().powerRelay;
            }

            foreach (var pylonController in _connections.Values)
            {
                if (pylonController.GetPowerRelay() == null) continue;
                _connectedPowerSource.AddInboundPower(pylonController.GetPowerRelay());
            }
        }
    }

    public void AddConnection(BaseTelepowerPylonManager baseTelepowerPylonManager, bool toggleSelf = false)
    {
        _connections?.Add(baseTelepowerPylonManager.GetBaseID(), baseTelepowerPylonManager);
        QuickLogger.Debug($"Asking Base {baseTelepowerPylonManager.GetBaseID()} to check {GetBaseID()}");
        baseTelepowerPylonManager.CheckItem(toggleSelf ? baseTelepowerPylonManager : this);
    }

    public void RemoveConnection(BaseTelepowerPylonManager baseTelepowerPylonManager, bool toggleSelf = false)
    {
        if (!_connections.ContainsKey(baseTelepowerPylonManager.GetBaseID())) return;
        _pauseUpdates = true;
        _connectedPowerSource.RemoveInboundPower(_connections[baseTelepowerPylonManager.GetBaseID()].GetPowerRelay());
        QuickLogger.Debug($"Asking Base {baseTelepowerPylonManager.GetBaseID()} to uncheck {GetBaseID()}");

        baseTelepowerPylonManager.UnCheckItem(toggleSelf ? baseTelepowerPylonManager : this);

        _connections.Remove(baseTelepowerPylonManager.GetBaseID());
        _pauseUpdates = false;
    }

    private void CheckItem(BaseTelepowerPylonManager baseTelepowerPylonManager)
    {
        foreach (var basePylon in _basePylons)
        {
            basePylon.CheckFrequencyItem(baseTelepowerPylonManager.GetBaseID());
        }
    }

    private void ChangePylonColors(Color color)
    {
        foreach (var basePylon in _basePylons)
        {
            basePylon.ChangeEffectColor(color);
        }
    }

    private void UnCheckItem(BaseTelepowerPylonManager baseTelepowerPylonManager)
    {
        foreach (var basePylon in _basePylons)
        {
            basePylon.UnCheckFrequencyItem(baseTelepowerPylonManager.GetBaseID());
        }
    }

    public IPowerInterface GetPowerRelay()
    {
        return _connectedPowerSource;
    }

    public bool HasConnections()
    {
        foreach(var manager in _globalTelePowerPylonManagers)
        {
            if(manager == this) continue;

            if (manager.GetIsConnected(GetBaseID()))
            {
                return true;
            }
        }

        return _connections.Any();
        //return ;
    }

    public Dictionary<string, BaseTelepowerPylonManager> GetConnections()
    {
        return _connections;
    }

    public bool HasConnection(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId)) return false;
        return _connections?.ContainsKey(unitId.ToLower()) ?? false;
    }

    public TelepowerPylonMode GetCurrentMode()
    {
        return _mode;
    }

    public void SetCurrentMode(TelepowerPylonMode mode)
    {
        QuickLogger.Debug($"Setting Current Pylon Mode to {mode}", true);
        _mode = mode;
    }

    public string GetBaseID()
    {
        if (_manager == null) FindManager();
        return _manager?.GetBasePrefabID() ?? "N/A";
    }

    public string GetBaseName()
    {
        if (_manager == null) FindManager();
        if (_manager == null) return string.Empty;
        var name = !string.IsNullOrWhiteSpace(_manager.GetBaseShortHandFormatedID())
            ? _manager.GetBaseShortHandFormatedID()
            : _manager.GetBaseFriendlyName();
        return name;
    }

    public bool IsConnectionAllowed(BaseTelepowerPylonManager baseTelepowerPylonManager)
    {
        return !_connections.ContainsKey(baseTelepowerPylonManager.GetBaseID());
    }

    public GameObject GetRoot()
    {
        return SubRoot.gameObject;
    }

    public void RegisterPylon(TelepowerPylonController controller)
    {
        _basePylons.Add(controller);
        controller.ChangeEffectColor(GetUpgradeColor());
    }

    public void UnRegisterPylon(TelepowerPylonController controller)
    {
        _basePylons.Remove(controller);


        if (GetPylonCount() == 1)
        {
            switch (_currentUpgrade)
            {
                case TelepowerPylonUpgrade.MK2:
                    PlayerInteractionHelper.GivePlayerItem(Mk2UpgradeTechType);
                    break;
                case TelepowerPylonUpgrade.MK3:
                    PlayerInteractionHelper.GivePlayerItem(Mk3UpgradeTechType);
                    break;
            }
        }
    }

    public static void RegisterPylonManager(BaseTelepowerPylonManager manager)
    {
        _globalTelePowerPylonManagers.Add(manager);
    }

    public static void UnRegisterPylonManager(BaseTelepowerPylonManager manager)
    {
        if (manager is null) return;
        _globalTelePowerPylonManagers.Remove(manager);
    }

    private Color GetUpgradeColor()
    {
        QuickLogger.Debug($"GeyColorUpgrade : {_currentUpgrade}");
        switch(_currentUpgrade)
        {
            case TelepowerPylonUpgrade.MK2:
                return Color.cyan; 
            case TelepowerPylonUpgrade.MK3:
                return Color.green;
            default: 
                return new Color(0.9490196f, 0.5607843f, 0.09019608f, 1f);
        }
    }

    public static List<BaseTelepowerPylonManager> GetGlobalTelePowerPylonsManagers()
    {
        return _globalTelePowerPylonManagers;
    }

    internal BaseTelePowerSave Save()
    {
        if (_manager == null || _connections == null) return new BaseTelePowerSave();
        return new()
        {
            Id = _manager.GetBaseID(),
            Mode = _mode,
            Connections = _connections.Keys.ToList(),
            Upgrade = _currentUpgrade
        };
    }

    public bool GetIsConnected(string baseId)
    {
        return _connections.ContainsKey(baseId);
    }

    public bool HasPylon()
    {
        return _basePylons.Any();
    }

    public TelepowerPylonUpgrade GetUpgrade()
    {
        return _currentUpgrade;
    }

    public int GetPylonCount()
    {
        return _basePylons.Count;
    }
}
