using FCSCommon.Utilities;
using SMLHelper.Json;
using SMLHelper.Options;
using SMLHelper.Options.Attributes;

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
            //Main.Configuration.Save();
        }
    }
}
