using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using FCS_AIMarineTurbine.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AIMarineTurbine
{
    [QModCore]
    public static class QPatch
    {
        public static AssetBundle Bundle { get; private set; }
        
        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
            //TODO remove on release
            //AIWindSurferBuildable.PatchSMLHelper(); 
#endif

            try
            {
                LoadAssetBundle();

                AddItemsToTechFabricator();

                AISolutionsData.PatchHelper();
                AIJetStreamT242Buildable.PatchSMLHelper();
                AIMarineMonitorBuildable.PatchSMLHelper();
      
                var harmony = HarmonyInstance.Create("com.aijetstreamt242.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
                
                QuickLogger.Debug("Unload Bundle");
                Bundle.Unload(false);
                QuickLogger.Debug("Bundle Unloaded");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex.Message);
            }
        }

        private static void LoadAssetBundle()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset($"FCS_MarineTurbine", "aimarineturbinemodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            Bundle = assetBundle;
        }

        private static void AddItemsToTechFabricator()
        {
            var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.MarineMonitorClassID}.png")));
            var craftingTab = new CraftingTab(Mod.MarineTurbinesTabID, Mod.MarineTurbinesFriendlyName, icon);

            var jetStreamT242Kit = new FCSKit(Mod.JetstreamKitClassID, Mod.JetStreamFriendlyName, craftingTab, Mod.JetstreamKitIngredients);
            jetStreamT242Kit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var marineMonitorKit = new FCSKit(Mod.MarineMontiorKitClassID, Mod.MarineMonitorFriendlyName, craftingTab, Mod.MarineMonitorKitIngredients);
            marineMonitorKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
