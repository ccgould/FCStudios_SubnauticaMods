using FCS_AlterraHub.Core.Components;
using FCSCommon.Utilities;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class BasePowerInfoListItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text units;
    [SerializeField]
    private TMP_Text powerSources;
    [SerializeField]
    private TMP_Text epm;
    [SerializeField]
    private TMP_Text charge;
    private List<PowerSource> _powerSource;
    private List<FCSPowerInterface> _powerInterface;
    private List<Charger> _charger;

    internal void Initialize(string key, List<PowerSource> value)
    {
        units.text = value.Count.ToString();
        powerSources.text = key;
        _powerSource = value;
        InvokeRepeating(nameof(Refresh), 1, 1);
    }

    internal void Initialize(string deviceName, List<FCSPowerInterface> powerInterfaces)
    {
        units.text = powerInterfaces.Count.ToString();
        epm.text = powerInterfaces.FirstOrDefault()?.PowerPerSec.ToString() ?? "0";
        powerSources.text = deviceName;
        _powerInterface = powerInterfaces;
        InvokeRepeating(nameof(RefreshPowerInterfaces), 1, 1);
    }

    internal void Initialize(string deviceName, List<Charger> charger)
    {
        units.text = charger.Count.ToString();
        epm.text = charger.FirstOrDefault()?.chargeSpeed.ToString() ?? "0";
        powerSources.text = deviceName;
        _charger = charger;
        InvokeRepeating(nameof(RefreshChargers), 1, 1);
    }

    private void RefreshPowerInterfaces()
    {
        float chargeAmount = 0f;


        foreach (var powerInterface in _powerInterface)
        {
            if(powerInterface.IsRunning())
            {
                chargeAmount += powerInterface.GetPowerUsage();
            }
        }
        charge.text = chargeAmount.ToString("F2");
    }

    private void RefreshChargers()
    {
        QuickLogger.Debug("Refresh Chargers");
        float chargeAmount = 0f;

        float num = 0;
        foreach (var charger in _charger)
        {
            foreach (KeyValuePair<string, IBattery> keyValuePair in charger.batteries)
            {
                IBattery value = keyValuePair.Value;
                if (value != null)
                {
                    float charge = value.charge;
                    float capacity = value.capacity;
                    if (charge < capacity)
                    {
                        num++;
                        float num3 = DayNightCycle.main.deltaTime * charger.chargeSpeed * capacity;
                        if (charge + num3 > capacity)
                        {
                            num3 = capacity - charge;
                        }
                        chargeAmount += num3;
                    }
                }
            }
        }

        QuickLogger.Debug("End Of RefreshPowerInterfaces");
        charge.text = chargeAmount.ToString("F2");
    }

    private void Refresh()
    {
        float chargeAmount = 0f;
        float maxAmount = 0f;

        foreach (var powerSource in _powerSource)
        {
            chargeAmount += powerSource.power;
            maxAmount += powerSource.maxPower;
        }

        charge.text = $"{Mathf.RoundToInt(chargeAmount)}/{Mathf.RoundToInt(maxAmount)}";
    }
}
