using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
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

        [Toggle("[Home Solutions] Enable Debugs", Order = 0), OnChange(nameof(EnableDebugsToggleEvent)), Tooltip("Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)")]
        public bool EnableDebugLogs = false;

        public FCSGameMode ModMode;

        #region Alien Chef

        [Toggle("[Alien Chef] Is Mod Enabled"),
         Tooltip("Enables/Disables Alien Chef from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsAlienChiefEnabled = true;

        #endregion

        #region Mini Fountain Filter


        [Toggle("[Mini Fountain Filter] Is Mod Enabled"),
         Tooltip("Enables/Disables Mini Fountain Filter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroys all objects before disabling a mod)")]
        public bool IsMiniFountainFilterEnabled = true;

        [Toggle("[Mini Fountain Filter] Enable/Disable SoundFX")]
        public bool MiniFountainFilterPlaySFX = true;
        public float MiniFountainFilterTankCapacity = 100f;
        public float MiniFountainFilterEnergyPerSec = 0.425f;
        public int MiniFountainFilterStorageWidth = 3;
        public int MiniFountainFilterStorageHeight = 2;
        public float MiniFountainFilterWaterPerSecond = 1f;
        public bool MiniFountainFilterAutoGenerateMode = false;

        #endregion

        #region SeaBreeze

        [Toggle("[SeaBreeze] Is Mod Enabled"),
         Tooltip("Enables/Disables SeaBreeze from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSeaBreezeEnabled = true;
        public int SeaBreezeStorageLimit = 100;
        public float SeaBreezePowerUsage = 0.5066666666666667f;
        public bool SeaBreezeUseBasePower = true;

        [Choice("[SeaBreeze] Game Mode"), OnChange(nameof(ChangeGameModeEvent))]
        public FCSGameMode SeaBreezeModMode = FCSGameMode.HardCore;

        private void ChangeGameModeEvent(ChoiceChangedEventArgs e)
        {
            OnSeaBreezeGameModeChanged?.Invoke(e.Index);
        }

        public Action<int> OnSeaBreezeGameModeChanged { get; set; }
        public List<string> AlienChiefCustomFoodTrees { get; set; } = new List<string>
        {
            "CF3mod",
            "CookFab"
        };

        #endregion

        #region Paint Tool

        [Toggle("[Paint Tool] Is Mod Enabled"),
         Tooltip("Enables/Disables Paint Tool from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsPaintToolEnabled = true;

        [Keybind("[Paint Tool] Select Color Forward"),Tooltip("Selects the next color on the paint tool")]
        public KeyCode PaintToolSelectColorForwardKeyCode = KeyCode.RightArrow;

        [Keybind("[Paint Tool] Select Color Back"), Tooltip("Selects the previous color on the paint tool")]
        public KeyCode PaintToolSelectColorBackKeyCode = KeyCode.LeftArrow;

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
        [Toggle("[Quantum Teleporter] Is Mod Enabled"),
         Tooltip("Enables/Disables Quantum Teleporter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsQuantumTeleporterEnabled = true;
        [Slider("[Quantum Teleporter] Portal Trail Brightness",0,1, Step = 0.1f, Format="{0:F2}", DefaultValue = 1),
         Tooltip("Allows you to adjust the brightness of the trail effect in the teleporter effects."),OnChange(nameof(PortalBrightnessChangeEvent))]
        public float QuantumTeleporterPortalTrailBrightness = 1;

        private static void PortalBrightnessChangeEvent(SliderChangedEventArgs e)
        {
            BaseManager.GlobalNotifyByID(Mod.QuantumTeleporterTabID,"UpdateTeleporterEffects");
        }

        [JsonProperty] internal float QuantumTeleporterGlobalTeleportPowerUsage = 1500f;
        [JsonProperty] internal float QuantumTeleporterInternalTeleportPowerUsage = 150f;

        #endregion

        #region Televisions

        [Toggle("[Smart Televisions] Is Mod Enabled"),
         Tooltip("Enables/Disables Smart Televisions from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSmartTelevisionEnabled = true;

        [Keybind("[Smart Televisions] Volume Up"), Tooltip("Changes the keybind for the volume up for the tv.")]
        public KeyCode SmartTelevisionsVolumeUp = KeyCode.UpArrow;

        [Keybind("[Smart Televisions] Volume Down"), Tooltip("Changes the keybind for the volume down for the tv.")]
        public KeyCode SmartTelevisionsVolumeDown = KeyCode.DownArrow;

        [Keybind("[Smart Televisions] Channel Up"), Tooltip("Changes the keybind for the channel up for the tv.")]
        public KeyCode SmartTelevisionsChannelUp = KeyCode.RightArrow;

        [Keybind("[Smart Televisions] Channel Down"), Tooltip("Changes the keybind for the channel down for the tv.")]
        public KeyCode SmartTelevisionsChannelDown = KeyCode.LeftArrow;

        [Keybind("[Smart Televisions] Turn On/Off Tv"), Tooltip("Changes the keybind for turning the tv on or off.")]
        public KeyCode SmartTelevisionsToggleTv = KeyCode.F;

        #endregion

        #region Railings

        [Toggle("[Railings] Is Mod Enabled"),
         Tooltip("Enables/Disables railings from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsRailingsEnabled = true;

        #endregion

        #region Sweet Water Bar

        [Toggle("[Sweet Water Bar] Is Mod Enabled"),
         Tooltip("Enables/Disables Sweet Water Bar from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSweetWaterBarEnabled = true;

        #endregion

        #region Smart OutDoor Planter

        [Toggle("[Smart OutDoor Planter] Is Mod Enabled"),
         Tooltip("Enables/Disables Smart OutDoor Planter from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsSmartOutDoorPlanterEnabled = true;

        #endregion

        #region Fire Extinguisher Refueler

        [Toggle("[Fire Extinguisher Refueler] Is Mod Enabled"),
         Tooltip("Enables/Disables Fire Extinguisher Refueler from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsFireExtinguisherRefuelerEnabled = true;

        #endregion

        #region Trash Receptacle

        [Toggle("[Trash Receptacle] Is Mod Enabled"),
         Tooltip("Enables/Disables Trash Receptacle from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTrashReceptacleEnabled = true;

        #endregion                
        
        #region Trash Recycler

        [Toggle("[Trash Recycler] Is Mod Enabled"),
         Tooltip("Enables/Disables Trash Recycler from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsTrashRecyclerEnabled = true;

        #endregion        
        
        #region Curtain

        [Toggle("[Curtain] Is Mod Enabled"),
         Tooltip("Enables/Disables Curtain from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsCurtainEnabled = true;

        #endregion

        #region Observation Tank

        [Toggle("[Observation Tank] Is Mod Enabled"),
         Tooltip("Enables/Disables Observation Tank from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsObservationTankEnabled = true;

        #endregion

        #region Mini Bathroom

        [Toggle("[Mini Bathroom] Is Mod Enabled"),
         Tooltip("Enables/Disables Mini Bathroom from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsAlterraMiniBathroomEnabled = true;

        #endregion

        #region Wall Signs

        [Toggle("[Wall Signs] Is Mod Enabled"),
         Tooltip("Enables/Disables Wall Signs from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsWallSignsEnabled = true;

        #endregion

        #region Cabinets

        [Toggle("[Cabinets] Is Mod Enabled"),
         Tooltip("Enables/Disables Cabinets from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsCabinetsEnabled = true;

        #endregion        
        
        #region Shelves and Tables

        [Toggle("[Shelves and Tables] Is Mod Enabled"),
         Tooltip("Enables/Disables Shelves and Tables from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsShelvesAndTablesEnabled = true;

        #endregion

        #region LED Lights

        [Toggle("[LED Lights] Is Mod Enabled"),
         Tooltip("Enables/Disables LED Lights and Tables from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsLEDLightsEnabled = true;

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
