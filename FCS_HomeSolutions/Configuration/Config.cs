using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_HomeSolutions.Configuration
{
    
    [Menu("FCS Home Solutions Menu")]
    public class Config : ConfigFile
    {
        public Config() : base("homeSolutions-config","Configurations") { }

        [Toggle("[Home Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        public FCSGameMode ModMode;

        #region Stove

        [Toggle("[Stove] Is Mod Enabled", Order = 1, Tooltip = "Enables/Disables Alien Chef from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsStoveEnabled = true;

        #endregion

        #region Mini Fountain Filter


        [Toggle("[Mini Fountain Filter] Is Mod Enabled", Order = 2, Tooltip = "Enables/Disables Mini Fountain Filter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroys all objects before disabling a mod)")]
        public bool IsMiniFountainFilterEnabled = true;

        [Toggle("[Mini Fountain Filter] Enable/Disable SoundFX", Order = 2)]
        public bool MiniFountainFilterPlaySFX = true;

        public float MiniFountainFilterTankCapacity = 100f;
        public float MiniFountainFilterEnergyPerSec = 0.425f;
        public int MiniFountainFilterStorageWidth = 3;
        public int MiniFountainFilterStorageHeight = 2;
        public float MiniFountainFilterWaterPerSecond = 1f;
        public bool MiniFountainFilterAutoGenerateMode = false;

        #endregion

        #region SeaBreeze

        [Toggle("[SeaBreeze] Is Mod Enabled", Order = 3, Tooltip ="Enables/Disables SeaBreeze from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSeaBreezeEnabled = true;
        public int SeaBreezeStorageLimit = 100;
        public float SeaBreezePowerUsage = 0.5066666666666667f;
        public bool SeaBreezeUseBasePower = true;

        [Choice("[SeaBreeze] Game Mode", Order = 3), OnChange(nameof(ChangeGameModeEvent))]
        public FCSGameMode SeaBreezeModMode = FCSGameMode.HardCore;

        private void ChangeGameModeEvent(ChoiceChangedEventArgs e)
        {
            OnSeaBreezeGameModeChanged?.Invoke(e.Index);
        }

        internal Action<int> OnSeaBreezeGameModeChanged { get; set; }
        public List<string> AlienChiefCustomFoodTrees { get; set; } = new List<string>
        {
            "CF3mod",
            "CookFab"
        };
        
        #endregion

        #region Paint Tool

        [Toggle("[Paint Tool] Is Mod Enabled", Order = 4, Tooltip ="Enables/Disables Paint Tool from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsPaintToolEnabled = true;

        [Keybind("[Paint Tool] Select Color Forward", Order = 4, Tooltip = "Selects the next color on the paint tool")]
        public KeyCode PaintToolSelectColorForwardKeyCode = KeyCode.RightArrow;

        [Keybind("[Paint Tool] Select Color Back", Order = 4, Tooltip = "Selects the previous color on the paint tool")]
        public KeyCode PaintToolSelectColorBackKeyCode = KeyCode.LeftArrow;

        [Keybind("[Paint Tool] Sample Color Template", Order = 4, Tooltip = "Gets the color template from the object in view.")]
        public KeyCode PaintToolColorSampleKeyCode = KeyCode.P;

        public List<AdditionalColor> PaintToolAdditionalPaintColors = new List<AdditionalColor>
        {
            new AdditionalColor
            {
                Color = new Vec4(178f,34f,34f,255),
                ColorName = "FireBrick"
            }
        };

        #endregion

        #region Quantum Teleporter
        [Toggle("[Quantum Teleporter] Is Mod Enabled", Order = 5, Tooltip = "Enables/Disables Quantum Teleporter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsQuantumTeleporterEnabled = true;
        [Slider("[Quantum Teleporter] Portal Trail Brightness",0,1, Step = 0.1f, Format="{0:F2}", DefaultValue = 1, Order = 5,
         Tooltip ="Allows you to adjust the brightness of the trail effect in the teleporter effects."),OnChange(nameof(PortalBrightnessChangeEvent))]
        public float QuantumTeleporterPortalTrailBrightness = 1;

        private static void PortalBrightnessChangeEvent(SliderChangedEventArgs e)
        {
            BaseManager.GlobalNotifyByID(QuantumTeleporterBuildable.QuantumTeleporterTabID,"UpdateTeleporterEffects");
        }

        [JsonProperty] internal float QuantumTeleporterGlobalTeleportPowerUsage = 1500f;
        [JsonProperty] internal float QuantumTeleporterInternalTeleportPowerUsage = 150f;

        #endregion

        #region Televisions

        [Toggle("[Smart Televisions] Is Mod Enabled", Order = 6, Tooltip ="Enables/Disables Smart Televisions from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSmartTelevisionEnabled = true;

        [Keybind("[Smart Televisions] Volume Up", Order = 6, Tooltip ="Changes the keybind for the volume up for the tv.")]
        public KeyCode SmartTelevisionsVolumeUp = KeyCode.UpArrow;

        [Keybind("[Smart Televisions] Volume Down", Order = 6, Tooltip ="Changes the keybind for the volume down for the tv.")]
        public KeyCode SmartTelevisionsVolumeDown = KeyCode.DownArrow;

        //[Keybind("[Smart Televisions] Channel Up", Tooltip="Changes the keybind for the channel up for the tv.")]
        //public KeyCode SmartTelevisionsChannelUp = KeyCode.RightArrow;

        //[Keybind("[Smart Televisions] Channel Down",Tooltip="Changes the keybind for the channel down for the tv.")]
        //public KeyCode SmartTelevisionsChannelDown = KeyCode.LeftArrow;

        [Keybind("[Smart Televisions] Turn On/Off Tv", Order = 6, Tooltip ="Changes the keybind for turning the tv on or off.")]
        public KeyCode SmartTelevisionsToggleTv = KeyCode.F;

        #endregion

        #region Railings

        [Toggle("[Railings] Is Mod Enabled", Order = 7, Tooltip ="Enables/Disables railings from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsRailingsEnabled = true;

        #endregion

        #region Peeper Lounge Bar

        [Toggle("[Peeper Lounge Bar] Is Mod Enabled", Order = 8, Tooltip = "Enables/Disables Peeper Lounge Bar from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsPeeperLoungeBarEnabled = true;

        [Slider("[Peeper Lounge Bar] Turn speed.", 0, 5, Step = 0.1f, Format = "{0:F2}", DefaultValue = 5,Order = 8, Tooltip = "Allows you to adjust the turn speed.")]
        public float PeeperLoungeBarTurnSpeed = 5;

        [Toggle("[Peeper Lounge Bar] Play SFX", Order = 8, Tooltip = "Enables/Disables Peeper Lounger Bar voice from playing")]
        public bool PeeperLoungeBarPlayVoice { get; set; } = true;

        [Toggle("[Elevator] Is Mod Enabled", Order = 8, Tooltip = "Enables/Disables Elevator from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsElevatorEnabled { get; set; } = true;
        
        #endregion

        #region Neon Planter

        [Toggle("[Neon Planter] Is Mod Enabled", Order = 9, Tooltip ="Enables/Disables Smart OutDoor Planter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsNeonPlanterEnabled = true;

        #endregion

        #region Fire Extinguisher Refueler

        [Toggle("[Fire Extinguisher Refueler] Is Mod Enabled",Order = 10,Tooltip="Enables/Disables Fire Extinguisher Refueler from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsFireExtinguisherRefuelerEnabled = true;

        #endregion

        #region Trash Receptacle

        [Toggle("[Trash Receptacle] Is Mod Enabled", Order = 11, Tooltip ="Enables/Disables Trash Receptacle from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTrashReceptacleEnabled = true;

        #endregion                
        
        #region Trash Recycler

        [Toggle("[Trash Recycler] Is Mod Enabled", Order = 12, Tooltip ="Enables/Disables Trash Recycler from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTrashRecyclerEnabled = true;

        #endregion        
        
        #region Curtain

        [Toggle("[Curtain] Is Mod Enabled", Order = 13, Tooltip ="Enables/Disables Curtain from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsCurtainEnabled = true;

        #endregion
        
        #region Shower

        [Toggle("[Shower] Is Mod Enabled", Order = 14, Tooltip ="Enables/Disables Mini Bathroom from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsShowerEnabled = true;

        #endregion

        #region Wall Signs

        [Toggle("[Wall Signs] Is Mod Enabled", Order = 15, Tooltip ="Enables/Disables Wall Signs from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsWallSignsEnabled = true;

        #endregion

        #region Cabinets

        [Toggle("[Cabinets] Is Mod Enabled", Order = 15, Tooltip ="Enables/Disables Cabinets from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsCabinetsEnabled = true;

        #endregion        
        
        #region Shelves and Tables

        [Toggle("[Shelves and Tables] Is Mod Enabled", Order = 15, Tooltip ="Enables/Disables Shelves and Tables from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsShelvesAndTablesEnabled = true;

        #endregion

        #region LED Lights

        [Toggle("[LED Lights] Is Mod Enabled", Order = 16, Tooltip ="Enables/Disables LED Lights and Tables from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsLEDLightsEnabled = true;

        [Keybind("[LED Lights] Decrease Light Intensity", Order = 16, Tooltip ="Changes how bright the light is.")]
        public KeyCode LEDLightBackwardKeyCode = KeyCode.LeftArrow;
        [Keybind("[LED Lights] Increase Light Intensity", Order = 16, Tooltip ="Changes how bright the light is.")]
        public KeyCode LEDLightForwardKeyCode = KeyCode.RightArrow;
        [Keybind("[LED Lights] Increase Light Intensity", Order = 16, Tooltip ="Enables/Disables the night sensor which turns the light off during the day and on in the night")]
        public KeyCode LEDLightNightSensorToggleKeyCode = KeyCode.K;

        #endregion


        [Toggle("[Hatch Stairway] Is Mod Enabled", Order = 17, Tooltip = "Enables/Disables Hatch Stairway from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsHatchStairwayEnabled { get; set; } = true;

        [Toggle("[Hatch Stairway] Limit length over base", Order = 18, Tooltip = "Enabled: always builds short stairs for hatch directly on platform. Disabled: builds full length stairs which will penetrate base objects.")]
        public bool StairsLimitLengthOverBasePieces { get; set; } = false;

        [Toggle("Developer Mode", Order = 0, Tooltip = "For use by FCStudios Developers.")]
        public bool DeveloperModeEnabled { get; set; } = false;

        internal bool IsWallPartitionsEnabled { get; set; } = false;


        #region JukeBox
        [Toggle("[Juke Box] Is Mod Enabled", Order = 19, Tooltip = "Enables/Disables Juke Box from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]

        public bool IsJukeBoxEnabled = true;

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
}
