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
                OxStationModelPrefab.GetPrefabs();
                OxStationBuildable.PatchHelper();
                OxStationScreenBuildable.PatchHelper();

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

            var oxStationScreenKit = new FCSKit(Mod.OxstationScreenKitClassID, Mod.ScreenFriendlyName, craftingTab, Mod.OxstationScreenIngredients);
            oxStationScreenKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var oxStationCyclopsModule = new FCSVehicleModule(Mod.OxstationCyclopsModuleClassID, Mod.CyclopsModuleFriendlyName,Mod.OxstationCyclopsModuleDescription,EquipmentType.CyclopsModule, Mod.OxstationCyclopsIngredients, craftingTab);
            oxStationCyclopsModule.ChangeIconLocation(Mod.GetAssetFolder(),"oxStation_C");
            oxStationCyclopsModule.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
            
            var oxStationSeamothModule = new FCSVehicleModule(Mod.OxstationSeamothModuleClassID, Mod.OxStationSeamothFriendlyName, Mod.OxstationSeamothModuleDescription, EquipmentType.SeamothModule, Mod.OxstationSeamothIngredients, craftingTab);
            oxStationSeamothModule.ChangeIconLocation(Mod.GetAssetFolder(), "oxStation_S");
            oxStationSeamothModule.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var oxStationPrawnModule = new FCSVehicleModule(Mod.OxstationPrawnSuitModuleClassID, Mod.OxStationPrawnSuitFriendlyName, Mod.OxstationPrawnModuleDescription, EquipmentType.ExosuitModule, Mod.OxstationPrawnSuitIngredients, craftingTab);
            oxStationPrawnModule.ChangeIconLocation(Mod.GetAssetFolder(), "oxStation_P");
            oxStationPrawnModule.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}

