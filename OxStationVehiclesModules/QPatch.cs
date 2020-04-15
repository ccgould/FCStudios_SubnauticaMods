using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using Harmony;
using OxStationVehiclesModules.Configuration;
using QModManager.API.ModLoading;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;

namespace OxStationVehiclesModules
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

                HarmonyInstance.Create("com.oxstationvehicles.fcs").PatchAll(Assembly.GetExecutingAssembly());
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
            
            var h = new FCSVehicleModule("oxstationvehiclemodule", "Oxstation Vehicle Module", "A oxstation module for the seamoth and prawn suit", EquipmentType.VehicleModule, craftingTab, Mod.Ingredients);
            h.ChangeIconLocation(Mod.GetAssetFolder(), "icon");
            h.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
            Mod.ModuleTechType = h.TechType;
        }
    }
}
