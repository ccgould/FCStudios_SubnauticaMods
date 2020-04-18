using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using Harmony;
using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.Upgrades;
using OxStationCyclopsModule.Config;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;

namespace OxStationCyclopsModule
{
    [QModCore]
    public class QPatch
    {
        [QModPatch]
        public static void Patch()
        {
            try
            {
                QuickLogger.Info(
                    $"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif

                AddTechFabricatorItems();

                MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) => new UpgradeHandler(Mod.ModuleTechType, cyclops));

                HarmonyInstance.Create("com.oxstationcyclops.fcs").PatchAll(Assembly.GetExecutingAssembly());
                QuickLogger.Info("Finished patching");

            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void AddTechFabricatorItems()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "OxStation.png"));
            var craftingTab = new CraftingTab("OX", "Oxstation", icon);

            var h = new FCSVehicleModule("oxstationcyclopsmodule", "Oxstation Cyclops Module", "A oxstation module for the cyclops", EquipmentType.CyclopsModule, craftingTab, Mod.OxstationCyclopsIngredients);
            h.ChangeIconLocation(Mod.GetAssetFolder(), "icon");
            h.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
            Mod.ModuleTechType = h.TechType;
        }
    }
}
