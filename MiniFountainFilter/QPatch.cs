using AE.MiniFountainFilter.Buildable;
using AE.MiniFountainFilter.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AE.MiniFountainFilter
{
    [QModCore]
    public class QPatch
    {
        internal static AssetBundle GlobalBundle { get; set; }
        internal static ModConfiguration Configuration { get; set; }
        internal static string Version { get; set; }
        internal static TechType BottleTechType { get; set; }

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

                LoadConfiguration();

                AddItemsToTechFabricator();

                SetWater();

                MiniFountainFilterBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.minifountainfilter.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void SetWater()
        {
            if (Configuration.BottleTechType == null)
            {
                QuickLogger.Error("Bottle TechType is null setting to default");
                BottleTechType = TechType.DisinfectedWater;
                return;
            }

            var water = Configuration.BottleTechType.ToTechType();
            
            if (water == TechType.None)
            {
                QuickLogger.Error("Bottle TechType is None setting to default");
                BottleTechType = TechType.DisinfectedWater;
                return;
            }

            BottleTechType = water;
        }
        
        private static void LoadConfiguration()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(Mod.ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            //LoadData
            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
        }

        private static void AddItemsToTechFabricator()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ClassID}.png"));
            var craftingTab = new CraftingTab(Mod.MiniFountainFilterTabID, Mod.FriendlyName, icon);

            var quantumTeleportKit = new FCSKit(Mod.MiniFountainFilterKitClassID, Mod.FriendlyName, craftingTab, Mod.MiniFountainFilterKitIngredients);
            quantumTeleportKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
