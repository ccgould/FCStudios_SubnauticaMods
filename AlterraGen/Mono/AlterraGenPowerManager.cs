using System;
using System.Collections.Generic;
using System.Linq;
using AlterraGen.Configuration;
using AlterraGen.Enumerators;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace AlterraGen.Mono
{
    internal class AlterraGenPowerManager : MonoBehaviour, IFCSStorage,IFCSPowerManager
    {
        private float _toConsume;
		private const float powerPerSecond = 0.8333333f;
        private readonly List<TechType> _container = new List<TechType>(MaxSlots);
        private readonly List<TechType> _toRemove = new List<TechType>();
        private AlterraGenController _mono;
        private FCSPowerStates _prevPowerState;
        private const int MaxSlots = 8;
        private PowerRelay _powerRelay;
        private PowerSource _powerSource;
        private float _storedPower;
        private FCSPowerStates _powerState = FCSPowerStates.Powered;

        internal bool ProducingPower
        {
            get
            {
                var value = _mono != null && _mono.IsConstructed && _container.Count > 0 && PowerState != FCSPowerStates.Tripped && GameModeUtils.RequiresPower();
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
                float num = powerPerSecond * DayNightCycle.main.deltaTime;
                float num2 = this._powerSource.maxPower - this._powerSource.power;
                if (num2 > 0f)
                {
                    if (num2 < num)
                    {
                        num = num2;
                    }
                    float amount = this.ProducePower(num);
                    float num3;
                    this._powerSource.AddEnergy(amount, out num3);
                }
            }
        }

        private float ProducePower(float requested)
        {
            float num = 0f;
            if (requested > 0f && this._container.Count > 0)
            {
                _toConsume += requested;
                num = requested;
                foreach (TechType inventoryItem in _container)
                {
                    TechType techType = inventoryItem;
                    float num2 = 0f;
                    if (FuelTypes.Charge.TryGetValue(techType, out num2) && this._toConsume >= num2)
                    {
                        _toConsume -= num2;
                        _toRemove.Add(techType);
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
                    num -= _toConsume;
                    _toConsume = 0f;
                }
            }
            return num;
        }

        #region IFCSStorage

        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount, TechType techType)
        {
            throw new NotImplementedException();
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
                if (FuelTypes.Charge.ContainsKey(techType))
                {
                    flag = true;
                }
            }
            if (!flag && verbose)
            {
                ErrorMessage.AddMessage(Language.main.Get("BaseBioReactorCantAddItem"));
            }
            return flag;
		}

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            var lookup = _container?.ToLookup(x => x).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
		}

        public Action<int, int> OnContainerUpdate { get; set; }
        
        public bool ContainsItem(TechType techType)
        {
            return _container.Contains(techType);
        }

        #endregion

        #region IFCSPowerManager

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

        internal void LoadFromSave(SaveDataEntry saveData)
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

        public float GetPowerUsagePerSecond()
        {
            return 0f;
        }

        public float GetDevicePowerCharge()
        {
            return _powerSource?.power ?? 0f;
        }

        public float GetDevicePowerCapacity()
        {
            return _powerSource?.maxPower ?? 0f;
        }

        public FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        public void TogglePowerState()
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

        public void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }

        public bool IsDevicePowerFull()
        {
            if (_powerSource == null) return true;
            return _powerSource.power >= _powerSource.maxPower;
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            var result = _powerSource.ModifyPower(amount, out var consumedO);
            consumed = consumedO;
            return result;
        }

        #endregion
    }
}
