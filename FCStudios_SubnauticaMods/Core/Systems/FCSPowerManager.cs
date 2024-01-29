using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models.Enumerators;
using UnityEngine;

namespace FCS_AlterraHub.Core.Systems;
public abstract class FCSPowerManager : MonoBehaviour
{
    public virtual float GetPowerUsagePerSecond()
    {
        return 0f;
    }

    public virtual float GetDevicePowerCharge()
    {
        return 0f;
    }

    public virtual float GetDevicePowerCapacity()
    {
        return 0f;
    }

    public virtual FCSPowerStates GetPowerState()
    {
        return FCSPowerStates.None;
    }

    public virtual void TogglePowerState()
    {

    }

    public virtual void SetPowerState(FCSPowerStates state)
    {

    }

    public virtual bool IsDevicePowerFull()
    {
        return true;
    }

    public virtual bool ModifyPower(float amount, out float consumed)
    {
        consumed = 0f;
        return false;
    }

    public virtual bool HasEnoughPowerToOperate()
    {
        return false;
    }

    public virtual void SetPowerRelay(PowerRelay powerRelay)
    {

    }

    public virtual bool IsPowerAvailable()
    {
        return false;
    }

    public virtual PowercellData GetBatteryPowerData()
    {
        return null;
    }

    public virtual float GetPowerUsage()
    {
        return 0f;
    }
}
