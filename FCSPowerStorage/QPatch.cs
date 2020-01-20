using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using System;
using System.IO;
using System.Reflection;
using FCSPowerStorage.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;


namespace FCSPowerStorage
{
    [QModCore]
    public class QPatch
    {
        private static bool _success = true;

        [QModPatch]
        public static void Patch()
        {
            QuickLogger.Info("Initializing FCS Power Storage");

            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));
            
            try
            {
#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif
                AddTechFabricatorItems();

                LoadData.Patch();

                FCSPowerStorageBuildable.PatchHelper();
            }
            catch (Exception e)
            {
                _success = false;
                QuickLogger.Error($"Error in QPatch {e.Message}");
            }
            QuickLogger.Info("FCS Power Storage initializ" + (!_success ? "ation failed." : "ed successfully."));
        }

        private static void AddTechFabricatorItems()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Information.GetAssetPath(), $"{Information.ModName}.png"));
            var craftingTab = new CraftingTab(Information.PowerStorageTabID, Information.ModFriendlyName, icon);

            var powercellSocketKit = new FCSKit(Information.PowerStorageKitClassID, Information.ModFriendlyName, craftingTab, Information.PowerStorageIngredients);
            powercellSocketKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
