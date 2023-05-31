using System;
using System.Collections.Generic;
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


    internal void Initialize(string key, List<PowerSource> value)
    {
        units.text = value.Count.ToString();
        powerSources.text = key;
        _powerSource = value;
        InvokeRepeating(nameof(Refresh), 1, 1);
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
