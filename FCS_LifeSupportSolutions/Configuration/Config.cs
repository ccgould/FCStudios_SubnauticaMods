using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models;
using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Newtonsoft.Json;


namespace FCS_LifeSupportSolutions.Configuration;
[Menu("FCS Life Support Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("lifeSupportSolutions-config", "Configurations") { }

    [Toggle("[Life Support Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
    public bool EnableDebugLogs = false;

    #region Base Utility Unit

    [Toggle("[Base Utility Unit] Is Mod Enabled", Tooltip = "Enables/Disables Base Utility Unit from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
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
