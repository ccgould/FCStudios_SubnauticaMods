
using AE.SeaCooker.Mono;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace AE.SeaCooker.Configuration
{
    using SMLHelper.V2.Utility;
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;


    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "FCS_AESeaCooker";
        internal const string BundleName = "seacookermodbundle";
        internal const string SeaCookerTabID = "SC";
        internal const string SaveDataFilename = "SeaCookerSaveData.json";
        internal const string FriendlyName = "SeaCooker";
        internal const string Description = "A automatic food cooker for all your cooking needs";
        internal const string ClassID = "SeaCooker";
        internal const string SeaCookerKitClassID = "SeaCookerBuildableKit_SC";
        internal static string MODFOLDERLOCATION => GetModPath();

#if SUBNAUTICA
        internal static TechData SeaCookerIngredients => new TechData
#elif BELOWZERO
        internal static RecipeData SeaCookerIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Aerogel, 2),
                new Ingredient(TechType.TitaniumIngot, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData SeaAlienGasTankKitIngredients => new TechData
#elif BELOWZERO
        internal static RecipeData SeaAlienGasTankKitIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.SeaTreaderPoop, 2),
                new Ingredient(TechType.Tank, 1),
                new Ingredient(TechType.FilteredWater, 2)
            }
        };

#if SUBNAUTICA
        internal static TechData SeaGasTankKitIngredients => new TechData
#elif BELOWZERO
        internal static RecipeData SeaGasTankKitIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.GasPod, 1),
                new Ingredient(TechType.Tank, 1)
            }
        };

        internal const string SeaAlienGasClassID = "SeaAlienGasTank_SC";
        internal const string SeaAlienGasFriendlyName = "Sea Alien Gas";
        internal const string SeaAlienGasDescription = "This tank allows you to cook food in the Sea Cooker using Alien Feces.";

        internal const string SeaGasClassID = "SeaGasTank_SC";
        internal const string SeaGasFriendlyName = "Sea Gas";
        internal const string SeaGasDescription = "This tank allows you to cook food in the Sea Cooker using Gaspod gas.";

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<SeaCookerController>();

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

        internal static List<SerializableColor> SerializedColors()
        {
            return JsonConvert.DeserializeObject<List<SerializableColor>>(File.ReadAllText(Path.Combine(GetAssetFolder(), "colors.json")));
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
}
