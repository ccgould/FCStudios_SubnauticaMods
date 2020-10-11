using FCSTechFabricator.Abstract;
using FCSTechFabricator.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Policy;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using FCSTechFabricator.Enums;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCSTechFabricator.Components
{
    public class FCSConnectableDevice : MonoBehaviour
    {
        private PrefabIdentifier _prefabId;
        private NameController _nameController;
        private FCSController _mono;
        private IFCSStorage _storage;
        private TechType _techtype;
        private FCSPowerManager _powerManager;
        private bool _canBeVisible;
        private bool _isVisible = false;
        private bool _isBase { get; set; }
        public string UnitID { get; set; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnIsVisibleToggled?.Invoke(this,value);
            }
        }
        //TODO Save Is Visible

        /// <summary>
        /// Initializes the FCSConnectable but doesnt register it to the tech fabricator (Registration must be dont by hand)
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="storage"></param>
        /// <param name="powerManager"></param>
        /// <param name="modID"></param>
        /// <param name="canBeVisible"></param>
        public void Initialize(FCSController mono, IFCSStorage storage, FCSPowerManager powerManager,bool canBeVisible = false)
        {
            _mono = mono;
            _storage = storage;
            _powerManager = powerManager;
            _canBeVisible = canBeVisible;
        }

        /// <summary>
        /// Initializes the FCSConnectable and registers it to the tech fabricator
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="storage"></param>
        /// <param name="powerManager"></param>
        /// <param name="deviceID"></param>
        /// <param name="canBeVisible"></param>
        public void Initialize(FCSController mono, IFCSStorage storage, FCSPowerManager powerManager,string deviceID, bool canBeVisible = false)
        {
            _mono = mono;
            _storage = storage;
            _powerManager = powerManager;
            _canBeVisible = canBeVisible;
            FcTechFabricatorService.PublicAPI.RegisterDevice(this, GetPrefabIDString(), deviceID);
        }

        
        public void InitializeBase(FCSController mono, IFCSStorage storage, FCSPowerManager powerManager,
            PrefabIdentifier prefabIdentifier)
        {
            _mono = mono;
            _storage = storage;
            _powerManager = powerManager;
            _prefabId = prefabIdentifier;
            _isBase = true;
        }

        public bool IsBase()
        {
            return _isBase;
        }

        /// <summary>
        /// Only us this if this Connectable is a base
        /// </summary>
        public void SetTechType(TechType techType)
        {
            if (_isBase)
            {
                _techtype = techType;
            }
        }

        public TechType GetTechType()
        {
            if (_isBase)
            {
                return _techtype;
            }
            if (_techtype == TechType.None)
            {
                var techTag = GetComponentInParent<TechTag>() ?? GetComponentInChildren<TechTag>();
                _techtype = techTag == null ? TechType.None : techTag.type;
            }

            QuickLogger.Debug($"FCSConnectable TechType: {_techtype}",true);
            return _techtype;
        }
        
        public int GetContainerFreeSpace => _storage.GetContainerFreeSpace;
        public Action<FCSConnectableDevice, bool> OnIsVisibleToggled { get; set; }

        public string GetPrefabIDString()
        {
            if (_prefabId == null)
            {
                _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            }
            return _prefabId?.Id;
        }

        public void SetPrefabID(PrefabIdentifier prefabid)
        {
            _prefabId = prefabid;
        }

        public float GetDevicePowerCharge()
        {
            return _powerManager?.GetDevicePowerCharge() ?? 0f;
        }

        public float GetDevicePowerCapacity()
        {
            return _powerManager?.GetDevicePowerCapacity() ?? 0f;
        }

        public void ToggleDevicePowerState()
        {
            _powerManager?.TogglePowerState();
        }

        public void SetDevicePowerState(FCSPowerStates state)
        {
            _powerManager?.SetPowerState(state);
        }

        public FCSPowerManager GetPowerManager()
        {
            return _powerManager;
        }

        public float GetPowerUsagePerSecond()
        {
            return _powerManager?.GetPowerUsagePerSecond() ?? 0f;
        }

        public bool IsDevicePowerFull()
        {
            return _powerManager?.IsDevicePowerFull() ?? true;
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            var result = _powerManager.ModifyPower(amount, out var consumedOut);
            consumed = consumedOut;
            return result;
        }

        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            return _storage.CanBeStored(amount,techType);
        }

        public virtual bool AddItemToContainer(InventoryItem item, out string reason)
        {
            reason = null;//Todo add needed thibgsb
            return _storage.AddItemToContainer(item);
        }

        public virtual void SetNameControllerTag(object obj)
        {
            if (_nameController != null)
            {
                _nameController.Tag = obj;
            }
        }

        public string GetName()
        {
            return _nameController != null ? _nameController.GetCurrentName() : string.Empty;
        }
        
        public void Start()
        {
            if (_nameController == null)
                _nameController = GetComponentInParent<NameController>();
        }

        public void SubscribeToNameChange(Action<string, NameController> method)
        {
            if (_nameController != null)
            {
                _nameController.OnLabelChanged += method;
            }
        }

        public void OnDestroy()
        {

        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount,bool destroy = false)
        {
            var item = _storage.RemoveItemFromContainer(techType, amount);
            if (!destroy) return item;
            Destroy(item);
            return null;
        }

        public Dictionary<TechType,int> GetItemsWithin()
        {
            return _storage.GetItemsWithin();
        }

        public IFCSStorage GetStorage()
        {
            return _storage;
        }

        public bool ContainsItem(TechType techType)
        {
           return _storage.ContainsItem(techType);
        }

        public virtual int GetItemCount(TechType techType)
        {  var items = _storage.GetItemsWithin();
            return items.ContainsKey(techType) ? items[techType] : 0;
        }

        public virtual bool IsConstructed()
        {
            return _mono.IsConstructed;
        }

        public virtual bool IsOperational()
        {
            return _powerManager?.GetPowerState() == FCSPowerStates.Powered && IsConstructed();
        }

        public FCSController GetController()
        {
            return _mono;
        }

        public bool CanBeVisible()
        {
            return _canBeVisible;
        }

        public SubRoot GetHabitat()
        {
            return _mono.GetComponentInParent<SubRoot>() ?? _mono.GetComponentInChildren<SubRoot>();
        }
    }
}
