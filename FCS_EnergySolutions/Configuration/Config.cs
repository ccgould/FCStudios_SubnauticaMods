using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;


namespace FCS_EnergySolutions.Configuration
{
    [Menu("FCS Energy Solutions Menu")]
    internal class Config : ConfigFile
    {

        public Config() : base("energySolutions-config", "Configurations")
        {
        }

        [Toggle("[Energy Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        [Toggle("[Energy Solutions] Lights Enables", Tooltip = "Enables lights on objects that have lights (May improve performance)"), OnChange(nameof(EnableDisableLightToggleEvent))]
        public bool EnableDisableLights = true;

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

        [Toggle("[JetStreamT242] Is Mod Enabled", Tooltip = "Enables/Disables JetStreamT242 from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsJetStreamT242Enabled = true;
        
        [Toggle("[Solar Panel Cluster] Is Mod Enabled", Tooltip = "Enables/Disables Solar Panel Cluster from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsAlterraSolarPanelClusterEnabled = true;

        [Toggle("[Telepower Pylon] Is Mod Enabled", Order = 1,Tooltip = "Enables/Disables Telepower Pylon from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTelepowerPylonEnabled = true;

        [Slider("[Telepower Pylon] Pylon Effects Brightness", 0, 1, Step = 0.1f, Format = "{0:F2}", DefaultValue = 1,
            Order = 1,Tooltip = "Allows you to adjust the brightness of the trail effect in the telepower Pylon effects."),
         OnChange(nameof(TelepowerPylonBrightnessChangeEvent))]
        public float TelepowerPylonTrailBrightness { get; set; } = 1;


        [Slider("[Telepower Pylon] Pylon Power Usage Per Meter", 0, 0.00085f, Step = 0.00001f, Format = "{0:F6}", DefaultValue = 0.00085f,
            Order = 1,Tooltip = "Allows you to adjust the power usage of the Telepower Pylon.")]
        public float TelepowerPylonPowerUsagePerMeter { get; set; } = 0.00085f;

        [Toggle("[PowerStorage] Is Mod Enabled", Tooltip = "Enables/Disables PowerStorage from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsPowerStorageEnabled = true;

        [Toggle("[AlterraGen] Is Mod Enabled", Tooltip = "Enables/Disables AlterraGen from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsAlterraGenEnabled = true;

        [Toggle("[WindSurfer] Is Mod Enabled", Tooltip = "Enables/Disables WindSurfer from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsWindSurferEnabled = true;

        public Dictionary<string, float> JetStreamT242BiomeSpeeds { get; set; } = new Dictionary<string, float>
        {
            {"kelpforest",283f },
            {"mushroomforest",200f },
            {"koosh",220f },
            {"jellyshroom",166f },
            {"sparsereef",250f },
            {"grandreef",300f },
            {"deepgrandreef",295f },
            {"dunes",295f },
            {"mountains",275f },
            {"bloodkelp",255f },
            {"underwaterislands",282f },
            {"inactivelavazone",295f },
            {"floaterislands",298f },
            {"lostriver",267f },
            {"ghosttree",267f },
            {"skeletoncave",267f },
            {"activelavazone",300f },
            {"crashzone",300f },
            {"seatreaderpath",300f },
            {"safe",300f },
            {"grassy",296f },
            {"crag",275f },
            {"ilz",295f },
            {"alz",300f },
            {"lava",300f },
            {"void",300f },
            {"floating",300f },
            {"None",0f }
        };


        private static void TelepowerPylonBrightnessChangeEvent(SliderChangedEventArgs e)
        {
            BaseManager.GlobalNotifyByID(Mod.TelepowerPylonTabID, "UpdateEffects");
        }

        private static void EnableDisableLightToggleEvent(ToggleChangedEventArgs e)
        {
            Mod.OnLightsEnabledToggle?.Invoke(e.Value);
        }


    }
}
