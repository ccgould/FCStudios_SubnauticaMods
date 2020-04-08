using System;
using System.IO;
using System.Reflection;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCSCommon.Exceptions;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using FCSTechFabricator.Objects;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HydroponicHarvesters
{
    [QModCore]
    public class QPatch
    {
        internal static ConfigFile Configuration { get; private set; }

        [QModPatch]
        public static void Patch()
        {
            try
            {
                QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif

                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);

                Configuration = Mod.LoadConfiguration();

                AddTechFabricatorItems();

                if (HydroponicHarvestersModelPrefab.GetPrefabs())
                {
                    var hydroHavesterLarge = new HydroponicHarvestersBuidable(Mod.LargeClassID, Mod.LargeFriendlyName, Mod.LargeDescription,
                        new Vector3(2.115407f, 2.37773f, 2.245471f), new Vector3(0f, -0.9747202f, 0.03156948f), Mod.LargeHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.LargePrefab,Mod.LargeBubblesLocations);
                    hydroHavesterLarge.Patch();

                    QuickLogger.Debug("Patched Large");

                    var hydroHavesterMedium = new HydroponicHarvestersBuidable(Mod.MediumClassID, Mod.MediumFriendlyName, Mod.MediumDescription,
                        new Vector3(0.0119499f, 1.639193f, 0.02987486f), new Vector3(2.075496f, 2.238697f, 2.183046f), Mod.MediumHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.MediumPrefab, Mod.MediumBubblesLocations);
                    hydroHavesterMedium.Patch();

                    QuickLogger.Debug("Patched Medium");

                    var hydroHavesterSmall = new HydroponicHarvestersBuidable(Mod.SmallClassID, Mod.SmallFriendlyName, Mod.SmallDescription,
                        new Vector3(0.0119499f, 1.639193f, 0.02987486f), new Vector3(2.075496f, 2.238697f, 2.183046f), Mod.SmallHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.SmallPrefab, Mod.SmallBubblesLocations);
                    hydroHavesterSmall.Patch();

                    QuickLogger.Debug("Patched Small");

                }
                else
                {
                    throw new PatchTerminatedException("Failed to get the prefabs from the asset bundle");
                }

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        internal static AssetBundle GlobalBundle { get; set; }


        private static void AddTechFabricatorItems()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "HydroHarv.png"));
            var craftingTab = new CraftingTab(Mod.HydroHarvTabID, Mod.ModFriendlyName, icon);

            var largeHydroHarv = new FCSKit(Mod.LargeHydroHarvKitClassID, Mod.LargeFriendlyName, craftingTab, Mod.LargeHydroHarvIngredients);
            largeHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var mediumHydroHarv = new FCSKit(Mod.MediumHydroHarvKitClassID, Mod.MediumFriendlyName, craftingTab, Mod.MediumHydroHarvIngredients);
            mediumHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var smallHydroHarv = new FCSKit(Mod.SmallHydroHarvKitClassID, Mod.SmallFriendlyName, craftingTab, Mod.SmallHydroHarvIngredients);
            smallHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            foreach (var dnaSample in Mod.DNASamples)
            {
                var dna = new FCSDNASample(dnaSample.ClassID, dnaSample.Friendly, dnaSample.Description,dnaSample.Ingredient, dnaSample.Amount);
                dna.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
            }
        }
    }
}
