using System;
using System.IO;
using System.Reflection;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Craftable;
using FCS_HydroponicHarvesters.Model;
using FCS_HydroponicHarvesters.Patches;
using FCSCommon.Exceptions;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using Harmony;
using QModManager.API;
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

                OptionsPanelHandler.RegisterModOptions(new Options());

                AddTechFabricatorItems();

                var harmony = HarmonyInstance.Create("com.hydroponicharvestor.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                PatchEasyCraft(harmony);

                //VersionChecker.Check<ModConfiguration>("https://github.com/ccgould/FCStudios_SubnauticaMods/raw/master/FCS_HydroponicHarvesters/mod.json");

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
            var dnaIcon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "DNA.png"));
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "LargeHydroponicHarvester.png"));
            var eatableIcon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "eatableIcon.png"));
            var usableIcon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "usableIcon.png"));
            var decorIcon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "BluePalm_FCSDNA.png"));
            var craftingTab = new CraftingTab(Mod.HydroHarvTabID, Mod.ModFriendlyName, icon);
            
            var largeHydroHarv = new FCSKit(Mod.LargeHydroHarvKitClassID, Mod.LargeFriendlyName, craftingTab, Mod.LargeHydroHarvIngredients);
            largeHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var mediumHydroHarv = new FCSKit(Mod.MediumHydroHarvKitClassID, Mod.MediumFriendlyName, craftingTab, Mod.MediumHydroHarvIngredients);
            mediumHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var smallHydroHarv = new FCSKit(Mod.SmallHydroHarvKitClassID, Mod.SmallFriendlyName, craftingTab, Mod.SmallHydroHarvIngredients);
            smallHydroHarv.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            FcTechFabricatorService.PublicAPI.AddTabNode("DNASample", "DNA Samples", dnaIcon);
            FcTechFabricatorService.PublicAPI.AddTabNode("FCS_EatableDNA", "Eatable DNA Samples", eatableIcon, "DNASample");
            FcTechFabricatorService.PublicAPI.AddTabNode("FCS_UsableDNA", "Usable DNA Samples", usableIcon, "DNASample");
            FcTechFabricatorService.PublicAPI.AddTabNode("FCS_DecorDNA", "Decor DNA Samples", decorIcon, "DNASample");

            foreach (var dnaSample in Mod.EatableDNASamples)
            {
                var dna = new FCSDNASample(dnaSample.ClassID, dnaSample.Friendly, dnaSample.Description, dnaSample.Ingredient, dnaSample.Amount);
                dna.ChangeIconLocation(Mod.GetAssetFolder());
                dna.Patch();
                FcTechFabricatorService.PublicAPI.AddCraftNode(dna, "FCS_EatableDNA");
            }

            foreach (var dnaSample in Mod.UsableDNASamples)
            {
                var dna = new FCSDNASample(dnaSample.ClassID, dnaSample.Friendly, dnaSample.Description, dnaSample.Ingredient, dnaSample.Amount);
                dna.ChangeIconLocation(Mod.GetAssetFolder());
                dna.Patch();
                FcTechFabricatorService.PublicAPI.AddCraftNode(dna, "FCS_UsableDNA");
            }

            foreach (var dnaSample in Mod.DecorSamples)
            {
                var dna = new FCSDNASample(dnaSample.ClassID, dnaSample.Friendly, dnaSample.Description, dnaSample.Ingredient, dnaSample.Amount);
                dna.ChangeIconLocation(Mod.GetAssetFolder());
                dna.Patch();
                FcTechFabricatorService.PublicAPI.AddCraftNode(dna, "FCS_DecorDNA");
            }

            if (HydroponicHarvestersModelPrefab.GetPrefabs())
            {
                var hydroHarvesterLarge = new HydroponicHarvestersBuildable(Mod.LargeClassID, Mod.LargeFriendlyName, Mod.LargeDescription,
                    new Vector3(2.13536f, 2.379217f, 2.341017f), new Vector3(0f, 1.556781f, 0f), Mod.LargeHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.LargePrefab, Mod.LargeBubblesLocations);
                hydroHarvesterLarge.Patch();

                QuickLogger.Debug("Patched Large");

                var hydroHarvesterMedium = new HydroponicHarvestersBuildable(Mod.MediumClassID, Mod.MediumFriendlyName, Mod.MediumDescription,
                    new Vector3(1.654228f, 2.46076f, 2.274961f), new Vector3(-0.02562737f, 1.505608f, 0.02242398f), Mod.MediumHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.MediumPrefab, Mod.MediumBubblesLocations);
                hydroHarvesterMedium.Patch();

                QuickLogger.Debug("Patched Medium");

                var hydroHarvesterSmall = new HydroponicHarvestersBuildable(Mod.SmallClassID, Mod.SmallFriendlyName, Mod.SmallDescription,
                    new Vector3(1.648565f, 2.492922f, 1.784077f), new Vector3(-0.01223725f, 1.492922f, 0.1544394f), Mod.SmallHydroHarvKitClassID.ToTechType(), HydroponicHarvestersModelPrefab.SmallPrefab, Mod.SmallBubblesLocations);
                hydroHarvesterSmall.Patch();

                FloraKleen = new FloraKleenPatcher(Mod.FloraKleenClassID, Mod.FloraKleenFriendlyName, Mod.FloraKleenDescription, craftingTab);
                FloraKleen.Register();
                FloraKleen.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

                QuickLogger.Debug("Patched Small");
            }
            else
            {
                throw new PatchTerminatedException("Failed to get the prefabs from the asset bundle");
            }
        }

        private static void PatchEasyCraft(HarmonyInstance harmony)
        {
            var isEasyCraftInstalled = QModServices.Main.ModPresent("EasyCraft");

            if (isEasyCraftInstalled)
            {
                QuickLogger.Debug("EasyCraft is installed");

                var easyCraftClosestItemContainersType = Type.GetType("EasyCraft.ClosestItemContainers, EasyCraft");
                var easyCraftMainType = Type.GetType("EasyCraft.Main, EasyCraft");
                var easyCraftSettingsType = Type.GetType("EasyCraft.Settings, EasyCraft");

                if (easyCraftMainType != null)
                {
                    QuickLogger.Debug("Got EasyCraft Main Type");
                    EasyCraftSettingsInstance = easyCraftMainType
                        .GetField("settings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    if (EasyCraftSettingsInstance != null)
                    {
                        QuickLogger.Debug("Got EasyCraft Settings Field Info");
                        if (easyCraftSettingsType != null)
                        {
                            QuickLogger.Debug("Got EasyCraft Settings type");

                            QuickLogger.Debug($"Got EasyCraft Settings type: {easyCraftSettingsType.Name}");
                            var autoCraft = easyCraftSettingsType.GetField("autoCraft").GetValue(EasyCraftSettingsInstance);
                            UseStorage = easyCraftSettingsType.GetField("useStorage");
                            var returnSurplus = easyCraftSettingsType.GetField("returnSurplus")
                                .GetValue(EasyCraftSettingsInstance);
                        }
                    }
                }


                if (easyCraftClosestItemContainersType != null)
                {
                    QuickLogger.Debug("Got EasyCraft Type");
                    var destroyItemMethodInfo = easyCraftClosestItemContainersType.GetMethod("DestroyItem");
                    var getPickupCountMethodInfo = easyCraftClosestItemContainersType.GetMethod("GetPickupCount");

                    if (destroyItemMethodInfo != null)
                    {
                        QuickLogger.Debug("Got EasyCraft DestroyItem Method");
                        var postfix = typeof(EasyCraft_Patch).GetMethod("DestroyItem");
                        harmony.Patch(destroyItemMethodInfo, null, new HarmonyMethod(postfix));
                    }

                    if (getPickupCountMethodInfo != null)
                    {
                        QuickLogger.Debug("Got EasyCraft GetPickupCount Method");
                        var postfix = typeof(EasyCraft_Patch).GetMethod("GetPickupCount");
                        harmony.Patch(getPickupCountMethodInfo, null, new HarmonyMethod(postfix));
                    }
                }
                else
                {
                    QuickLogger.Error("Failed to get EasyCraft Type");
                }
            }
            else
            {
                QuickLogger.Debug("EasyCraft  not installed");
            }
        }

        internal static FloraKleenPatcher FloraKleen { get; set; }
        internal static object EasyCraftSettingsInstance { get; set; }
        internal static FieldInfo UseStorage { get; set; }
    }
}
