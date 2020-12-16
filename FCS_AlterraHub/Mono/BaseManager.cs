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
        private Dictionary<string,TechLight> _baseTechLights;

        public SubRoot Habitat { get; set; }

        public bool IsVisible { get; set; }

        public static List<BaseManager> Managers { get; } = new List<BaseManager>();
        public static Dictionary<string,TrackedLight> GlobalTrackedLights { get; } = new Dictionary<string, TrackedLight>();
        public Action<PowerSystem.Status> OnPowerStateChanged { get; set; }
        public bool IsBaseExternalLightsActivated { get; set; }

        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            BaseID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            _registeredDevices = new Dictionary<string, FcsDevice>();
            _baseTechLights = new Dictionary<string, TechLight>();
            _baseName = GetDefaultName();
            Player_Patch.OnPlayerUpdate += PowerConsumption;
            Player_Patch.OnPlayerUpdate += PowerStateCheck;

        }

        private void PowerStateCheck()
        {
            if (Habitat?.powerRelay == null) return;
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
            QuickLogger.Debug($"Created new base manager with ID {manager.BaseID}", true);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            OnManagerCreated?.Invoke(manager);
            return manager;
        }

        public static BaseManager FindManager(SubRoot subRoot)
        {
            var baseManager = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            return baseManager ?? CreateNewManager(subRoot);
        }

        public static BaseManager FindManager(string instanceID)
        {
            var manager = Managers.Find(x => x.BaseID == instanceID);
            return manager;
        }

        public static BaseManager FindManager(TechLight techLight)
        {
            return GlobalTrackedLights.FirstOrDefault(x => x.Value.TechLight == techLight).Value.Manager;
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
            QuickLogger.Message(baseMessage, true);
        }

        public void RegisterDevice(FcsDevice device)
        {
            if (!_registeredDevices.ContainsKey(device.UnitID))
            {

                _registeredDevices.Add(device.UnitID, device);
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
                    }
                }

                _timeLeft = 1f;
            }
        }

        public bool HasEnoughPower(float power)
        {
            if (Habitat.powerRelay == null)
            {
                QuickLogger.DebugError("Habitat is null");
            }

            if (!GameModeUtils.RequiresPower()) return true;

            if (Habitat.powerRelay == null || Habitat.powerRelay.GetPower() < power) return false;
            return true;
        }

        public void UnRegisterDevice(FcsDevice device)
        {
            if (_registeredDevices.ContainsKey(device.UnitID))
                _registeredDevices?.Remove(device.UnitID);
        }

        public void UnRegisterDevice(TechLight device)
        {
            var prefabId = device.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            if (!string.IsNullOrEmpty(prefabId) && _baseTechLights.ContainsKey(prefabId))
            {
                _baseTechLights.Remove(prefabId);
                RemoveGlobalTrackedLight(prefabId);
            }
        }

        public IEnumerable<FcsDevice> GetDevices(string tabID)
        {
            foreach (KeyValuePair<string, FcsDevice> device in _registeredDevices)
            {
                if (device.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase))
                {
                    yield return device.Value;
                }
            }
        }

        public bool DeviceBuilt(string tabID)
        {
            return _registeredDevices.Any(x => x.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase));
        }

        public void NotifyByID(string modID, string commandMessage)
        {
            if (!string.IsNullOrEmpty(modID))
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI
                    .GetRegisteredDevicesOfId(modID))
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
            else
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices()
                )
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
        }

        public static void GlobalNotifyByID(string modID, string commandMessage)
        {
            if (!string.IsNullOrEmpty(modID))
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI
                    .GetRegisteredDevicesOfId(modID))
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
            else
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices()
                )
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
        }

        public float GetPower()
        {
            if (Habitat.powerRelay != null)
            {
                return Habitat.powerRelay.GetPower();
            }

            return 0;
        }

        public void RegisterDevice(TechLight device)
        {
            var prefabId = device.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            if (!string.IsNullOrEmpty(prefabId) && !_baseTechLights.ContainsKey(prefabId))
            {
                _baseTechLights.Add(prefabId,device);
                var controller = device.gameObject.AddComponent<FCSTechLigtController>();
                controller.Initialize(this);
                AddGlobalTrackedLight(prefabId, device,null, this);
            }
        }

        private static void AddGlobalTrackedLight(string prefabId, TechLight techLight, BaseSpotLight spotLight, BaseManager baseManager)
        {
            if (!GlobalTrackedLights.ContainsKey(prefabId))
            {
                GlobalTrackedLights.Add(prefabId,new TrackedLight{Manager = baseManager, TechLight = techLight, SpotLight = spotLight});
            }
        }

        private static void RemoveGlobalTrackedLight(string prefabId)
        {
            if (GlobalTrackedLights.ContainsKey(prefabId))
            {
                GlobalTrackedLights.Remove(prefabId);
            }
        }

        public int GetDevicesCount(string unitTabId)
        {
            var i = 0;
            foreach (var device in _registeredDevices)
            {
                if (device.Value.UnitID.StartsWith(unitTabId))
                {
                    i++;
                }
            }

            return i;
        }
    }

    public struct TrackedLight 
    {
        public BaseManager Manager { get; set; }
        public TechLight TechLight { get; set; }
        public BaseSpotLight SpotLight { get; set; }
    }

    public class FCSTechLigtController : MonoBehaviour
    {
        private BaseManager _manager;

        public void Initialize(BaseManager manager)
        {
            _manager = manager;
        }


        private void Update()
        {

        }
    }
}
