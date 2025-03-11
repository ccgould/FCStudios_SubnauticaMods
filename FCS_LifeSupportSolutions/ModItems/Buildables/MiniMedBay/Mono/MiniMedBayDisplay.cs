using UnityEngine;
using UnityEngine.UI;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayDisplay : MonoBehaviour
{
    [SerializeField] private Text _dispenserCounter;
    [SerializeField] private Text _healMeter;



    public void UpdatePlayerHealthPercent(int amount)
    {
        _healMeter.text = $"{amount}%";
    }

    public void UpdateDispenserCount(int medKits)
    {
       _dispenserCounter.text = medKits.ToString();
    }
}
