using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.AlterraGen.Buildables;
using FCS_EnergySolutions.Mods.AlterraGen.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraGen.Mono
{
    internal class AlterraGenPowerManager : FCSPowerManager, IFCSStorage
    {
        private float _toConsume;
        private const float PowerPerSecond = 1.167f; // Old Value 0.8333333f;
        private readonly List<TechType> _container = new();
        private readonly List<TechType> _toRemove = new();
        private AlterraGenController _mono;
        private FCSPowerStates _prevPowerState;
        private PowerRelay _powerRelay;
        private PowerSource _powerSource;
        private float _storedPower;
        private FCSPowerStates _powerState = FCSPowerStates.Powered;
        public float _targetEnergyValue { get; private set; }
        private float _multiplier;
        internal int MaxSlots => 9;

        internal bool ProducingPower
        {
            get
            {
                var value = _mono != null && _mono.IsConstructed && _container.Count > 0 && PowerState != FCSPowerStates.Tripped && UWEHelpers.RequiresPower();
                return value;
            }
        }
		internal Action<AlterraGenPowerManager> OnPowerUpdateCycle { get; set; }

        internal FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value;

                _mono?.DisplayManager?.GotoPage(value == FCSPowerStates.Powered
                    ? AlterraGenPages.HomePage
                    : AlterraGenPages.PoweredOffPage);
            }
        }

        private void Start()
        {
            _mono = gameObject.GetComponent<AlterraGenController>();
            this._powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
            if (this._powerRelay == null)
            {
                Debug.LogError("AlterraGen could not find PowerRelay", this);
            }
            this._powerSource = base.GetComponent<PowerSource>();
            if (this._powerSource == null)
            {
                Debug.LogError("AlterraGen could not find PowerSource", this);
            }

			InvokeRepeating(nameof(UpdateSubscribers),0.5f,0.5f);
        }
        
        private void UpdateSubscribers()
        {
            OnPowerUpdateCycle?.Invoke(this);
		}
        
        private void Update()
        {
            if (this.ProducingPower)
            {
                float num = PowerPerSecond * DayNightCycle.main.deltaTime;

                float num2 = _powerSource.maxPower - _powerSource.power;
                if (num2 > 0f)
                {
                    if (num2 < num)
                    {
                        num = num2;
                    }
                    float amount = this.ProducePower(num);
                    _powerSource.AddEnergy(amount, out var num3);
                }
            }
        }

        private float GetMultiplier(TechType techType)
        {
            var multiplier = 0f;

#if SUBNAUTICA
            var size = CraftData.GetItemSize(techType);
#else
                    var size = TechData.GetItemSize(techType);
#endif

            if (size.x > 1 || size.y > 1)
            {
                multiplier = 2.2f;
            }
            else
            {
                multiplier = 1f;
            }
            return multiplier;
        }

        private float ProducePower(float requested)
        {
            float modifiedAmount = 0f;
            if (requested > 0f && this._container.Count > 0)
            {
                _toConsume += requested;

                modifiedAmount = requested;
                
                foreach (TechType techType in _container)
                {
                    if (Mod.GetBioChargeValues().TryGetValue(techType, out var value))
                    {
                        _multiplier = GetMultiplier(techType);
                        _targetEnergyValue = _multiplier * value;

                        if (_toConsume >= _targetEnergyValue)
                        {
                            _toConsume -= _targetEnergyValue;
                            _toRemove.Add(techType);
                        }
                    }
                }


                for (int i = _toRemove.Count - 1; i >= 0; i--)
                {
                    TechType techType = _toRemove[i];
                    _container.Remove(techType);
                    OnContainerUpdate?.Invoke(_container.Count, MaxSlots);
				}
                
                _toRemove.Clear();
                
                if (_container.Count == 0)
                {
                    modifiedAmount -= _toConsume;
                    _toConsume = 0f;
                }
            }
            return modifiedAmount;
        }

        #region IFCSStorage

        public int GetContainerFreeSpace => MaxSlots - _container.Count;
        public bool IsFull => _container.Count >= MaxSlots;
        public bool CanBeStored(int amount, TechType techType)
        {
            
            if (!Mod.GetBioChargeValues().ContainsKey(techType))
            {
                QuickLogger.ModMessage(AlterraGenBuildable.NotAllowedItem());
                return false;
            }

            var storageResult = !IsFull && amount + _container.Count <= MaxSlots;
            
            return storageResult;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
			_container.Add(item.item.GetTechType());
			Destroy(item.item.gameObject);
            OnContainerUpdate?.Invoke(_container.Count,MaxSlots);

			return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                flag = CanBeStored(_mono.DumpContainer.GetCount() + 1, techType);
            }

            if (!flag && verbose)
            {
                QuickLogger.ModMessage(AlterraGenBuildable.StorageFullMessage());
            }
            return flag;
		}

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
        {
            OnContainerRemoveItem(_mono, techType);
            return null;
        }

        public bool IsAllowedToAdd(TechType techType,bool verbose = false)
        {
            bool flag = false;
            if (techType != TechType.None)
            {
               flag = CanBeStored(_mono.DumpContainer.GetCount() + 1, techType);
            }

            if (!flag && verbose)
            {
                QuickLogger.ModMessage(AlterraGenBuildable.StorageFullMessage());
            }
            return flag;
        }


        public Dictionary<TechType, int> GetItemsWithin()
        {
            var lookup = _container?.ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
		}

        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }


        public bool ContainsItem(TechType techType)
        {
            return _container.Contains(techType);
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return _container.Count;
        }

        #endregion

        #region FCSPowerManager

        internal string GetTotalPowerString()
        {
            return _powerSource == null ? "0 kW" : $"{Mathf.RoundToInt(_powerSource.power)} kW";
        }

        internal float GetStoredPower()
        {
            return _storedPower;
        }

        internal PowerSource GetBatteryData()
        {
            return _powerSource == null ? null : _powerSource;
        }

        internal float GetToConsume()
        {
            return _toConsume;
        }

        internal void LoadFromSave(AlterraGenDataEntry saveData)
        {
            if (saveData.Storage != null) 
            {
                foreach (KeyValuePair<TechType, int> pair in saveData.Storage)
                {
                    for (int i = 0; i < pair.Value; i++)
                    {
                        _container.Add(pair.Key);
                    }
                }
            }

            _storedPower = saveData.StoredPower;
            _toConsume = saveData.ToConsume;
            PowerState = saveData.PowerState;
            _powerSource?.SetPower(saveData.Power);
            OnContainerUpdate?.Invoke(_container.Count,MaxSlots);
        }

        internal float GetPowerSourcePower()
        {
            return _powerSource.power;
        }
        
        public override float GetDevicePowerCharge()
        {
            return _powerSource?.power ?? 0f;
        }

        public override float GetDevicePowerCapacity()
        {
            return _powerSource?.maxPower ?? 0f;
        }

        public override FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        public override void TogglePowerState()
        {
            PowerState = PowerState == FCSPowerStates.Powered ? FCSPowerStates.Tripped : FCSPowerStates.Powered;

            if (PowerState == FCSPowerStates.Tripped)
            {
                _storedPower = _powerSource.power;
                _powerSource.SetPower(0f);
            }
            else
            {
                _powerSource.SetPower(_storedPower);
                _storedPower = 0f;
            }
        }

        public override void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }

        public override bool IsDevicePowerFull()
        {
            if (_powerSource == null) return true;
            return _powerSource.power >= _powerSource.maxPower;
        }

        public override bool ModifyPower(float amount, out float consumed)
        {
            var result = _powerSource.ModifyPower(amount, out var consumedO);
            consumed = consumedO;
            return result;
        }

        public override float GetPowerUsagePerSecond() => PowerPerSecond;

        #endregion
    }
}
