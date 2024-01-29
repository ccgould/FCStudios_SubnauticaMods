using FCS_AlterraHub.Core.Extensions;
using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FCS_ProductionSolutions.Configuration;

[Menu("FCS Production Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("productionSolutions-config", "Configurations") { }

    [Toggle("[Production Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
    public bool EnableDebugLogs = false;

    #region Matter Analyzer

    [Toggle("[Matter Analyzer] Is Mod Enabled", Tooltip = "Enables/Disables Matter Analyzer from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsMatterAnalyzerEnabled = true;

    [Toggle("[Matter Analyzer] Toggle Fx")]
    public bool MatterAnalyzerPlaySFX = true;

    #endregion

    #region Replicator

    [Toggle("[Replicator] Is Mod Enabled", Tooltip = "Enables/Disables Replicator from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsReplicatorEnabled = true;

    #endregion

    #region Harvester

    [Toggle("[Hydroponic Harvester]] Is Mod Enabled", Tooltip = "Enables/Disables Hydroponic Harvester from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsHydroponicHarvesterEnabled = true;

    [Toggle("[Hydroponic Harvester] Enable/Disable Light Trigger")]
    public bool HHIsLightTriggerEnabled = true;

    [Slider("[Hydroponic Harvester] Light Trigger Range", 0, 20)]
    public int HHLightTriggerRange = 4;

    #endregion

    #region Deep Driller

    [Toggle("[Deep Driller] Is Mod Enabled", Order = 1, Tooltip = "Enables/Disables Deep Driller MK3 from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsDeepDrillerEnabled = true;
    //[Toggle("[Deep Driller] Is HardCore Mode", Order = 1)]
    //public bool DDHardCoreMode  = true;
    [Toggle("[Deep Driller Play Fx", Order = 1, Tooltip = "Enables/Disables Deep Driller MK3 sound effects")]
    public bool DDMK3FxAllowed = true;
    [Slider("[Deep Driller] Master Volume", 0.0f, 1.0f, DefaultValue = 0.1f, Step = 0.01f, Format = "{0:P0}", Order = 1, Tooltip = "This control affects all drills.")]
    public float MasterDeepDrillerVolume { get; set; } = 0.1f;

    internal float DDEnergyPerOre = 0.1064f;


    internal float DDDefaultOrePerDay = 25.0f;
    internal float DDDefaultOperationalPowerUsage = 2.00f;
    public float DDOrePowerUsageBelowDefault = 0.0264f;
    internal float DDOrePowerUsage = 0.66f;
    public float DDOilTimePeriodInDays = 30.0f;
    public float DDOilRestoresInDays = 5.0f;
    public Dictionary<string, List<string>> DDAdditionalBiomeOres = new Dictionary<string, List<string>>();
    public float DDMaxOreCountUpgradePowerUsage = 0.2f;
    public float DDInternalBatteryCapacity = 1000f;
    public float DDDrillAlterraStorageRange = 30f;
    public float DDSolarCapacity = 125;

    [Toggle("[Auto Crafter]] Is Mod Enabled", Tooltip = "Enables/Disables Auto Crafter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
    public bool IsAutocrafterEnabled = true;

    [JsonIgnore] internal Dictionary<string, List<TechType>> DDBiomeOresTechType = new Dictionary<string, List<TechType>>();



    internal void Convert()
    {
        try
        {
            foreach (KeyValuePair<string, List<string>> biomeOre in DDAdditionalBiomeOres)
            {
                var types = new List<TechType>();

                foreach (string sTechType in biomeOre.Value)
                {
                    types.Add(sTechType.ToTechType());
                }
                QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                if (!DDBiomeOresTechType.ContainsKey(biomeOre.Key))
                {
                    DDBiomeOresTechType.Add(biomeOre.Key, types);
                }
                else
                {
                    DDBiomeOresTechType[biomeOre.Key] = DDBiomeOresTechType[biomeOre.Key].Union(types).ToList();
                }

            }
        }
        catch (Exception e)
        {
            QuickLogger.Error($"Error: {e.Message} || Stack Trace: {e.StackTrace}");
        }
    }

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
