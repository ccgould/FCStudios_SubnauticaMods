using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using Harmony;
using Oculus.Newtonsoft.Json;
using QModManager.API.ModLoading;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Configuration;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace QuantumTeleporter
{
    [QModCore]
    public class QPatch
    {
        internal static AssetBundle GlobalBundle { get; set; }
        
        internal static ModConfiguration Configuration { get; set; }
        
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

                Configuration = Mod.LoadConfiguration();

                AddItemsToTechFabricator();

                QuantumTeleporterBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.quantumteleporter.fcstudios");
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
            var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ClassID}.png")));
            var craftingTab = new CraftingTab(Mod.QuantumTeleporterTabID, Mod.FriendlyName, icon);
            
            var quantumTeleportScannerKit = new FCSKit(Mod.TeleporterScannerConnectionKitClassID, Mod.TeleporterScannerConnectionKitText, craftingTab, Mod.TeleporterScannerConnectionKitIngredients);
            quantumTeleportScannerKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var quantumTeleportWiringKit = new FCSKit(Mod.AdvancedTeleporterWiringKitClassID, Mod.AdvancedTeleporterWiringKitText, craftingTab, Mod.AdvancedTeleporterWiringKitIngredients);
            quantumTeleportWiringKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var quantumTeleportKit = new FCSKit(Mod.QuantumTeleporterKitClassID, Mod.QuantumTeleporterKitText, craftingTab, Mod.QuantumTeleporterKitIngredients);
            quantumTeleportKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
