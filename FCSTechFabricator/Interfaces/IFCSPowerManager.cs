using FCSTechFabricator.Enums;

namespace FCSTechFabricator.Interfaces
{
    public interface IFCSPowerManager
    {
        float GetPowerUsagePerSecond();
        float GetDevicePowerCharge();
        float GetDevicePowerCapacity();
        FCSPowerStates GetPowerState();
        void TogglePowerState();
        void SetPowerState(FCSPowerStates state);
        bool IsDevicePowerFull();
        bool ModifyPower(float amount, out float consumed);
    }
}
