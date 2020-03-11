using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using GasPodCollector.Buildables;
using GasPodCollector.Configuration;
using Harmony;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace GasPodCollector
{
    [QModCore]
    public class QPatch
    {
        [QModPatch]
        public static void Patch()
        {
            var version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {version}");


#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif
            try
            {

                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);
                
                if (GlobalBundle == null)
                {
                    QuickLogger.Error("Global Bundle has returned null stopping patching");
                    throw new FileNotFoundException("Bundle failed to load");
                }

                AddItemsToTechFabricator();

                GaspodCollectorBuildable.PatchHelper();

                Configuration = Mod.LoadConfiguration();
                
                var harmony = HarmonyInstance.Create("com.gaspodcollector.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }

        }

        internal static ConfigFile Configuration { get; set; }

        private static void AddItemsToTechFabricator()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ClassID}.png"));
            var craftingTab = new CraftingTab(Mod.GasPodCollectorTabID, Mod.FriendlyName, icon);

            var gaspadKit = new FCSKit(Mod.GaspodCollectorKitClassID, Mod.FriendlyName, craftingTab, Mod.GaspodCollectorIngredients);
            gaspadKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }

        public static AssetBundle GlobalBundle { get; set; }
    }
}
