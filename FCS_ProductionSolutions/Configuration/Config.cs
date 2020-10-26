using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using FCSCommon.Utilities;
using SMLHelper.V2.Options;

namespace FCS_ProductionSolutions.Configuration
{

    [Menu("FCS Production Solutions Menu")]
    public class Config : ConfigFile
    {
        public Config() : base("productionSolutions-config","Configurations") { }

        [Toggle("Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        public float EnergyConsumpion { get; set; } = 15000f;

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

    [Menu("Hydroponic Harvester Menu")]
    public class HarvesterConfig : ConfigFile
    {
        public HarvesterConfig() : base("harvester-config", "Configurations") { }

        [Toggle("Enable/Disable Light Trigger")]
        public bool IsLightTriggerEnabled = true;

        [Slider("Light Trigger Range",0,20)]
        public int LightTriggerRange = 4;
    }
}
