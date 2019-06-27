using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Mono;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FCSPowerStorage
{
    internal class LoadData : MonoBehaviour
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        internal static BatteryConfiguration BatteryConfiguration { get; set; }

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>
        public static void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSPowerCell");

            // == Load Configuration == //
            string configJson = File.ReadAllText(Information.ConfigurationFile().Trim());

            //LoadData
            BatteryConfiguration = JsonConvert.DeserializeObject<BatteryConfiguration>(configJson);

            QuickLogger.Debug(BatteryConfiguration.ValidateData().ToString());
        }

        private static IEnumerable<string> GetPowerStorageIds(FCSPowerStorageController[] units)
        {
            foreach (FCSPowerStorageController customBatteryController in units)
            {
                yield return customBatteryController.GetPrefabID();
            }
        }

        private static string[] GetSaveFiles(string modName)
        {
            return Directory.GetFiles(Information.GetSaveFileDirectory(), "*.json");
        }

        public static void CleanOldSaveData()
        {
            try
            {
                var powerStorage = FindObjectsOfType<FCSPowerStorageController>();

                var powerStorageIDs = GetPowerStorageIds(powerStorage).ToList();

                QuickLogger.Debug($"powerStorageIDs Count: {powerStorageIDs.Count}");

                var savesFolderFiles = GetSaveFiles(Information.ModName).ToList();

                QuickLogger.Debug($"savesFolderFiles Count: {savesFolderFiles.Count}");

                savesFolderFiles.RemoveAll(c => powerStorageIDs.ToList().Exists(c.Contains));

                QuickLogger.Debug("Deleting " + savesFolderFiles.Count + "unused saved file " + (savesFolderFiles.Count > 1 ? "s" : string.Empty) + ".");

                foreach (var file in savesFolderFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
