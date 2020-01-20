using AE.MiniFountainFilter.Mono;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace AE.MiniFountainFilter.Configuration
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        #endregion

        #region Internal Properties
        internal const string ModName = "FCS_MiniFountainFilter";
        internal const string BundleName = "minifountainfiltermodbundle";
        internal const string FriendlyName = "Mini Fountain Filter";
        internal const string Description = "A smaller water filtration system for your base or cyclops.";
        internal const string ClassID = "MiniFountainFilter";
        internal const string SaveDataFilename = "MiniFountainFilterSaveData.json";
        internal const string MiniFountainFilterTabID = "MFF";
        internal const string MiniFountainFilterKitClassID = "MiniFountainFilterKit_MFF";


#if SUBNAUTICA
        internal static TechData MiniFountainFilterKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Bleach, 2),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#elif BELOWZERO
        internal static RecipeData MiniFountainFilterKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Bleach, 2),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#endif

        internal static string MODFOLDERLOCATION => GetModPath();

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<MiniFountainFilterController>();

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
            return Path.Combine(MODFOLDERLOCATION, "config.json");
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
    }
}
