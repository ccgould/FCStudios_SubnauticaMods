using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Model;
using Mono;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSDemo.Configuration
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

# endregion

        #region Internal Properties
        internal const string ModName = "FCSDemo";
        internal const string BundleName = "fcsdemo";
        internal const string FriendlyName = "FCS Demo";
        internal const string Description = "A demo mod for FCStudios";
        internal const string ClassID = "FcsDemo";
        internal const string PrefabName = "DemoModel";
        internal static string AssetFolder => Path.Combine(ModName, "Assets");

        internal const string FCSDemoKitClassID = "FCSDemo_Kit";

        internal static string SaveDataFilename => $"{ClassID}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();

#if SUBNAUTICA
        internal static TechData FCSDemoIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData FCSDemoIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
                    {
                        new Ingredient(TechType.Titanium, 1),
                    }
        };

        internal static bool ProtectPlayer { get; set; }


        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods

        internal static void Save()
        {


            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<FCSDemoController>();

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
        [JsonProperty] internal float PowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float PlaceMaxDistance { get; set; } = 5f;
        [JsonProperty] internal float PlaceMinDistance { get; set; } = 1.2f;
        [JsonProperty] internal float PlaceDefaultDistance { get; set; } = 2f;
        [JsonProperty] internal bool AllowedOutside { get; set; } = false;
        [JsonProperty] internal bool AllowedInBase { get; set; } = true;
        [JsonProperty] internal bool AllowedOnGround { get; set; } = true;
        [JsonProperty] internal bool AllowedOnWall { get; set; } = false;
        [JsonProperty] internal bool RotationEnabled { get; set; } = true;
        [JsonProperty] internal bool AllowedOnCeiling { get; set; } = false;
        [JsonProperty] internal bool AllowedInSub { get; set; } = false;
        [JsonProperty] internal bool AllowedOnConstructables { get; set; } = false;
        [JsonProperty] internal bool UseCustomBoundingBox { get; set; } = false;
        [JsonProperty] internal Vec3 BoundingCenter { get; set; } = new Vec3(0f, 0, 0f);
        [JsonProperty] internal Vec3 BoundingSize { get; set; } = new Vec3(0, 0, 0);
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
