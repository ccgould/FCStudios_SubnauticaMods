using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Interfaces;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Model
{
    internal class BaseTelepowerPylonManager : MonoBehaviour,IPylonPowerManager
    {
        private static List<BaseTelepowerPylonManager> _globalTelePowerPylonManagers = new();
        private HashSet<TelepowerPylonController> _basePylons = new();

        public Dictionary<string, BaseTelepowerPylonManager> _connections = new();
        private BaseManager _manager;
        public SubRoot SubRoot;
        private bool _isInitialized;
        private PowerRelay _connectedPowerSource;
        private bool _pauseUpdates;
        private TelepowerPylonMode _mode = TelepowerPylonMode.PUSH;
        private TelepowerPylonUpgrade _currentUpgrade = TelepowerPylonUpgrade.MK1;
        internal static TechType Mk2UpgradeTechType;
        internal static TechType Mk3UpgradeTechType;
        private int _maxConnectionLimit;
        private const int DEFAULT_CONNECTIONS_LIMIT = 6;
        private BaseTelePowerSave _savedData;


        private void Awake()
        {
            _maxConnectionLimit = DEFAULT_CONNECTIONS_LIMIT;

            if (Mk2UpgradeTechType == TechType.None || Mk3UpgradeTechType == TechType.None)
            {
                Mk2UpgradeTechType = "TelepowerMk2Upgrade".ToTechType();
                Mk3UpgradeTechType = "TelepowerMk3Upgrade".ToTechType();
            }

            InvokeRepeating(nameof(UpdateConnections), 1f, 1f);
        }

        internal int GetMaxConnectionLimit()
        {
            return _maxConnectionLimit;
        }

        private void Start()
        {
            FindManager();

            if (_savedData == null && _manager != null)
            {
                _savedData = Mod.GetBaseTelePowerPylonSaveData(_manager.BaseFriendlyID);
            }

            if (_savedData != null)
            {
                QuickLogger.Debug("Loading Telepower Base Save Data", true);

                _mode = _savedData.Mode;
                foreach (var connection in _savedData.Connections)
                {
                    _connections.Add(connection, _globalTelePowerPylonManagers.FirstOrDefault(x => x.GetBaseID().Equals(connection)));
                }


                if (_savedData.Upgrade == TelepowerPylonUpgrade.MK2)
                {
                    AttemptUpgrade(Mk2UpgradeTechType);
                }

                if (_savedData.Upgrade == TelepowerPylonUpgrade.MK3)
                {
                    AttemptUpgrade(Mk3UpgradeTechType);
                }
            }
        }

        internal bool AttemptUpgrade(TechType techType)
        {
            if (techType == Mk2UpgradeTechType && _currentUpgrade == TelepowerPylonUpgrade.MK1)
            {
                _currentUpgrade = TelepowerPylonUpgrade.MK2;
                _maxConnectionLimit = 8;
                ChangePylonColors(Color.cyan);
                return true;
            }

            if (techType == Mk3UpgradeTechType && _currentUpgrade != TelepowerPylonUpgrade.MK3)
            {
                _currentUpgrade = TelepowerPylonUpgrade.MK3;
                _maxConnectionLimit = 10;
                ChangePylonColors(Color.green);
                return true;
            }

            return false;
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

                var manager = BaseManager.FindManager(SubRoot);
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
                    _connectedPowerSource = _manager.Habitat.powerRelay;
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
            QuickLogger.Debug($"Asking Base {baseTelepowerPylonManager.GetBaseID()} to check {this.GetBaseID()}");
            baseTelepowerPylonManager.CheckItem(toggleSelf ? baseTelepowerPylonManager : this);
        }

        public void RemoveConnection(BaseTelepowerPylonManager baseTelepowerPylonManager, bool toggleSelf = false)
        {
            if (!_connections.ContainsKey(baseTelepowerPylonManager.GetBaseID())) return;
            _pauseUpdates = true;
            _connectedPowerSource.RemoveInboundPower(_connections[baseTelepowerPylonManager.GetBaseID()].GetPowerRelay());
            QuickLogger.Debug($"Asking Base {baseTelepowerPylonManager.GetBaseID()} to uncheck {this.GetBaseID()}");

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
            return _connections.Any();
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
            QuickLogger.Debug($"Setting Current Pylon Mode to {mode}",true);
            _mode = mode;
        }
        
        public string GetBaseID()
        {
            if(_manager == null) FindManager();
            return _manager?.GetBaseFriendlyId() ?? "N/A";
        }

        public string GetBaseName()
        {
            if (_manager == null) FindManager();
            if (_manager == null) return string.Empty;
            var name = !string.IsNullOrWhiteSpace(_manager.GetBaseName())
                ? _manager.GetBaseName()
                : _manager.GetBaseFriendlyId();
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
            controller.GoToPage(_mode);
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
            _globalTelePowerPylonManagers.Remove(manager);
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
                Id = _manager.GetBaseFriendlyId(), 
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

    internal class BaseTelePowerSave
    {
        public string Id { get; set; }
        public TelepowerPylonMode Mode { get; set; } = TelepowerPylonMode.PUSH;
        public List<string> Connections { get; set; } = new();
        public TelepowerPylonUpgrade Upgrade { get; set; } = TelepowerPylonUpgrade.MK1;
    }
}