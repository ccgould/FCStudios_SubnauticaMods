using FCS_DeepDriller.Ores;
using FCSCommon.Utilities;
using System;
using System.IO;
using System.Reflection;
using FCS_DeepDriller.Buildable.MK2;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Craftable;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using HarmonyLib;
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

            try
            {
                Configuration = Mod.LoadConfiguration();

                OptionsPanelHandler.RegisterModOptions(new Options());
                
                AddItemsToTechFabricator();

                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);

                FCSDeepDrillerBuildable.PatchHelper();

                SandSpawnable.PatchHelper();

                var harmony = new Harmony("com.fcsdeepdriller.fcstudios");
                
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        public static AssetBundle GlobalBundle { get; set; }

        private static void AddItemsToTechFabricator()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "FCSDeepDriller.png"));
            var craftingTab = new CraftingTab(Mod.DeepDrillerTabID, Mod.ModFriendlyName, icon);
            
            var deepDrillerKit = new FCSKit(Mod.DeepDrillerKitClassID, Mod.DeepDrillerKitFriendlyName, craftingTab, Mod.DeepDrillerKitIngredients);
            deepDrillerKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var FCSGlass = new FCSGlassCraftable(craftingTab);
            FCSGlass.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

        }
    }
}
