using FCS_DeepDriller.Buildable;
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

        private float _charge;
        private float _capacity = 1000f;
        private float _passedTime;
        private bool _produceSolarPower;
        private float _maxDepth = 200f;
        private AnimationCurve _depthCurve;
        internal Action<bool> OnPowerUpdate;
        private void Update()
        {
            if (!_mono.IsConstructed) return;

            if (_powerState != FCSPowerStates.Powered || _module == DeepDrillModules.None || _charge < 1f)
            {
                if (_hasPower)
                {
                    HasPower = false;
                    SetPowerState(FCSPowerStates.Unpowered);
                }

                _produceSolarPower = false;
                QuickLogger.Debug($"Power Conditions Not Met: PS{_powerState} || M {_module} || C {_charge}");

                return;
            }

            ProduceSolarPower();

            if (DayNightCycle.main == null) return;

            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= 1)
            {
                _charge = Mathf.Clamp(_charge - FCSDeepDrillerBuildable.DeepDrillConfig.PowerDraw, 0, _capacity);
                _passedTime = 0.0f;

                if (!_hasPower)
                {
                    HasPower = true;
                    SetPowerState(FCSPowerStates.Powered);
                }

                QuickLogger.Debug($"Current Charge: {_charge} || Current Capacity: {_capacity}");
            }
        }

        private void ProduceSolarPower()
        {
            if (!_produceSolarPower) return;
            _charge = Mathf.Clamp(_charge + this.GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f, 0f, _capacity);
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

        internal void SetModule(DeepDrillModules module)
        {
            _module = module;

            switch (module)
            {
                case DeepDrillModules.None:
                    _capacity = 0;
                    break;
                case DeepDrillModules.Solar:
                    _capacity = FCSDeepDrillerBuildable.DeepDrillConfig.SolarCapacity;
                    _produceSolarPower = true;
                    break;
                case DeepDrillModules.Battery:
                    _charge = 1000f;
                    break;
            }
        }

        internal bool GetHasPower()
        {
            return _hasPower;

        }

    }
}
