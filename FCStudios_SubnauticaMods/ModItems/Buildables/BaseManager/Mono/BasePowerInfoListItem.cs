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

    private void RefreshPowerInterfaces()
    {
        QuickLogger.Debug("RefreshPowerInterfaces");
        float chargeAmount = 0f;


        foreach (var powerInterface in _powerInterface)
        {
            if(powerInterface.IsRunning())
            {
                chargeAmount += powerInterface.GetPowerUsage();
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
