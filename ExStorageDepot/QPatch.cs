using ExStorageDepot.Buildable;
using ExStorageDepot.Configuration;
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


namespace ExStorageDepot
{
    [QModCore]
    public static class QPatch
    {
        internal static Config Config { get; private set; }

        [QModPatch]
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly()));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                LoadConfiguration();

                AddTechFabricatorItems();
                
                ExStorageDepotBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.exstoragedepot.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void LoadConfiguration()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(Mod.ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            //LoadData
            Config = JsonConvert.DeserializeObject<Config>(configJson, settings);
        }

        private static void AddTechFabricatorItems()
        {
            var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(), $"{Mod.ClassID}.png")));
            var craftingTab = new CraftingTab(Mod.ExStorageTabID, Mod.ModFriendly, icon);

            var miniMedBayKit = new FCSKit(Mod.ExStorageKitClassID, Mod.ModFriendly, craftingTab, Mod.ExStorageIngredients);
            miniMedBayKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
