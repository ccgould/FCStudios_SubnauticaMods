using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Extensions;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Configuration
{
    internal static class Mod
    {
        private static int seabreezeCount;

        #region Internal Properties
        internal const string ModName = "FCS_ARSSeaBreeze";
        internal const string BundleName = "arsseabreezefcs32modbundle";
        internal const string SeaBreezeTabID = "SB";
        internal const string FriendlyName = "ARS Sea Breeze FCS32";
        internal const string Description = "Alterra Refrigeration Sea Breeze will keep your items fresh longer!";
        internal const string ClassID = "ARSSeaBreezeFCS32";
        internal const string ModFolderName = ModName;
        internal static string ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal const string SeaBreezeKitClassID = "SeaBreezeKit_SB";

        internal static string MODFOLDERLOCATION => GetModPath();

#if SUBNAUTICA
        internal static TechData SeaBreezeIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.PowerCell, 1),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Glass, 1),
                new Ingredient(Mod.SeaBreezeKitClassID.ToTechType(), 1)
            }
        };
#elif BELOWZERO
        internal static RecipeData SeaBreezeIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.PowerCell, 1),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Glass, 1),
                new Ingredient(Mod.SeaBreezeKitClassID.ToTechType(), 1)
            }
        };

#endif



        #endregion

        #region Internal Methods
        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }

        internal static string GetNewSeabreezeName()
        {
            QuickLogger.Debug($"Get Seabreeze New Name");
            return $"{FriendlyName} {seabreezeCount++}";
        }
        #endregion

        #region Private Methods
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

        #endregion
    }
}
