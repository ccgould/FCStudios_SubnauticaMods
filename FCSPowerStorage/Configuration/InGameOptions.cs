﻿using SMLHelper.V2.Options;
using UnityEngine;

namespace FCSPowerStorage.Configuration
{
    internal class InGameOptions : ModOptions
    {
        private const string CapacityID = "Capacity";
        private const string MinEnergyID = "MinEnergy";
        private const string AutoActivateMulID = "AutoActivateMul";
        private const string BaseDrainProtectionMulID = "BaseDrainProtectionMul";
        private const string DisableOnMinEnergyRequiredID = "DisableOnMinEnergyRequired";
        private const string AutoActivateAtID = "AutoActivateAt";


        public InGameOptions() : base("Power Storage (Numbers are multiplied by the multipliers)")
        {
            ToggleChanged += OnToggleChanged;
            SliderChanged += OnSliderChanged;
        }

        private void OnSliderChanged(object sender, SliderChangedEventArgs e)
        {
            switch (e.Id)
            {
                case CapacityID:
                    LoadData.BatteryConfiguration.Capacity = e.IntegerValue * 10;
                    break;

                case MinEnergyID:
                    LoadData.BatteryConfiguration.BaseDrainProtectionSlider = e.IntegerValue;
                    UpdateGoals(MinEnergyID, e.IntegerValue);
                    break;

                case AutoActivateAtID:
                    LoadData.BatteryConfiguration.AutoActivateSlider = e.IntegerValue;
                    UpdateGoals(AutoActivateAtID, e.IntegerValue);
                    break;

                case BaseDrainProtectionMulID:
                    LoadData.BatteryConfiguration.BaseDrainProtectionMultiplier = e.IntegerValue;
                    UpdateGoals(BaseDrainProtectionMulID, e.IntegerValue);
                    break;

                case AutoActivateMulID:
                    LoadData.BatteryConfiguration.AutoActivateMultiplier = e.IntegerValue;
                    UpdateGoals(AutoActivateMulID, e.IntegerValue);
                    break;
            }

            LoadData.BatteryConfiguration.SaveConfiguration();
        }

        private void UpdateGoals(string id, int value)
        {
            switch (id)
            {
                case MinEnergyID:
                    LoadData.BatteryConfiguration.BaseDrainProtectionGoal = value * LoadData.BatteryConfiguration.BaseDrainProtectionMultiplier;
                    break;

                case AutoActivateAtID:
                    LoadData.BatteryConfiguration.AutoActivateAt = value * LoadData.BatteryConfiguration.AutoActivateMultiplier;
                    break;

                case BaseDrainProtectionMulID:
                    LoadData.BatteryConfiguration.BaseDrainProtectionGoal = LoadData.BatteryConfiguration.BaseDrainProtectionSlider * value;
                    break;

                case AutoActivateMulID:
                    LoadData.BatteryConfiguration.AutoActivateAt = LoadData.BatteryConfiguration.AutoActivateSlider * value;
                    break;
            }
        }

        private void OnToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (e.Id == DisableOnMinEnergyRequiredID)
                LoadData.BatteryConfiguration.BaseDrainProtection = e.Value;

            LoadData.BatteryConfiguration.SaveConfiguration();
        }

        public override void BuildModOptions()
        {
            //AddSliderOption(CapacityID, "Power Storage Capacity", 1f, 1000f, LoadData.BatteryConfiguration.Capacity / 10); //Disabled because of slider limitations
            AddToggleOption(DisableOnMinEnergyRequiredID, "Enable Base Drain Protection.", LoadData.BatteryConfiguration.BaseDrainProtection);

            AddSliderOption(BaseDrainProtectionMulID, "Base Drain Protection Goal Multiplier", 1f, 100f, LoadData.BatteryConfiguration.BaseDrainProtectionMultiplier);
            AddSliderOption(MinEnergyID, "Base Drain Protection Goal.", 1f, 100f, Mathf.Round(Mathf.Round(LoadData.BatteryConfiguration.BaseDrainProtectionSlider) / Mathf.Round(LoadData.BatteryConfiguration.BaseDrainProtectionMultiplier)));

            AddSliderOption(AutoActivateMulID, "Auto Activate Multiplier", 1f, 100f, LoadData.BatteryConfiguration.AutoActivateMultiplier);
            AddSliderOption(AutoActivateAtID, "Auto Activate Discharge At..", 1f, 100f, Mathf.Round(Mathf.Round(LoadData.BatteryConfiguration.AutoActivateSlider) / Mathf.Round(LoadData.BatteryConfiguration.AutoActivateMultiplier)));

        }
    }
}
