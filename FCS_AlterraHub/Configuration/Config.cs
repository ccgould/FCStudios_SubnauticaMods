using System;
using System.Collections.Generic;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Structs;
using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;
using UnityEngine.Events;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Configuration
{
    [Menu("FCS AlterraHub Menu")]
    public class Config : ConfigFile
    {
        public Config() : base("alterrahub-config", "Configurations")
        {
        }

        [Toggle("Enable Debugs", Order = 0), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        [JsonIgnore]
        public UnityAction<int> onGameModeChanged;

        [Keybind("Open/Close FCS PDA"), OnChange(nameof(PDAKeyCodeChangedEvent))]
        public KeyCode FCSPDAKeyCode { get; set; } = KeyCode.F2;

        [Keybind("FCS PDA Information Button"), OnChange(nameof(PDAKeyCodeChangedEvent))]
        public KeyCode PDAInfoKeyCode { get; set; } = KeyCode.I;

        public List<FCSStoreEntry> AdditionalStoreItems = new();

        [Slider("Ore Payout Difficulty", 0.1f, 1.9f, DefaultValue = 1, Step = 0.1f, Format = "{0}", Tooltip = "The higher the value the more payout per ore.")]
        public float OrePayoutMultiplier { get; set; } = 1f;

        [Toggle("Play Sound Effects"), OnChange(nameof(PlaySoundToggleEvent))]
        public bool PlaySFX = true;

        private void PlaySoundToggleEvent(ToggleChangedEventArgs e)
        {
            OnPlaySoundToggleEvent?.Invoke(e.Value);
        }

        [JsonIgnore]
        internal Action<bool> OnPlaySoundToggleEvent { get; set; }

        [Toggle("[Alterra Transport Drone] Enable Drone Audio", Order = 1, Tooltip = "Enables/Disables the sound effects on the drone.")]
        public bool AlterraTransportDroneFxAllowed { get; set; } = true;

        //[Slider("[Alterra Transport Drone] Master Volume", 0.0f, 1.0f, DefaultValue = 1.0f, Step = 0.01f,
        //    Format = "{0:P0}", Order = 1, Tooltip = "This control affects all drones.")]
        //public float AlterraTransportDroneVolume { get; set; } = 1;


        [Toggle("Show Credit Messages.",Tooltip = "Enables or disabled the credit added/removed account messages on screen")]
        public bool ShowCreditMessages = true;

        [Toggle("Hide All F.C.S On-Screen Messages", Tooltip = "Prevents all FCS messages that can appear on-screen from showing. (Warning: Even important functional messages from mods will not show.)")]
        public bool HideAllFCSOnScreenMessages = false;


        private void PDAKeyCodeChangedEvent(KeybindChangedEventArgs e)
        {
            //uGUI_PowerIndicator_Initialize_Patch.MissionHUD?.UpdateButtonPressLabel();
        }

        private void ChangeGameModeEvent(ChoiceChangedEventArgs e)
        {
            onGameModeChanged?.Invoke(e.Index);
        }

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
            //QPatch.Configuration.Save();
        }
    }


    public class EncyclopediaConfig : ConfigFile
    {
        public EncyclopediaConfig() : base("encyclopedia-config", "Configurations")
        {
        }

        public Dictionary<string, List<EncyclopediaEntryData>> EncyclopediaEntries = new();
    }
}
