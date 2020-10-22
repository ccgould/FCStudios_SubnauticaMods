using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine.Events;

namespace FCS_AlterraHub.Configuration
{
    [Menu("AlterraHub Menu")]
    internal class Config : ConfigFile
    {

        [Toggle("Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        [JsonIgnore]
        public UnityAction<int> onGameModeChanged;
        

        public List<CustomStoreItem> AdditionalStoreItems = new List<CustomStoreItem>();
        
        [Choice("Mode Game Mode"), OnChange(nameof(ChangeGameModeEvent))]
        public FCSGameMode GameModeOption { get; set; }

        [Toggle("Play Sound Effects"), OnChange(nameof(PlaySoundToggleEvent))]
        public bool PlaySFX = true;

        private void PlaySoundToggleEvent(ToggleChangedEventArgs e)
        {
            OnPlaySoundToggleEvent?.Invoke(e.Value);
        }

        [JsonIgnore]
        internal Action<bool> OnPlaySoundToggleEvent { get; set; }

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
}
