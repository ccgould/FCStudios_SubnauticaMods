using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using ARS_SeaBreezeFCS32.Craftables;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace ARS_SeaBreezeFCS32
{
    [QModCore]
    public static class QPatch
    {
        internal static AssetBundle Bundle { get; set; }

        internal static AssetBundle GlobalBundle { get; set; }
        internal static ModConfiguration Configuration { get; private set; }

        [QModPatch]
        public static void Patch()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(Mod.ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
            
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));


#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                LoadAssetBundle();

                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);
                
                AddTechFabricatorItems();
                
                ARSSeaBreezeFCS32Buildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.arsseabreezefcs32.fcstudios");

                harmony.PatchAll(assembly);

                QuickLogger.Info("Finished patching");

                QuickLogger.Debug($"{Configuration.StorageLimit}");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void LoadAssetBundle()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.BundleName,Mod.ExecutingDirectory);

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            Bundle = assetBundle;
        }

        private static void AddTechFabricatorItems()
        {
            var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetFolder(), "ARSSeaBreeze.png")));
            var craftingTab = new CraftingTab(Mod.SeaBreezeTabID, Mod.FriendlyName, icon);

            var freon = new FreonPatcher("Freon_ARS", "Freon", "Freon gives your SeaBreeze cooling on Planet 4546B.", craftingTab);
            freon.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var seaBreezeKit = new FCSKit(Mod.SeaBreezeKitClassID, Mod.FriendlyName, craftingTab, Mod.SeaBreezeIngredients);
            seaBreezeKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}

