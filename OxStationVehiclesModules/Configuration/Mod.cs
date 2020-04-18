using System;
using System.IO;
using SMLHelper.V2.Crafting;

namespace OxStationVehiclesModules.Configuration
{
    internal class Mod
    {
        internal const string ModFolderName = "FCS_OxStationVehiclesModules";

#if SUBNAUTICA
        internal static TechData OxstationSeamothPrawnIngredients => new TechData
        {
#elif BELOWZERO
        internal static RecipeData OxstationSeamothPrawnIngredients => new RecipeData
        {
#endif
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Tank, 1),
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
