using FCSCommon.Utilities;
using FCSCommon.Utilities.Language;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Model;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FCSPowerStorage
{
    public class LoadItems : MonoBehaviour
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        public static ModStrings ModStrings { get; set; }

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


            LoadLanguage();

            // == Create Custom Battery == //

            var customBattery = new CustomBattery(Information.ModName, "FCS Power Storage");
            customBattery.RegisterFCSPowerStorage();
            customBattery.Patch();
        }

        private static void LoadLanguage()
        {
            //  == Load the language settings == //
            LanguageSystem.GetCurrentSystemLanguageInfo();
            var currentLang = LanguageSystem.CultureInfo.Name;

            var languages = LanguageSystem.LoadCurrentRegion<ModStrings>(Path.Combine(Information.LANGUAGEDIRECTORY, "languages.json"));

            var _modStrings = languages.Single(x => x.Region.Equals(currentLang));

            if (_modStrings != null)
            {
                ModStrings = _modStrings;
            }
            else
            {
                ModStrings = new ModStrings();
                ModStrings.LoadDefault();
                QuickLogger.Error($"Language {currentLang} not found in the languages.json");
            }
        }

        private static IEnumerable<string> GetTurbineIds(CustomBatteryController[] turbines)
        {
            foreach (CustomBatteryController customBatteryController in turbines)
            {
                yield return customBatteryController.ID;
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
                var powerStorage = FindObjectsOfType<CustomBatteryController>();

                var powerStorageIDs = GetTurbineIds(powerStorage);

                QuickLogger.Debug($"powerStorageIDs Count: {powerStorageIDs.Count()}");

                var savesFolderFiles = GetSaveFiles(Information.ModName).ToList();

                QuickLogger.Debug($"savesFolderFiles Count: {savesFolderFiles.Count()}");

                savesFolderFiles.RemoveAll(c => powerStorageIDs.ToList().Exists(n => c.Contains(n)));


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
