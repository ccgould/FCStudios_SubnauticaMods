using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<string, FcsDevice> _registeredDevices;

        public SubRoot Habitat { get; set; }
        public bool IsVisible { get; set; }

        public static List<BaseManager> Managers { get; } = new List<BaseManager>();

        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            BaseID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            _registeredDevices = new Dictionary<string, FcsDevice>();
            _baseName = GetDefaultName();
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
    }
}
