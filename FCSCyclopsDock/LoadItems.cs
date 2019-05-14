using FCSCommon.Exceptions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Language;
using FCSCyclopsDock.Configuration;
using FCSCyclopsDock.Models.Controllers;
using FCSCyclopsDock.Models.Prefabs;
using Harmony;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ModStrings = FCSCyclopsDock.Language.ModStrings;

namespace FCSCyclopsDock
{
    public class LoadItems : MonoBehaviour
    {
        #region Public Properties
        public static AssetBundle ASSETBUNDLE { get; set; }
        public static Cfg Config { get; set; }
        public static ModStrings ModStrings { get; set; }
        public static GameObject CyclopsDockPrefab { get; set; }
        public static CyclopsDock CYCLOPSDOCK_PREFAB_OBJECT { get; set; }
        #endregion

        #region Internal Properties
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;
        #endregion

        #region Public Methods

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>

        public static void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create($"com.FCStudios.{Information.ModName}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            //Load the language
            ModStrings = LoadLanguage<ModStrings>(Information.ModName) as ModStrings;

            bool FCSTechWorkBench = TechTypeHandler.ModdedTechTypeExists("FCSTechWorkBench");

            //if (!FCSTechWorkBench)
            //{
            //    QuickLogger.Error($"Mod FCS WorkBench is needed!");
            //    throw new PatchTerminatedException("Error finding FCS Work Bench");
            //}

            //if (!FindAllowedModules())
            //{
            //    QuickLogger.Error($"Allowed modules not found!");

            //    throw new PatchTerminatedException("Error finding all allowed modules in FCS Work Bench");
            //}

            //Load Configuration
            bool loaded = LoadConfigs();

            if (!loaded)
            {
                throw new PatchTerminatedException("Error loading Configuration");
            }

            // == Get the Prefabs == //
            if (GetPrefabs())
            {
                // === Create the jetStream == //
                if (ModStrings != null)
                {
                    var cyclopsDock = new CyclopsDock(Information.ModName, Information.ModFriendly,
                        ModStrings.Description);
                    cyclopsDock.Register();
                    cyclopsDock.Patch();
                    CYCLOPSDOCK_PREFAB_OBJECT = cyclopsDock;
                }
                else
                {
                    throw new PatchTerminatedException($"Error {nameof(ModStrings)} is null");
                }

                //var file = Path.Combine(Information.LANGUAGEDIRECTORY, $"{Information.MarineTurbinesMonitorName}.json");
                //var file = Information.GetConfigFile(Information.DeepDrillerName);

                //var data = JsonConvert.SerializeObject(new DeepDrillerCfg(), Formatting.Indented);

                //File.WriteAllText(file, data);
            }
            else
            {
                throw new PatchTerminatedException("Error loading finding a prefab");
            }
        }

        private static bool FindAllowedModules()
        {
            var aIDeepDrillerBattery = TechTypeHandler.TryGetModdedTechType("AIDeepDrillerBattery", out TechType aiDeepDrillerBattery);
            if (aIDeepDrillerBattery)
            {
                //DeepDrillerAllowedModules.Add(aiDeepDrillerBattery);
            }
            else
            {
                QuickLogger.Error($"Deep Driller Battery not found!");
                return false;
            }
            return true;
        }

        private static bool LoadConfigs()
        {
            if (File.Exists(Information.GetConfigFile(Information.ModName)))
            {
                string savedDataJson = File.ReadAllText(Information.GetConfigFile(Information.ModName)).Trim();

                //LoadData
                Config = JsonConvert.DeserializeObject<Cfg>(savedDataJson);
            }
            else
            {
                QuickLogger.Error($"{Information.GetConfigFile(Information.ModName)} doesn't exist");
                return false;
            }

            return true;
        }


        public static void CleanOldSaveData()
        {
            try
            {
                var turbines = FindObjectsOfType<CyclopsDockController>();

                var turbineIDs = GetTurbineIds(turbines);

                QuickLogger.Debug($"turbineIDs Count: {turbineIDs.Count()}");

                var savesFolderFiles = GetSaveFiles(Information.ModName).ToList();

                QuickLogger.Debug($"savesFolderFiles Count: {savesFolderFiles.Count()}");

                savesFolderFiles.RemoveAll(c => turbineIDs.ToList().Exists(n => c.Contains(n)));
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

        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the prefabs from the asset bundle
        /// </summary>
        /// <returns></returns>
        private static bool GetPrefabs()
        {
            var assetBundle = AssetHelper.Asset(Information.ModName, Information.ModBundleName);

            if (assetBundle != null)
            {
                QuickLogger.Debug("Loaded AssetBundle");
                ASSETBUNDLE = assetBundle;
            }
            else
            {
                QuickLogger.Error("Failed to load asset bundle");
                return false;
            }

            Shader shader = ASSETBUNDLE.LoadAsset<Shader>("Dissolve");
            
            if (shader != null)
            {
                DissolveShader = shader;
            }
            else
            {
                QuickLogger.Error("Failed to load Dissolve Shader", true);
                return false;
            }

            var prefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.ModBundleRoot);

            if (prefab != null)
            {
                QuickLogger.Debug("Loaded Cyclops Dock Prefab");
                CyclopsDockPrefab = prefab;
            }
            else
            {
                QuickLogger.Error("Failed to load prefab");
                return false;
            }

            return true;
        }

        public static Shader DissolveShader { get; private set; }

        private static string[] GetSaveFiles(string modName)
        {
            return Directory.GetFiles(Information.GetSaveFileDirectory(), "*.json");
        }

        private static IEnumerable<string> GetTurbineIds(CyclopsDockController[] turbines)
        {
            foreach (CyclopsDockController jetStreamT242Controller in turbines)
            {
                yield return jetStreamT242Controller.ID;
            }
        }

        private static FCSCommon.Helpers.ModStrings LoadLanguage<T>(string mod) where T : FCSCommon.Helpers.ModStrings
        {
            //  == Load the language settings == //
            LanguageSystem.GetCurrentSystemLanguageInfo();
            var currentLang = LanguageSystem.CultureInfo.Name;

            QuickLogger.Debug(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));


            var languages = LanguageSystem.LoadCurrentRegion<T>(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));


            if (languages != null)
            {
                QuickLogger.Debug(languages.Count.ToString());

                var modStrings = languages.Single(x => x.Region.Equals(currentLang));

                if (modStrings != null)
                {
                    return modStrings;
                }
            }
            else
            {
                QuickLogger.Error($"Load Current Region returned null");
            }
            return null;
        }
        #endregion

    }
}
