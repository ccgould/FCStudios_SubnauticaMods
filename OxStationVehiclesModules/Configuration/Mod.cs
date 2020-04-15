using System;
using System.IO;
using SMLHelper.V2.Crafting;

namespace OxStationVehiclesModules.Configuration
{
    internal class Mod
    {
        internal const string ModFolderName = "FCS_OxStationVehiclesModules";

        internal static TechData Ingredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Kyanite, 8)
            }
        };

        public static TechType ModuleTechType { get; set; }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }
    }
}
