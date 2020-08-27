using System;
using System.IO;
using SMLHelper.V2.Crafting;

namespace CyclopsUpgradeConsole.Configuration
{
    internal static class Mod
    {
        internal const string BundleName = "cyclopsupgradeconsolebundle";
        internal const string ModTabID = "CUC";
        internal const string ModFriendlyName = "Cyclops Upgrade Console";
        internal const string ModName = "CyclopsUpgradeConsole";
        internal static string CyclopsUpgradeConsoleKitClassID => $"{ModName}_Kit";
        internal static string ModClassName => ModName;
        internal static string ModPrefabName => ModName;
        internal static string ModFolderName => $"FCS_{ModName}";

#if SUBNAUTICA
        internal static TechData CyclopsUpgradeConsoleIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData CyclopsUpgradeConsoleIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Titanium,3),
                new Ingredient(TechType.Lead, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };

        internal const string ModDescription = "A wall mountable upgrade console to connect a greater number of upgrades to your Cyclops.";


        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }
    }
}