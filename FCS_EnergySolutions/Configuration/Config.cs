using System.Collections.Generic;
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

        [Toggle("[Telepower Pylon] Is Mod Enabled", Tooltip = "Enables/Disables Telepower Pylon from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTelepowerPylonEnabled = true;


        [Toggle("[PowerStorage] Is Mod Enabled", Tooltip = "Enables/Disables PowerStorage from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsPowerStorageEnabled = true;

        [Toggle("[AlterraGen] Is Mod Enabled", Tooltip = "Enables/Disables AlterraGen from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsAlterraGenEnabled = true;

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
    }
}
