using System;
using System.IO;

namespace FCSTechWorkBench.Configuration
{
    public static class Information
    {
        public static string AIDeepDrillerBatteryName => "AIDeepDrillerBattery";

        public static string AIDeepDrillerBatteryFriendly => "Deep Driller Battery";

        public static string AIDeepDrillerSolarName => "AIDeepDrillerSolar";

        public static string AIDeepDrillerSolarFriendly => "Deep Driller Solar";


        /// <summary>
        /// The mod name "ClassID" of the FCS Power Storage
        /// </summary>
        public static string ModName => "FCSTechWorkBench";

        /// <summary>
        /// The friendly name of the prefab
        /// </summary>
        public static string ModFriendly => "FCS Tech WorkBench";

        /// <summary>
        /// The name of the asset bundle file
        /// </summary>
        public static string ModBundleName => "fcstechworkbenchbundle";


        /// <summary>
        /// The definition of the FCS Power Storage
        /// </summary>
        //public static string PowerStorageDef => "This is a wall mounted battery storage for base backup power.";

        /// <summary>
        /// The assets folder
        /// </summary>
        public static string ASSETSFOLDER => GetAssetFolder();

        public static string GetAssetFolder()
        {
            return Path.Combine(Information.ModName,"Assets");
        }

        public static string MODFOLDERLOCATION => GetModPath();

        public static string LANGUAGEDIRECTORY => GetLanguagePath();

        public static string ModBundleRoot { get; set; } = "FCSTechWorkBench";


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
    }
}
