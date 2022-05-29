using System;
using FCS_LifeSupportSolutions.Spawnables;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono
{
    internal class PlayerAdrenaline : MonoBehaviour
    {
        public float Percentage { get; set; }
        public bool HasAdrenaline => Percentage > 0f;
        public float AdrenalineLevel { get; set; }
        const float MaxAdrenaline = 120f;
        public Action OnAdrenalineAdded { get; set; }

        public void Update()
        {
            AdrenalineLevel = Mathf.Clamp(AdrenalineLevel - DayNightCycle.main.deltaTime, 0, MaxAdrenaline);
            Percentage = AdrenalineLevel / MaxAdrenaline;
        }
        
        public void AddAdrenaline(PillComponent pill)
        {
            AdrenalineLevel = Mathf.Clamp(pill.Amount + AdrenalineLevel, 0, MaxAdrenaline);
            OnAdrenalineAdded?.Invoke();
        }
    }
}