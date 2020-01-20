using AMMiniMedBay.Buildable;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using AMMiniMedBay.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;

namespace AMMiniMedBay
{
    [QModCore]
    public class QPatch
    {
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
                AddTechFabricatorItems();

                AMMiniMedBayBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.amminimedbay.fcstudios");

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
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), $"{Mod.ClassID}.png"));
            var craftingTab = new CraftingTab(Mod.MiniMedBayTabID, Mod.ModFriendlyName, icon);

            var miniMedBayKit = new FCSKit(Mod.MiniMedBayKitClassID, Mod.ModFriendlyName, craftingTab, Mod.MiniMedBayIngredients);
            miniMedBayKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
