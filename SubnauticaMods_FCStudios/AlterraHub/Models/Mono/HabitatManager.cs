using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono
{
    public class HabitatManager : MonoBehaviour
    {
        private HashSet<FCSDevice> _registeredDevices = new();
        private string _baseFriendlyID => GetBaseFriendlyName();
        private SubRoot _habitat;
        private Base _baseComponent;
        private string _prefabID;
        private int _baseID = -1;
        private string _baseName;
        
        private string GetBaseFriendlyName()
        {
            var baseType = _habitat.isBase ? "Base" : "Cyclops";
            return $"{baseType} {_baseID:D3}";
        }

        internal bool HasDevice(string prefabID)
        {
            return _registeredDevices.Any(x => x.GetPrefabID().Equals(prefabID));
        }

        internal void RegisterDevice(FCSDevice device)
        {
            _registeredDevices.Add(device);
        }

        internal void UnRegisterDevice(FCSDevice device)
        {
            _registeredDevices.Remove(device);
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

        public override string ToString()
        {
            return _baseName;
        }

        internal int GetBaseID() => _baseID;

        internal void SetBaseID(int id)
        {
            if (_baseID != -1) return;
            
            _baseID = id;
        }
    }
}
