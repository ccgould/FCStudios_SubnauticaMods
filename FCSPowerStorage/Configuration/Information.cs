using SMLHelper.V2.Utility;
using System;
using System.IO;

namespace FCSPowerStorage.Configuration
{
    public static class Information
    {
        /// <summary>
        /// The mod name "ClassID" of the FCS Power Storage
        /// </summary>
        public static string ModName => FCSTechFabricator.Configuration.PowerStorageClassID;
        public static string ModFolderName  => "FCS_PowerStorage";

        public static string MODFOLDERLOCATION => GetModPath();

        public static string LANGUAGEDIRECTORY => GetLanguagePath();

        /// <summary>
        /// The definition of the FCS Power Storage
        /// </summary>
        public static string ModDescription = "This is a wall mounted battery storage for base backup power.";

        public const string PrefrabName = "Power_Storage";
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
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
