using System.IO;
using FCSAlterraShipping.Buildable;
using FCSAlterraShipping.Configuration;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace FCSAlterraShipping
{
    using FCSCommon.Utilities;
    using Harmony;
    using System;
    using System.Reflection;

    [QModCore]
    public static class QPatch
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

                CraftDataHandler.SetHarvestType(TechType.Peeper, HarvestType.DamageAlive);
                CraftDataHandler.SetHarvestOutput(TechType.Peeper, TechType.AcidMushroom);

                AlterraShippingBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.alterrashipping.fcstudios");
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
            var craftingTab = new CraftingTab(Mod.AlterraShippingTabID, Mod.FriendlyName, icon);

            var alterraShippingKit = new FCSKit(Mod.AlterraShippingKitClassID, Mod.FriendlyName, craftingTab, Mod.AlterraShippingIngredients);
            alterraShippingKit.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
