using AE.SeaCooker.Buildable;
using AE.SeaCooker.Configuration;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using FCSCommon.Helpers;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AE.SeaCooker
{
    [QModCore]
    public class QPatch
    {
        internal static ConfigFile Configuration { get; set; }
        internal static string Version { get; set; }
        internal static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle AssetBundle { get; set; }

        [QModPatch]
        public static void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");


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

                QuickLogger.Info($"Storage:{Configuration.Config.StorageHeight}X{Configuration.Config.StorageWidth}");

                AddTechFabricatorItems();

                SeaCookerBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.seacooker.fcstudios");
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

            if (AssetBundle == null)
            {
                QuickLogger.Debug("GetPrefabs");
                AssetBundle = AssetHelper.Asset(Mod.ModName, Mod.BundleName);
            }
            
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ClassID}.png"));
            var craftingTab = new CraftingTab(Mod.SeaCookerTabID, Mod.FriendlyName, icon);

            var seaCookerKit = new FCSKit(Mod.SeaCookerKitClassID, Mod.FriendlyName, craftingTab, Mod.SeaCookerIngredients);
            seaCookerKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}