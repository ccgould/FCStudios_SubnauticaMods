using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;

namespace FCS_HomeSolutions.Configuration
{
    
    [Menu("FCS Home Solutions Menu")]
    public class Config : ConfigFile
    {
        public Config() : base("homeSolutions-config","Configurations") { }

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

    [Menu("Hover Lift Pad Menu")]
    public class HoverLiftPadConfig : ConfigFile
    {
        public HoverLiftPadConfig() : base("hoverliftPad-config", "Configurations") { }
        
        [Keybind("Lift Pad Up Button")]
        public KeyCode LiftPadUpKeyCode = KeyCode.None;

        [Keybind("Lift Pad Down Button")]
        public KeyCode LiftPadDownKeyCode = KeyCode.None;
    }
}
