using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerPowerHandler : MonoBehaviour
    {
        private FCSPowerStates _powerState;
        private FCSDeepDrillerController _mono;
        private DeepDrillModules _module;
        private bool _hasPower;
        private bool HasPower
        {
            get => _hasPower;
            set
            {
                _hasPower = value;
                OnPowerUpdate?.Invoke(value);
            }
        }
        private float _passedTime;
        private bool _produceSolarPower;
        private float _maxDepth = 200f;
        private AnimationCurve _depthCurve;
        private readonly DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();

        internal Action<bool> OnPowerUpdate;

        private void Awake()
        {
            var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.PrecursorIonPowerCell));
            var battery = prefab.gameObject.GetComponent<Battery>();
            battery._charge = 0f;
            battery._capacity = FCSDeepDrillerBuildable.DeepDrillConfig.SolarCapacity;
            _powerBank.Solar.Initialize(prefab.gameObject.GetComponent<Pickupable>().Pickup(false));
        }

        private void Update()
        {
            var hasPowerModule = _mono.DeepDrillerModuleContainer.HasPowerModule(out var module);

            //QuickLogger.Debug($"Has Power Module {hasPowerModule} || Mount is {module}");

            if (!_mono.IsConstructed) return;

            _produceSolarPower = module == DeepDrillModules.Solar;

            _module = module;

            ProduceSolarPower();

            if (_powerState != FCSPowerStates.Powered || _powerBank.GetCharge(_module) < FCSDeepDrillerBuildable.DeepDrillConfig.PowerDraw || !hasPowerModule)
            {
                if (_hasPower)
                {
                    HasPower = false;
                    QuickLogger.Debug($"Power Conditions Not Met: PS{_powerState} || M {_module} || C {_powerBank.GetCharge(_module)}");
                }

                return;
            }

            if (DayNightCycle.main == null) return;

            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= 1)
            {
                _powerBank.ConsumePower(_module);
                _passedTime = 0.0f;

                if (!_hasPower)
                {
                    HasPower = true;
                    SetPowerState(FCSPowerStates.Powered);
                }

                QuickLogger.Debug($"Current Charge: {_powerBank.GetCharge(_module)} || Current Capacity: {_powerBank.GetCapacity(_module)}");
            }
        }

        private void ProduceSolarPower()
        {
            if (!_produceSolarPower) return;
            _powerBank.SetSolarCharge(Mathf.Clamp(_powerBank.GetCharge(_module) + GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.50f * 5f, 0f, _powerBank.GetCapacity(_module)));
            QuickLogger.Debug($"Current Solar Charge: {_powerBank.Solar.Battery.charge} || Current Solar Capacity: {_powerBank.Solar.Battery.capacity}");
        }

        private float GetRechargeScalar()
        {
            return this.GetDepthScalar() * this.GetSunScalar();
        }

        private float GetDepthScalar()
        {
            float time = Mathf.Clamp01((_maxDepth - Ocean.main.GetDepthOf(base.gameObject)) / _maxDepth);

            return _depthCurve.Evaluate(time);
        }

        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar();
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
        }

        internal FCSPowerStates GetPowerState()
        {
            return _powerState;
        }

        internal void SetPowerState(FCSPowerStates state)
        {
            _powerState = state;
        }

        internal void AddBattery(Pickupable battery)
        {
            _powerBank.AddBattery(battery);
        }
        internal void RemoveBattery(Pickupable battery)
        {
            _powerBank.RemoveBattery(battery);
        }

        internal void Battery(Pickupable battery)
        {
            var newBattery = new DeepDrillerPowerData.PowerUnitData();
            newBattery.Initialize(battery);
            _powerBank.Batteries.Add(newBattery);
        }

        internal void SetPower(DeepDrillerPowerData data)
        {
            switch (_module)
            {
                case DeepDrillModules.Battery:
                    break;
                case DeepDrillModules.Solar:
                    _powerBank.SetSolarCharge(data.Solar.Charge);
                    break;
            }
        }

        internal float GetCharge()
        {
            return _powerBank.GetCharge(_module);
        }

        internal bool GetHasPower()
        {
            return _hasPower;

        }

        public string GetSolarPowerData()
        {
            return $"Solar panel (sun: {Mathf.RoundToInt(GetRechargeScalar() * 100f)}% charge {Mathf.RoundToInt(_powerBank.Solar.Battery.charge)}/{Mathf.RoundToInt(_powerBank.Solar.Battery.capacity)}";
        }
    }
}
