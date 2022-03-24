using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Mods.TelepowerPylon.Interfaces;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Model
{
    internal class BaseTelepowerPylonManager : MonoBehaviour,IPylonPowerManager
    {
        internal static HashSet<BaseTelepowerPylonManager> TelePowerPylonBases { get; set; } = new();
        private static HashSet<TelepowerPylonController> _basePylons = new();

        public Dictionary<string, BaseTelepowerPylonManager> _connections = new();
        private BaseManager _manager;
        public SubRoot SubRoot;
        private bool _isInitialized;
        private PowerRelay _connectedPowerSource;
        private bool _pauseUpdates;
        private TelepowerPylonMode _mode = TelepowerPylonMode.PUSH;


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

        public static void RegisterPylon(TelepowerPylonController controller)
        {
            _basePylons.Add(controller);
        }

        public static void UnRegisterPylon(TelepowerPylonController controller)
        {
            _basePylons.Remove(controller);
        }
    }
}