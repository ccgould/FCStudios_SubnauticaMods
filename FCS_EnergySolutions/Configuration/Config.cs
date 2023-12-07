using FCS_AlterraHub.Core.Extensions;
using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FCS_EnergySolutions.Configuration;

[Menu("FCS Energy Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("energySolutions-config", "Configurations") { }

    [Toggle("[Energy Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
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
