using FCSAIPowerCellSocket.Buildables;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using FCSAIPowerCellSocket.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;

namespace FCSAIPowerCellSocket
{
    [QModCore]
    public class QPatch
    {
        private const string PowerCellSocketKitClassId = "PowerCellSocket_AIS";
        private const string PowerCellSocketTabId = "PSS";
        private const string PowerCellSocketTabText = "Powercell Socket";

        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                AIPowerCellSocketBuildable.PatchSMLHelper();
                
                AddTechFabricatorItems();

                var harmony = HarmonyInstance.Create("com.fcsaipowercellsocket.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void AddTechFabricatorItems()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), $"{PowerCellSocketTabId}Icon.png"));
            var craftingTab = new CraftingTab(PowerCellSocketTabId, PowerCellSocketTabText, icon);
            
            var powercellSocketKit = new FCSKit(PowerCellSocketKitClassId, Mod.ModFriendlyName, craftingTab, Mod.PowercellSocketIngredients);
            powercellSocketKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
