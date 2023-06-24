using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;


namespace FCS_StorageSolutions.Configuration;
[Menu("FCS Storage Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("storageSolutions-config", "Configurations") { }


    [Toggle("[Storage Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
    public bool EnableDebugLogs = false;


    #region Remote Storage

    [Toggle("[Remote Storage] Is Mod Enabled", Tooltip = "Enables/Disables Remote Storage from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsRemoteStorageEnabled = true;

    #endregion

    #region Data Storage Solutions

    [Toggle("[Data Storage Solutions] Is Mod Enabled", Tooltip = "Enables/Disables Data Storage Solutions from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsDataStorageSolutionsEnabled = true;

    #endregion


    [Toggle("[Data Storage Solutions] Show Custom Server Tool Tip", Tooltip = "Allows more information when hovering over a server in your inventory")]
    public bool ShowServerCustomToolTip = true;


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
