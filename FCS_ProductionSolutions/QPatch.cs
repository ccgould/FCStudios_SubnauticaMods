using System;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Craftable;
using FCS_ProductionSolutions.DeepDriller.Ores;
using FCS_ProductionSolutions.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.MatterAnalyzer.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_ProductionSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            
            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();


            //Harmony
            var harmony = new Harmony("com.productionsolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (Configuration.IsHydroponicHarvesterEnabled)
            {
                var hydroHarvester = new HydroponicHarvesterPatch();
                hydroHarvester.Patch();
            }

            if (Configuration.IsReplicatorEnabled)
            {
                var replicator = new ReplicatorBuildable();
                replicator.Patch();
            }
            
            if (Configuration.IsReplicatorEnabled || Configuration.IsHydroponicHarvesterEnabled)
            {
                var matterAnalyzer = new MatterAnalyzerPatch();
                matterAnalyzer.Patch();
            }

            if (Configuration.IsDeepDrillerEnabled)
            {
                var sand = new SandSpawnable();
                sand.Patch();

                var glass = new FcsGlassCraftable();
                glass.Patch();

                var type = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
                if (type != null)
                {
                    var pingOriginal = AccessTools.Method(type, "Refresh");
                    var pingPrefix = new HarmonyMethod(AccessTools.Method(typeof(PingMapIcon_Patch), "Prefix"));
                    harmony.Patch(pingOriginal, pingPrefix);
                }
                
                var pingSprite = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "DeepDriller_ping.png"));
                DeepDrillerPingType = WorldHelpers.CreatePingType("Deep Driller","Deep Driller",pingSprite);
                
                var deepDriller = new FCSDeepDrillerBuildable();
                deepDriller.Patch();
            }

            if (Configuration.IsAutocrafterEnabled)
            {
                var dssAutoCrafter = new DSSAutoCrafterPatch();
                dssAutoCrafter.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            QuickLogger.Info($"Finished Patching");
        }

        public static PingType DeepDrillerPingType { get; set; }

        public static class PingMapIcon_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(object __instance)
            {
                FieldInfo field = __instance.GetType().GetField("ping");
                PingInstance ping = field.GetValue(__instance) as PingInstance;
                if (ping.pingType == QPatch.DeepDrillerPingType)
                {
                    FieldInfo field2 = __instance.GetType().GetField("icon");
                    uGUI_Icon icon = field2.GetValue(__instance) as uGUI_Icon;
                    icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, "Deep Driller");
                    icon.color = Color.black;
                    RectTransform rectTransform = icon.rectTransform;
                    rectTransform.sizeDelta = Vector2.one * 28f;
                    rectTransform.localPosition = Vector3.zero;
                    return false;
                }
                return true;
            }
        }
    }
}
