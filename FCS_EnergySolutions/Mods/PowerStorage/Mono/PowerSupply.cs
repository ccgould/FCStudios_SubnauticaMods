using FCS_EnergySolutions.Mods.PowerStorage.Enums;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.PowerStorage.Mono
{

    internal class PowerSupply : MonoBehaviour, IPowerInterface
    {
        private PowerStorageController _mono;
        private bool _allowedToDisCharge;
        private bool _performDischargeBuffer;
        private float _timeLeft = 5f;
        private float nextChargeAttemptTimer;
        private float nextDischargeAttemptTimer;
        private PowerCellCharger _powerCharger => _mono?.PowercellCharger;

        private void Update()
        {
            if (Time.deltaTime == 0f)
            {
                return;
            }

            var mode = _mono.GetMode();

            if (mode != PowerChargerMode.Auto)
            {
                if (mode == PowerChargerMode.ChargeMode)
                {
                    _allowedToDisCharge = false;
                    _powerCharger.SetAllowedToCharge(true);
                }
                else
                {
                    _allowedToDisCharge = true;
                    _powerCharger.SetAllowedToCharge(false);
                }
                return;
            }
            
            if (CheckPowerGoal())
            {
                AttemptDischarge();
            }

            if (CheckChargeGoal())
            {
                AttemptCharge();
            }
        }

        private bool CheckPowerGoal()
        {
            return _mono.CalculatePowerPercentage() <= 10f || _mono.CalculateBasePower() <= 0.1f;
        }
        
        private void AttemptDischarge()
        {
            if (nextDischargeAttemptTimer > 0f)
            {
                nextDischargeAttemptTimer -= DayNightCycle.main.deltaTime;
                if (nextDischargeAttemptTimer < 0f)
                {
                    nextDischargeAttemptTimer = 0f;
                }
            }


            if (nextDischargeAttemptTimer <= 0f)
            {
                if (CheckPowerGoal())
                {
                    _allowedToDisCharge = true;
                    _powerCharger.SetAllowedToCharge(false);
                }

                nextDischargeAttemptTimer = 5f;
            }
            
            if (nextDischargeAttemptTimer >= 0f)
            {
                int num8 = Mathf.CeilToInt(nextDischargeAttemptTimer);
                //QuickLogger.Debug($"Attempt DisCharge in: {num8}", true);
            }
        }        
        
        private void AttemptCharge()
        {
            if (nextChargeAttemptTimer > 0f)
            {
                nextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
                if (nextChargeAttemptTimer < 0f)
                {
                    nextChargeAttemptTimer = 0f;
                }
            }
            
            if (nextChargeAttemptTimer <= 0f)
            {
                if (CheckChargeGoal())
                {
                    _allowedToDisCharge = false;
                    _powerCharger.SetAllowedToCharge(true);
                }
                nextChargeAttemptTimer = 5f;
            }


            if (nextChargeAttemptTimer >= 0f)
            {
                int num8 = Mathf.CeilToInt(nextChargeAttemptTimer); 
                //QuickLogger.Debug($"Attempt Charge in: {num8}",true);
            }
        }

        private bool CheckChargeGoal()
        {
            return _mono.CalculatePowerPercentage() >= 40f;
        }

        internal void Initialize(PowerStorageController mono)
        {
            _mono = mono;
        }

        public float GetPower()
        {
            if (!_allowedToDisCharge) return 0;
            return _powerCharger?.GetTotal() ?? 0;
        }

        public float GetMaxPower()
        {
            if (!_allowedToDisCharge) return 0;
            return _powerCharger?.GetCapacity() ?? 0;
        }

        internal string GetPowerString()
        {
            return _powerCharger?.GetTotal() == null ? "0/0" : $"{Mathf.RoundToInt(_powerCharger.GetTotal())}/{Mathf.RoundToInt(_powerCharger.GetCapacity())}";
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            if (_powerCharger == null || !_allowedToDisCharge) return false;

            var currentPower = _powerCharger.GetTotal();
            var currentCapacity = _powerCharger.GetCapacity();

            float num = currentPower;
            bool result;

            if (amount >= 0f)
            {
                result = (amount <= currentCapacity - currentPower);
            }
            else
            {
                result = (currentPower >= -amount);
            }

            if (GameModeUtils.RequiresPower())
            {
                consumed = _powerCharger.GetTotal() + amount - num;
                _powerCharger.RemoveCharge(consumed);
            }
            else
            {
                consumed = amount;
            }
            return result;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }

        private void OnDestroy()
        {
            _powerCharger?.RemovePowerSource(this);
        }

        public void SetAllowedToCharge(bool value)
        {
            _powerCharger.SetAllowedToCharge(value);

            CheckAllowedToDischarge(value);
        }

        private void CheckAllowedToDischarge(bool value)
        {
            if (_mono.GetMode() == PowerChargerMode.Auto)
            {
                _performDischargeBuffer = true;
                return;
            }

            _allowedToDisCharge = !value;
        }
        
        public byte[] Save(ProtobufSerializer serializer)
        {
            return null; //_powerCharger.Save(serializer);
        }

#if SUBNAUTICA_STABLE
        public void Load(ProtobufSerializer serializer, byte[] savedDataData)
        {
            _powerCharger.Load(serializer, savedDataData);
        }
#else
        public void Load(ProtobufSerializer serializer, byte[] savedDataData)
        {
            StartCoroutine(_powerCharger.Load(serializer, savedDataData));
        }
#endif

        public bool HasPowercells()
        {
            return _powerCharger.HasPowerCells();
        }

        public bool GetIsReleasingPower()
        {
            return _allowedToDisCharge;
        }

        public void LoadFromSave()=> _powerCharger.LoadFromSave();
    }
}
