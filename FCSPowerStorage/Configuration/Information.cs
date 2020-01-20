using SMLHelper.V2.Utility;
using System;
using System.IO;
using SMLHelper.V2.Crafting;

namespace FCSPowerStorage.Configuration
{
    internal static class Information
    {
        internal const string ModName = "FCSPowerStorage";
        internal const string ModFolderName  = "FCS_PowerStorage";
        internal const string ModFriendlyName = "FCS Power Storage";
        internal const string ModDescription = "This is a wall mounted battery storage for base backup power.";
        internal const string PrefrabName = "Power_Storage";
        internal const string PowerStorageTabID = "PS";
        internal const string PowerStorageKitClassID = "PowerStorageKit_PS";
        internal const string PowerStorageKitFriendlyName = "Power Storage Kit";


        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string LANGUAGEDIRECTORY => GetLanguagePath();
#if SUBNAUTICA
        internal static TechData PowerStorageIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Battery, 1),
                new Ingredient(TechType.AcidMushroom, 6),
                new Ingredient(TechType.Titanium, 7),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Quartz, 1),
                new Ingredient(TechType.Salt, 6),

            }
        };
#elif BELOWZERO
        internal static RecipeData PowerStorageIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Battery, 1),
                new Ingredient(TechType.AcidMushroom, 6),
                new Ingredient(TechType.Titanium, 7),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Quartz, 1),
                new Ingredient(TechType.Salt, 6),

            }
        };
#endif

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModInfoPath()
        {
            return Path.Combine(GetModPath(), "mod.json");
        }

        private static string GetLanguagePath()
        {
            return Path.Combine(GetModPath(), "Language");

        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }
    }
}
