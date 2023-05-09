using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace FCS_AlterraHub.Configuation;

[Menu("FCS AlterraHub Menu")]
public class Config : ConfigFile
{
    public Config() : base("alterrahub-config", "Configurations")
    {
    }

    [Toggle("Enable Debugs", Order = 0), OnChange(nameof(EnableDebugsToggleEvent))]
    public bool EnableDebugLogs = false;

    internal UnityAction<int> onGameModeChanged;

    [Keybind("Open/Close FCS PDA"), OnChange(nameof(PDAKeyCodeChangedEvent))]
    public KeyCode FCSPDAKeyCode = KeyCode.F2;

    [Keybind("FCS PDA Information Button"), OnChange(nameof(PDAKeyCodeChangedEvent))]
    public KeyCode PDAInfoKeyCode = KeyCode.I;

    [Slider("Ore Payout Difficulty", 0.1f, 1.9f, DefaultValue = 1, Step = 0.1f, Format = "{0}", Tooltip = "The higher the value the more payout per ore.")]
    public float OrePayoutMultiplier = 1f;

    [Toggle("Play Sound Effects"), OnChange(nameof(PlaySoundToggleEvent))]
    public bool PlaySFX = true;

    private void PlaySoundToggleEvent(ToggleChangedEventArgs e)
    {
        OnPlaySoundToggleEvent?.Invoke(e.Value);
    }

    internal Action<bool> OnPlaySoundToggleEvent { get; set; }

    [Keybind("FCS DevicePage Interface Information Button")]
    public KeyCode PDASettingsKeyCode = KeyCode.F2;

    [Toggle("[Alterra Transport Drone] Enable Drone Audio", Order = 1, Tooltip = "Enables/Disables the sound effects on the drone.")]
    public bool AlterraTransportDroneFxAllowed = true;

    [Toggle("Show Credit Messages.", Tooltip = "Enables or disabled the credit added/removed account messages on screen")]
    public bool ShowCreditMessages = true;

    [Toggle("Hide All F.C.S On-Screen Messages", Tooltip = "Prevents all FCS messages that can appear on-screen from showing. (Warning: Even important functional messages from mods will not show.)")]
    public bool HideAllFCSOnScreenMessages = false;

    [Slider("Credit Message Delay", 0f, 600f, DefaultValue = 0, Step = 30.0f, Format = "{0}", Tooltip = "Delays credit messages by the selected amount in seconds.")]
    public float CreditMessageDelay = 0f;

    [Toggle("Ore Mode", Tooltip = "Removes the need for credit to build items. (Game restart required!)")]
    public bool OreBuildMode = false;


    private void PDAKeyCodeChangedEvent(KeybindChangedEventArgs e)
    {

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
    }
}
