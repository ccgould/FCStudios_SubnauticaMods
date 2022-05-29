using System;
using System.IO;
using SMLHelper.V2.Utility;

namespace FCSTechFabricator.Configuration
{
    internal static class Mod
    {
        internal static string ModName => "FCSTechFabricator";
        internal static string ModClassID => "FCSTechFabricator";

        internal static string ModFriendly => "FCS Tech Fabricator";

        internal static string ModFolderName => "FCS_TechFabricator";

        internal static string MODFOLDERLOCATION => GetModPath();

        internal static string LANGUAGEDIRECTORY => GetLanguagePath();
        internal static string ModDescription => "The place for all your FCStudios mod needs";
        internal static string AssetBundleName => "fcstechfabricatormodbundle";

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
