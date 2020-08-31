using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStoragePowerManager : IFCSPowerManager
    {
        public float GetPowerUsagePerSecond()
        {
            return 0f;
        }

        public float GetDevicePowerCharge()
        {
            return 0f;
        }

        public float GetDevicePowerCapacity()
        {
            return 0f;
        }

        public FCSPowerStates GetPowerState()
        {
            return FCSPowerStates.Powered;
        }

        public void TogglePowerState()
        {
            
        }

        public void SetPowerState(FCSPowerStates state)
        {
            
        }

        public bool IsDevicePowerFull()
        {
            return true;
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return true;
        }
    }
}
