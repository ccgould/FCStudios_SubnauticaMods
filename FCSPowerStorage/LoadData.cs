using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using Harmony;
using Oculus.Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace FCSPowerStorage
{
    public class LoadData : MonoBehaviour
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        public static BatteryConfiguration BatteryConfiguration { get; set; }

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>
        public static void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSPowerCell");

            // == Load Config == //
            string configJson = File.ReadAllText(Path.Combine(Information.MODFOLDERLOCATION, "config.json").Trim());

            //LoadData
            BatteryConfiguration = JsonConvert.DeserializeObject<BatteryConfiguration>(configJson);

            QuickLogger.Debug(BatteryConfiguration.ValidateData().ToString());
        }

        //private static IEnumerable<string> GetPowerStorageIds(CustomBatteryController[] turbines)
        //{
        //    foreach (CustomBatteryController customBatteryController in turbines)
        //    {
        //        yield return customBatteryController.PrefabId;
        //    }
        //}

        private static string[] GetSaveFiles(string modName)
        {
            return Directory.GetFiles(Information.GetSaveFileDirectory(), "*.json");
        }

        //public static void CleanOldSaveData()
        //{
        //    try
        //    {
        //        var powerStorage = FindObjectsOfType<CustomBatteryController>();

        //        var powerStorageIDs = GetPowerStorageIds(powerStorage);

        //        QuickLogger.Debug($"powerStorageIDs Count: {powerStorageIDs.Count()}");

        //        var savesFolderFiles = GetSaveFiles(Information.ModName).ToList();

        //        QuickLogger.Debug($"savesFolderFiles Count: {savesFolderFiles.Count()}");

        //        savesFolderFiles.RemoveAll(c => powerStorageIDs.ToList().Exists(n => c.Contains(n)));


        //        foreach (var file in savesFolderFiles)
        //        {
        //            File.Delete(file);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}
    }
}
