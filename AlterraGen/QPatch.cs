using System;
using System.IO;
using System.Reflection;
using AlterraGen.Buildables;
using AlterraGen.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AlterraGen
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
                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI
                        .GlobalBundleName);

                if (GlobalBundle == null)
                {
                    QuickLogger.Error("Global Bundle has returned null stopping patching");
                    throw new FileNotFoundException("Bundle failed to load");
                }

                Configuration = Mod.LoadConfiguration();

                AddTechFabricatorItems();

                var alterraGen = new AlterraGenBuildable();
                alterraGen.Patch();

                //var harmony = new Harmony("com.alterragen.fcstudios");

                //harmony.PatchAll(Assembly.GetExecutingAssembly());

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

            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ModClassName}.png"));
            var craftingTab = new CraftingTab(Mod.ModTabID, Mod.ModFriendlyName, icon);

            var alterraGenKit = new FCSKit(Mod.AlterraGenKitClassID, Mod.ModFriendlyName, craftingTab, Mod.AlterraGenIngredients);
            alterraGenKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}

