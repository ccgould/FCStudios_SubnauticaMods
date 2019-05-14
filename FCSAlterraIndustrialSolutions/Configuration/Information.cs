using FCSAlterraIndustrialSolutions.Utilities;
using System;
using System.IO;

namespace FCSAlterraIndustrialSolutions.Configuration
{
    public static class Information
    {
        /// <summary>
        /// THis is the ClassID of the Server Rack
        /// </summary>
        public static string JetStreamTurbineName => "AIJetStreamT242";

        /// <summary>
        /// This is the description of the prefab
        /// </summary>
        public static string JetStreamTurbineDescription { get; } = "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.";

        /// <summary>
        /// The is the friendly name for the prefab
        /// </summary>
        public static string JetStreamTurbineFriendly { get; set; } = "Jet Stream T242";

        /// <summary>
        /// The is the friendly name for the prefab
        /// </summary>
        public static string JetStreamTurbineGameObjectName { get; set; } = "JetStreamT242";


        /// <summary>
        /// THis is the description of the prefab
        /// </summary>
        public static string ModDescription { get; } = "Alterra Industrial Solutions Devision";

        /// <summary>
        /// The is the friendly name for the prefab
        /// </summary>
        public static string ModFriendly { get;} = "Alterra Industrial Solutions";

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
        public static string ModName { get; } = "FCSAISolutions";

        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModBundleName { get; } = "aisolutionsbundle";

        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModBundleRoot { get; } = "AISolutions";

        public static string MarineTurbinesMonitorName { get; set; } = "AIMarineTurbinesMonitor";
        public static string MarineTurbinesMonitorDescription { get; set; } = "Why go outside and get wet? Get your turbine status and control your turbine from inside!";
        public static string MarineTurbinesMonitorFriendly { get; set; } = "Marine Turbines Monitor";
        public static string MarineTurbinesMonitorGameObjectName { get; set; } = "MarineTurbinesMonitor";

        public static string DeepDrillerName { get;} = "AIDeepDriller";
        public static string DeepDrillerDescription { get;} = "Let's dig down to the deep down deep dark!";
        public static string DeepDrillerFriendly { get; } = "Deep Driller";
        public static string DeepDrillerGameObjectName { get;} = "DeepDriller";


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

        public static string GetGlobalBundle()
        {
            return Path.Combine(Path.Combine(QMODFOLDER, "FCSTechWorkBench"), "globalmaterials");
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
