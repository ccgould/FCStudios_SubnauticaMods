using FCSPowerStorage.Helpers;
using System;
using System.IO;

namespace FCSPowerStorage.Configuration
{
    public static class Information
    {
        /// <summary>
        /// The mod name "ClassID" of the FCS Power Storage
        /// </summary>
        public static string ModName => "FCSPowerStorage";

        /// <summary>
        /// The definition of the FCS Power Storage
        /// </summary>
        //public static string PowerStorageDef => "This is a wall mounted battery storage for base backup power.";

        /// <summary>
        /// The assets folder
        /// </summary>
        public static string ASSETSFOLDER { get; set; }

        public static string MODFOLDERLOCATION => GetModPath();

        public static string LANGUAGEDIRECTORY => GetLanguagePath();

        public const string PrefrabName = "Power_Storage";
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
            return Path.Combine(GetModPath(), "Assests");
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
            return FilesHelper.GetSaveFolderPath();
        }
    }
}
