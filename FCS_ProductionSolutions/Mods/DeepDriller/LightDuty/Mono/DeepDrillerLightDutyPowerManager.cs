using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.Interfaces;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;

namespace FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Mono
{
    internal class DeepDrillerLightDutyPowerManager : FCSPowerManager, IPowerManager
    {
        private float _timePassed;
        private DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();
        private DrillSystem _mono;
        private PowerRelay _powerRelay;
        private bool _isInitialized;
        private FCSPowerStates _prevPowerState;

        public FCSPowerStates PowerState { get; set; }

        internal bool IsFull { get; set; }


        private void Update()
        {
            if (_mono == null || !_mono.IsConstructed ||
                !_isInitialized || _powerBank?.Battery == null) return;

            UpdatePowerState();

            _timePassed += DayNightCycle.main.deltaTime;

            if (_timePassed >= 1)
            {
                AttemptToChargeBattery();
                ConsumePower(GetPowerUsage());
                _timePassed = 0.0f;
            }
        }

        private void ConsumePower(float amount)
        {
            if (!_mono.IsOperational || !_mono.OreGenerator.GetIsDrilling()) return;

            _powerBank.Battery.RemoveCharge(amount);
            QuickLogger.Debug($"Remove {amount} from battery", true);
            _mono.OnBatteryLevelChange?.Invoke(_powerBank.Battery);
        }

        private void AttemptToChargeBattery()
        {
            if (/*GetTotalCharge() <= 0 ||*/ _powerBank.Battery.IsFull()) return;

            var amount = _powerBank.Battery.GetCapacity() - _powerBank.Battery.GetCharge();

            if (amount <= 0)
            {
                _mono.OnBatteryLevelChange?.Invoke(_powerBank.Battery);
                return;
            }


            if (_powerBank.Battery.IsFull() || amount <= 0)
            {
                _mono.OnBatteryLevelChange?.Invoke(_powerBank.Battery);
                return;
            }

            if (_powerRelay != null && _powerRelay.GetPower() > 0)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
                _powerBank.Battery.AddCharge(amountConsumed);
                amount -= amountConsumed;
            }

            if (amount <= 0)
            {
                _mono.OnBatteryLevelChange?.Invoke(_powerBank.Battery);
                return;
            }

            
            _mono.OnBatteryLevelChange?.Invoke(_powerBank.Battery);
        }
        
        private void UpdatePowerState()
        {
            if (PowerState == FCSPowerStates.Tripped) return;

            if (IsPowerAvailable() && _prevPowerState != FCSPowerStates.Powered)
            {
                PowerState = FCSPowerStates.Powered;
                _prevPowerState = FCSPowerStates.Powered;
            }
            else if (!IsPowerAvailable() && _prevPowerState == FCSPowerStates.Powered)
            {
                PowerState = FCSPowerStates.UnPowered;
                _prevPowerState = FCSPowerStates.UnPowered;
            }
        }

        internal void Initialize(DrillSystem mono)
        {
            if (_isInitialized) return;
            BasePowerDraw = 0.025f; // since the drill is smaller I have set it to the lowest the solar can generate
            _mono = mono;
            _isInitialized = true;
        }
        
        public override bool IsPowerAvailable()
        {
            return !(GetTotalCharge() <= 0);
        }

        public override FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        public DeepDrillerPowerData SaveData()
        {
            return null;
        }

        public void LoadData(DeepDrillerLightDutySaveDataEntry data)
        {
            if (data?.PowerData?.Battery == null) return;

            QuickLogger.Message($"Solar Panel: {data.PowerData.SolarPanel} || Battery: {data.PowerData.Battery.GetCharge()}", true);


            if (_powerBank?.Battery == null)
            {
                _powerBank = new DeepDrillerPowerData();
            }

            _powerBank.Thermal = data.PowerData.Thermal;
            _powerBank.SolarPanel = data.PowerData.SolarPanel;
            _powerBank.Battery.AddCharge(data.PowerData.Battery.GetCharge());
            //PullPowerFromRelay = data.PullFromRelay; Disabled to until I decide to make it a toggle option
            PowerState = data.PowerState;
        }

        public override bool HasEnoughPowerToOperate()
        {
            if (!IsPowerAvailable()) return false;
            return _powerRelay?.GetPower() + _powerBank.SolarPanel.GetCharge() + _powerBank.Thermal.GetCharge() + _powerBank.Battery.GetCharge() >= GetPowerUsage();
        }

        public override bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return false;
        }

        internal float GetTotalCharge()
        {
            if (_powerBank?.Battery == null) return 0f;
            return (_powerRelay?.GetPower() ?? 0) + _powerBank.Battery.GetCharge();
        }

        public override float GetPowerUsagePerSecond()
        {
            return GetPowerUsage();
        }

        public override float GetPowerUsage()
        {
            return BasePowerDraw;
        }

        private float BasePowerDraw = Main.Configuration.DDDefaultOperationalPowerUsage + Main.Configuration.DDOrePowerUsage;


        public override float GetDevicePowerCharge()
        {
            return GetTotalCharge();
        }

        public override float GetDevicePowerCapacity()
        {
            return Main.Configuration.DDInternalBatteryCapacity;
        }

        public override void TogglePowerState()
        {
            if (GetPowerState() == FCSPowerStates.Tripped)
            {
               SetPowerState(FCSPowerStates.Powered);
            }
            else
            {
                SetPowerState(FCSPowerStates.Tripped);
            }
        }

        public override bool IsDevicePowerFull()
        {
            return IsFull;
        }

        public override void SetPowerRelay(PowerRelay powerRelay)
        {
            _powerRelay = powerRelay;
        }

        public override PowercellData GetBatteryPowerData()
        {
            return _powerBank.Battery;
        }
    }
}