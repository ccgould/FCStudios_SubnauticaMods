using System;
using System.IO;

namespace ARS_SeaBreezeFCS32.Configuration
{
    internal static class Mod
    {
        #region Internal Properties
        internal static string ModName => "FCS_ARSSeaBreeze";
        internal static string BundleName => "arsseabreezefcs32modbundle";

        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string FriendlyName => "ARS Sea Breeze FCS32";
        internal static string Description => "Alterra Refrigeration Sea Breeze will keep your items fresh longer!";
        internal static string ClassID => "ARSSeaBreezeFCS32";

        #endregion

        #region Internal Methods
        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "mod.json");
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
