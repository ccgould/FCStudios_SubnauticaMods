using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Objects;
using FCSCommon.Utilities;
using FCSDemo.Model;
using Model;
using Mono;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Json;
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
        internal static string ModName => QPatch.Configuration.ModName;
        internal const string BundleName = "fcsdemo";
        internal const string FriendlyName = "FCS Demo";
        internal const string Description = "A demo mod for FCStudios";
        internal const string ClassID = "FcsDemo";
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
    }

    internal class Config : ConfigFile
    {
        public bool AddPlants { get; set; } = false;
        public bool HasAquarium { get; set; } = false;
        public float PowerUsage { get; set; } = 0.1f;
        public float PlaceMaxDistance { get; set; } = 5f;
        public float PlaceMinDistance { get; set; } = 1.2f;
        public float PlaceDefaultDistance { get; set; } = 2f;
        public bool AllowedOutside { get; set; } = false;
        public bool AllowedInBase { get; set; } = true;
        public bool AllowedOnGround { get; set; } = true;
        public bool AllowedOnWall { get; set; } = false;
        public bool RotationEnabled { get; set; } = true;
        public bool AllowedOnCeiling { get; set; } = false;
        public bool AllowedInSub { get; set; } = false;
        public bool AllowedOnConstructables { get; set; } = false;
        public bool UseCustomBoundingBox { get; set; } = false;
        public Vec3 BoundingCenter { get; set; } = new Vec3(0f, 0, 0f);
        public Vec3 BoundingSize { get; set; } = new Vec3(0, 0, 0);
        public string ModName { get; set; } = "FCSDemo";
        public IEnumerable<ModEntry> Prefabs { get; set; }
        public bool ControlEmissionStrength { get; set; } = false;
        public float EmissionStrength { get; set; } = 5f;
    }
}
