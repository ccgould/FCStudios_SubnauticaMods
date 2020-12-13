using System;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
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

        public FCSGameMode ModMode { get; set; }

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

    [Menu("Mini Fountain Filter Menu")]
    public class MiniFountainConfig : ConfigFile
    {
        public MiniFountainConfig() : base("minifountainfilterconfig", "Configurations") { }

        [Toggle("Enable/Disable SoundFX")]
        public bool PlaySFX { get; set; } = true;
        public float TankCapacity { get; set; } = 100f;
        public float EnergyPerSec { get; set; } = 0.425f;
        public int StorageWidth { get; set; } = 3;
        public int StorageHeight { get; set; } = 2;
        public float WaterPerSecond { get; set; } = 1f;
        public bool AutoGenerateMode { get; set; } = false;
        public TechType BottleTechType { get; set; } = TechType.FilteredWater;
    }

    [Menu("SeaBreeze Menu")]
    public class SeaBreezeConfig : ConfigFile
    {
        public SeaBreezeConfig() : base("seabreeze-config", "Configurations") { }

        public int StorageLimit { get; set; } = 100;
        public float PowerUsage { get; set; } = 0.5066666666666667f;
        public bool UseBasePower { get; set; } = true;
        [Choice("Mode Game Mode"), OnChange(nameof(ChangeGameModeEvent))]
        public FCSGameMode ModMode { get; set; }

        private void ChangeGameModeEvent(ChoiceChangedEventArgs e)
        {
            OnGameModeChanged?.Invoke(e.Index);
        }

        public Action<int> OnGameModeChanged { get; set; }
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

    [Menu("Paint Tool Menu")]
    public class PaintToolConfig : ConfigFile
    {
        public PaintToolConfig() : base("paintTool-config", "Configurations") { }

        [Keybind("Select Color Forward")]
        public KeyCode SelectColorForwardKeyCode = KeyCode.RightArrow;

        [Keybind("Select Color Back")]
        public KeyCode SelectColorBackKeyCode = KeyCode.LeftArrow;
    }

    [Menu("Quantum Teleporter Menu")]
    public class QuantumTeleporterConfig : ConfigFile
    {
        public QuantumTeleporterConfig() : base("quantumTeleporter-config", "Configurations") { }

        [JsonProperty] internal float GlobalTeleportPowerUsage { get; set; } = 1500f;
        [JsonProperty] internal float InternalTeleportPowerUsage { get; set; } = 150f;

    }
}
