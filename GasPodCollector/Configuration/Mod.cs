using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using GasPodCollector.Mono;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace GasPodCollector.Configuration
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "FCS_GasPodCollector";
        internal const string BundleName = "gaspodcollectormodbundle";
        internal const string GasPodCollectorTabID = "GPC";
        internal const string FriendlyName = "GasPodCollector";
        internal const string Description = "A device that collect Gaspods from the gasopod.";
        internal const string ClassID = "GaspodCollector";
        internal static string AssetFolder => Path.Combine(ModName, "Assets");

        internal const string GaspodCollectorKitClassID = "GaspodCollector_Kit";

        internal static string SaveDataFilename => $"{ClassID}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();

#if SUBNAUTICA
        internal static TechData GaspodCollectorIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData GaspodCollectorIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
                    {
                        new Ingredient(TechType.Beacon, 1),
                        new Ingredient(TechType.AdvancedWiringKit, 1),
                        new Ingredient(TechType.TitaniumIngot, 2),
                        new Ingredient(TechType.EnameledGlass, 1),
                        new Ingredient(TechType.Gravsphere, 1),
                        new Ingredient(TechType.VehicleStorageModule, 1),
                        new Ingredient(TechType.Battery, 2),
                        new Ingredient(TechType.Gasopod, 2),
                        new Ingredient(TechType.StalkerTooth, 5)
                    }
        };



        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<GaspodCollectorController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        internal static void OnSaveComplete()
        {
            _saveObject.StartCoroutine(SaveCoroutine());
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static SaveDataEntry GetSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.Entries)
            {
                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new SaveDataEntry() { ID = id };
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, ConfigFileName);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile());
        }
        #endregion

        #region Private Methods
        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }
        public static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }


        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        #endregion

        private static void CreateModConfiguration()
        {
            var config = new ConfigFile { Config = new Config() };

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigFileName), saveDataJson);
        }

        private static ConfigFile LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<ConfigFile>(configJson, settings);
        }

        internal static ConfigFile LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }
    }

    internal class Config
    {
        [JsonProperty] internal bool PlaySFX { get; set; } = true;
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
