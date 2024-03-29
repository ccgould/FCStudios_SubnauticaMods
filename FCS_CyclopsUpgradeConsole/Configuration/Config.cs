﻿using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace CyclopsUpgradeConsole.Configuration
{
    [Menu("FCS Cyclops Upgrade Console Menu")]
    internal class Config : ConfigFile
    {
        public Config() : base("cyclopsupgradeconsole-config", "Configurations")
        {
        }

        [Toggle("Enable Debugs", Order = 0), OnChange(nameof(EnableDebugsToggleEvent))]
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
            //QPatch.Configuration.Save();
        }
    }
}
