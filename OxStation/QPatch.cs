using Harmony;
using MAC.OxStation.Buildables;
using MAC.OxStation.Config;
using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace MAC.OxStation
{

    [QModCore]
    public static class QPatch
    {
        internal static Configuration Configuration { get; set; }

        [QModPatch]
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                Configuration = Mod.LoadConfiguration();

                AddTechFabricatorItems();

                OptionsPanelHandler.RegisterModOptions(new Options());

                OxStationBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.oxstation.MAC");

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
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "OxStation.png"));
            var craftingTab = new CraftingTab(Mod.OxstationTabID, Mod.FriendlyName, icon);
            
            var oxstationKit = new FCSKit(Mod.OxstationKitClassID, Mod.FriendlyName, craftingTab, Mod.OxstationIngredients);
            oxstationKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}

