using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Configuration
{
    [Menu("FCS Life Support Solutions Menu")]
    internal class Config : ConfigFile
    {
        public Config() : base("lifeSupportSolutions-config", "Configurations")
        {
        }

        [Toggle("[Life Support Solutions] Enable Debugs", Order = 0), OnChange(nameof(EnableDebugsToggleEvent)), Tooltip("Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)")]
        public bool EnableDebugLogs = false;


        #region Base Utility Unit

        [Toggle("[Base Utility Unit] Is Mod Enabled"),
         Tooltip("Enables/Disables Base Utility Unit from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool BaseUtilityUnitIsModEnabled = true;

        [Toggle("Require Utility Unit for Oxygen")]
        public bool BaseUtilityUnitAffectPlayerOxygen = false;

        [Toggle("PlaySFX")]
        public bool BaseUtilityUnitPlaySFX = true;

        #endregion
        
        #region Small Base Oxygen
        [Toggle("Small Base Oxygen Tank Hardcore", Tooltip = "When Affect Player Oxygen is enabled this effects how many base pieces are allowed per Tank")]
        public bool SmallBaseOxygenHardcore = false;
        #endregion

        #region Energy Pill Vending Machine
        [Toggle("[Energy Pill Vending Machine] Is Mod Enabled"),
         Tooltip("Enables/Disables Energy Pill Vending Machine from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsEnergyPillVendingMachineEnabled = true;
        #endregion

        #region Mini MedBay
        [Toggle("[Mini MedBay] Is Mod Enabled"),
         Tooltip("Enables/Disables Mini MedBay from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsMiniMedBayEnabled = true;
        #endregion

        private void EnableDebugsToggleEvent(ToggleChangedEventArgs e)
        {
            if (e.Value)
            {
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
            }
            else
            {
                QuickLogger.DebugLogsEnabled = false;
                QuickLogger.Info("Debug logs disabled");
            }
        }
    }    
    
}
