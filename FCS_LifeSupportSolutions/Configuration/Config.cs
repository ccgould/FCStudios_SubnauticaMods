﻿using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace FCS_LifeSupportSolutions.Configuration
{
    [Menu("FCS Life Support Solutions Menu")]
    internal class Config : ConfigFile
    {
        public Config() : base("lifeSupportSolutions-config", "Configurations")
        {
        }

        [Toggle("Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

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
    
    [Menu("Base Utility Unit Menu")]
    internal class BaseUtilityUnitConfig : ConfigFile
    {
        public BaseUtilityUnitConfig() : base("baseUtilityUnit-config", "Configurations")
        {
        }
        [Toggle("Is Mod Enabled", Tooltip = "Set to true to enable the mod. (Requires Game Restart.)"), OnChange(nameof(IsModeEnabledToggleEvent))]
        public bool IsModEnabled = true;

        [Toggle("Require Utility Unit for Oxygen")]
        public bool AffectPlayerOxygen = false;

        [Toggle("Basic Oxygen for Small Habitats", Tooltip = "When Affect Player Oxygen is enabled and this option is enabled, you can have a small base about the size of a MoonPool without needing a life support system  (4x4 area, single-floor)")]
        public bool SmallBaseOxygen = false;

        [Toggle("PlaySFX")]
        public bool PlaySFX = true;

        private void IsModeEnabledToggleEvent(ToggleChangedEventArgs e)
        {
            if (e.Value)
            {
                QuickLogger.ModMessage("Base Utility Enabled");
            }
            else
            {
                QuickLogger.ModMessage("Base Utility Disabled");
            }
        }
    }
}
