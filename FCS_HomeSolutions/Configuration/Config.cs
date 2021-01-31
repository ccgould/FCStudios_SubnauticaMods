using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
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

        [Toggle("Alterra Chief Mod Enabled")] 
        public bool IsAlienChiefEnabled { get; set; } = true;

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
    
    [Menu("Paint Tool Menu")]
    public class PaintToolConfig : ConfigFile
    {
        public PaintToolConfig() : base("paintTool-config", "Configurations") { }

        [Keybind("Select Color Forward")]
        public KeyCode SelectColorForwardKeyCode = KeyCode.RightArrow;

        [Keybind("Select Color Back")]
        public KeyCode SelectColorBackKeyCode = KeyCode.LeftArrow;

        public List<AdditionalColor> AdditionalPaintColors { get; set; } = new List<AdditionalColor>
        {
            new AdditionalColor
            {
                Color = new Vec4(178f,34f,34f,255),
                ColorName = "FireBrick"
            }
        };
    }



    [Menu("Quantum Teleporter Menu")]
    public class QuantumTeleporterConfig : ConfigFile
    {
        public QuantumTeleporterConfig() : base("quantumTeleporter-config", "Configurations") { }

        [JsonProperty] internal float GlobalTeleportPowerUsage { get; set; } = 1500f;
        [JsonProperty] internal float InternalTeleportPowerUsage { get; set; } = 150f;

    }

    [Menu("Television Menu")]
    public class TelevisionConfig : ConfigFile
    {
        public TelevisionConfig() : base("television-config", "Configurations") { }

        [Keybind("Volume Up")]
        public KeyCode VolumeUp = KeyCode.UpArrow;

        [Keybind("Volume Down")]
        public KeyCode VolumeDown = KeyCode.DownArrow;

        [Keybind("Channel Up")]
        public KeyCode ChannelUp = KeyCode.RightArrow;

        [Keybind("Channel Down")]
        public KeyCode ChannelDown = KeyCode.LeftArrow;

        [Keybind("Turn On/Off Tv")]
        public KeyCode ToggleTv = KeyCode.F;

    }
}
