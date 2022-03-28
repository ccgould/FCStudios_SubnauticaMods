using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private BaseTelePowerSave _savedData;


        private void Awake()
        {
            FindManager();



            InvokeRepeating(nameof(UpdateConnections), 1f, 1f);
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

                if (_savedData == null)
                {
                    _savedData = Mod.GetBaseTelePowerPylonSaveData(manager.BaseFriendlyID);
                }

                if (_savedData != null)
                {
                    _mode = _savedData.Mode;
                    foreach (var connection in _savedData.Connections)
                    {
                        _connections.Add(connection,_globalTelePowerPylonManagers.FirstOrDefault(x=>x.GetBaseID().Equals(connection)));
                    }
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
            _mode = mode;
        }

        public string GetBaseID()
        {
            if(_manager == null) FindManager();
            return _manager?.GetBaseFriendlyId();
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
        }

        public void UnRegisterPylon(TelepowerPylonController controller)
        {
            _basePylons.Remove(controller);
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
            return new()
            {
                Id = _manager.GetBaseFriendlyId(), 
                Mode = _mode,
                Connections = _connections.Keys.ToList()
            };
        }

        public bool GetIsConnected(string id)
        {
            return _connections.ContainsKey(id);
        }

        public bool HasPylon()
        {
            return _basePylons.Any();
        }
    }

    internal class BaseTelePowerSave
    {
        public string Id { get; set; }
        public TelepowerPylonMode Mode { get; set; }
        public List<string> Connections { get; set; } = new();
    }
}