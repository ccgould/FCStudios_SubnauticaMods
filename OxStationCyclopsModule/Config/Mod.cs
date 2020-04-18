using System;
using System.IO;
using SMLHelper.V2.Crafting;

namespace OxStationCyclopsModule.Config
{
    internal class Mod
    {
        internal const string ModFolderName = "FCS_OxStationCyclopsModule";

#if SUBNAUTICA
        internal static TechData OxstationCyclopsIngredients => new TechData
        {
#elif BELOWZERO
        internal static RecipeData OxstationCyclopsIngredients => new RecipeData
        {
#endif
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.PlasteelIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.HighCapacityTank, 1),
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
