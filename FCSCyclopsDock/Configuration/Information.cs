using FCSCyclopsDock.Utilities;
using System;
using System.IO;

namespace FCSCyclopsDock.Configuration
{
    public static class Information
    {

        /// <summary>
        /// THis is the description of the prefab
        /// </summary>
        public static string ModDescription { get; } = "A Dock that allows you so safely stop the cyclops without causing base damage.";

        /// <summary>
        /// The is the friendly name for the prefab
        /// </summary>
        public static string ModFriendly { get; } = "FCS Cyclops Dock";

        /// <summary>
        /// This is the asset folder location of the mod
        /// </summary>
        public static string ASSETSFOLDER { get; set; }

        /// <summary>
        /// The location of the QMods directory
        /// </summary>
        public static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");


        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModName { get; } = "FCSCyclopsDock";

        /// <summary>
        /// Name of the mod bundle
        /// </summary>
        public static string ModBundleName { get; } = "fcscyclopsdockbundle";

        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModBundleRoot { get; } = "FCSCyclopsDock";

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

        private static string GetConfigPath()
        {
            return Path.Combine(GetModPath(), "Configurations");
        }

        private static string GetLanguagePath()
        {
            return Path.Combine(GetModPath(), "Language");

        }

        public static string GetConfigFile(string modName)
        {
            return Path.Combine(GetConfigPath(), $"{modName}.json"); ;
        }

        public static string GetSaveFileDirectory()
        {
            return FilesHelper.GetSaveFolderPath();
        }
    }
}
