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
    internal class Config : ConfigFile
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

        public List<FCSStoreEntry> AdditionalStoreItems = new List<FCSStoreEntry>();

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

        private void PDAKeyCodeChangedEvent(KeybindChangedEventArgs e)
        {
            uGUI_PowerIndicator_Initialize_Patch.MissionHUD?.UpdateButtonPressLabel();
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

        public Dictionary<string, List<EncyclopediaEntryData>> EncyclopediaEntries = new Dictionary<string, List<EncyclopediaEntryData>>();
    }
}
