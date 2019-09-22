using SMLHelper.V2.Utility;
using System;
using System.IO;

namespace FCSTechFabricator.Configuration
{
    public static class Information
    {
        /// <summary>
        /// The mod name "ClassID" of the FCS Power Storage
        /// </summary>
        public static string ModName => "FCSTechFabricator";

        public static string MODFOLDERLOCATION => GetModPath();

        public static string LANGUAGEDIRECTORY => GetLanguagePath();

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModName);
        }

        public static string GetAssetPath()
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

        public static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        public static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }
    }
}
