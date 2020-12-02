using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    /*
     * The Base manager class handles all connections to bases and power demands for the connected devices
     * Rules:
     * Should provide power usage information
     * Should Provide devices
     * Should allow communication between devices
     * 
     */
    public class BaseManager
    {
        private bool _hasBreakerTripped;
        private string _baseName;
        public string BaseID { get; set; }
        public static Action<BaseManager> OnManagerCreated { get; set; }
        private readonly Dictionary<string, FcsDevice> _registeredDevices;
        private float _timeLeft = 1f;
        private PowerSystem.Status _prevPowerState;

        public SubRoot Habitat { get; set; }
        
        public bool IsVisible { get; set; }

        public static List<BaseManager> Managers { get; } = new List<BaseManager>();
        public Action<PowerSystem.Status> OnPowerStateChanged { get; set; }

        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            BaseID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            _registeredDevices = new Dictionary<string, FcsDevice>();
            _baseName = GetDefaultName();
            Player_Patch.OnPlayerUpdate += PowerConsumption;
            Player_Patch.OnPlayerUpdate += PowerStateCheck;
            
        }

        private void PowerStateCheck()
        {
            if (_prevPowerState != Habitat.powerRelay.GetPowerStatus())
            {
                _prevPowerState = Habitat.powerRelay.GetPowerStatus();
                OnPowerStateChanged?.Invoke(Habitat.powerRelay.GetPowerStatus());
            }
        }

        #endregion

        public void ToggleBreaker()
        {
            _hasBreakerTripped = !_hasBreakerTripped;
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            QuickLogger.Debug($"Created new base manager with ID {manager.BaseID}",true);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            OnManagerCreated?.Invoke(manager);
            return manager;
        }
        
        public static BaseManager FindManager(SubRoot subRoot)
        {
#if DEBUG
            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");
#endif
            var baseManager = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            return baseManager ?? CreateNewManager(subRoot);
        }

        public static BaseManager FindManager(string instanceID)
        {
            QuickLogger.Debug($"InstanceID: {instanceID} Base Count: {Managers.Count}");
            var manager = Managers.Find(x => x.BaseID == instanceID);
            return manager;
        }

        /// <summary>
        /// Find the manager this gameObject is attached to.
        /// </summary>
        /// <param name="gameObject">The gameObject the we are trying to find the base for</param>
        /// <returns></returns>
        public static BaseManager FindManager(GameObject gameObject)
        {
            var subRoot = gameObject.GetComponentInParent<SubRoot>() ?? gameObject.GetComponentInChildren<SubRoot>();

            return subRoot != null ? FindManager(subRoot) : null;
        }

        public static void RemoveDestroyedBases()
        {
            for (int i = Managers.Count - 1; i > -1; i--)
            {
                if (Managers[i].Habitat == null)
                {
                    Managers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Gets the stored base Name from the
        /// </summary>
        /// <returns></returns>
        public string GetBaseName()
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
        }

        /// <summary>
        /// Creates a default base name and number index based on the count of items
        /// </summary>
        /// <returns></returns>
        public string GetDefaultName()
        {
            if (Habitat == null || Managers == null) return "Unknown";

            if (Habitat.isCyclops)
            {
                var count = Managers.Count(x => x.Habitat.isCyclops);
                return $"Cyclops {count}";
            }
            else
            {
                var count = Managers.Count(x => x.Habitat.isBase);
                return $"Base {count}";
            }
        }

        public void SendBaseMessage(string baseMessage)
        {
            QuickLogger.Message(baseMessage,true);
        }

        public void RegisterDevice(FcsDevice device)
        {
            if (!_registeredDevices.ContainsKey(device.UnitID))
            {

                _registeredDevices.Add(device.UnitID, device);
                QuickLogger.Debug($"Registered device: {device.UnitID}", true);
            }
        }

        private void PowerConsumption()
        {
            if (_registeredDevices == null) return;
            _timeLeft -= DayNightCycle.main.deltaTime;
            if (_timeLeft <= 0)
            {
                //Take power from the base
                for (int i = _registeredDevices.Count - 1; i >= 0; i--)
                {
                    var device = _registeredDevices.ElementAt(i);
                    if (device.Value.DoesTakePower && device.Value.IsOperational && Habitat.powerRelay != null)
                    {
                        var num = 1f * DayNightCycle.main.dayNightSpeed;
                        Habitat.powerRelay.ConsumeEnergy(device.Value.GetPowerUsage() * num, out float amountConsumed);
                        QuickLogger.Debug($"Base {_baseName} consumed {amountConsumed} || requested {device.Value.GetPowerUsage()} || Device {device.Value.UnitID}", true);
                    }
                }

                _timeLeft = 1f;
            }
        }

        public bool HasEnoughPower(float power)
        {
            if(Habitat.powerRelay == null) {QuickLogger.Debug("Habitat is null");}
            if (Habitat.powerRelay == null || Habitat.powerRelay.GetPower() < power) return false;
            return true;
        }

        public void UnRegisterDevice(FcsDevice device)
        {
            if (_registeredDevices.ContainsKey(device.UnitID))
                _registeredDevices?.Remove(device.UnitID);
        }

        public IEnumerable<FcsDevice> GetDevices(string tabID)
        {
            foreach (KeyValuePair<string, FcsDevice> device in _registeredDevices)
            {
                QuickLogger.Debug($"Checking registered devices: Device: {device.Key} || Key {tabID}");
                if (device.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase))
                {
                    QuickLogger.Debug($"Key Match",true);
                    yield return device.Value;
                }
            }
        }

        public bool DeviceBuilt(string tabID)
        {
            return _registeredDevices.Any(x => x.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase));
        }
    }
}
