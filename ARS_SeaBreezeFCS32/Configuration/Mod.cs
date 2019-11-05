using System;
using System.Collections.Generic;
using System.IO;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Configuration
{
    internal static class Mod
    {
        private static int seabreezeCount;

        #region Internal Properties
        internal static string ModName => "FCS_ARSSeaBreeze";
        internal static string BundleName => "arsseabreezefcs32modbundle";

        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string FriendlyName => "ARS Sea Breeze FCS32";
        internal static string Description => "Alterra Refrigeration Sea Breeze will keep your items fresh longer!";
        internal static string ClassID => FCSTechFabricator.Configuration.SeaBreezeClassID;

        #endregion

        #region Internal Methods
        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "mod.json");
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
            return Path.Combine(GetQModsPath(), ModName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        #endregion
    }
}
