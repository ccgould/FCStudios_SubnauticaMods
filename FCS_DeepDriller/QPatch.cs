using FCS_DeepDriller.Ores;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using FCS_DeepDriller.Buildable.MK1;
using FCS_DeepDriller.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;


namespace FCS_DeepDriller
{
    [QModCore]
    public static class QPatch
    {
        public static DeepDrillerCfg Configuration { get; private set; }

        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif
#if USE_ExStorageDepot
            QuickLogger.Info("Ex-Storage Version");
#endif
            try
            {
                
                Configuration = Mod.LoadConfiguration();
                Configuration.Convert();

                OptionsPanelHandler.RegisterModOptions(new Options());
                
                AddItemsToTechFabricator();

                FCSDeepDrillerBuildable.PatchHelper();

                SandSpawnable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fcsdeepdriller.fcstudios");
                
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void AddItemsToTechFabricator()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "FCSDeepDriller.png"));
            var craftingTab = new CraftingTab(Mod.DeepDrillerTabID, Mod.ModFriendlyName, icon);
            
            var deepDrillerKit = new FCSKit(Mod.DeepDrillerKitClassID, Mod.DeepDrillerKitFriendlyName, craftingTab, Mod.DeepDrillerKitIngredients);
            deepDrillerKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
