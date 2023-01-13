using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Objects;
using FCSCommon.Utilities;
using FCSDemo.Model;
using FCSDemo.Mono;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
#endif

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
        internal static string ModName => Main.MODNAME;
        internal const string BundleName = "fcsdemo";
        internal const string FriendlyName = "FCS Demo";
        internal const string Description = "A demo mod for FCStudios";
        internal const string ClassID = "FcsDemo";
        internal static string AssetFolder => Path.Combine(ModName, "Assets");

        internal const string FCSDemoKitClassID = "FCSDemo_Kit";

        internal static string SaveDataFilename => $"{ClassID}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();

#if SUBNAUTICA_STABLE
        internal static TechData FCSDemoIngredients => new TechData
#else
                internal static RecipeData FCSDemoIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
                    {
                        new Ingredient(TechType.Titanium, 1),
                    }
        };

        internal static  ConfigEntry<bool> ProtectPlayer { get; set; }


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

        internal static  bool IsSaving()
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

    internal class Config
    {
        public ConfigEntry<bool> AddPlants { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AddPlants), false, "Adds plant");
        public ConfigEntry<bool> HasAquarium { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(HasAquarium), false, "");
        public ConfigEntry<float> PowerUsage { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(PowerUsage), 0.1f, "");
        public ConfigEntry<float> PlaceMaxDistance { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(PlaceMaxDistance), 5f, "");
        public ConfigEntry<float> PlaceMinDistance { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(PlaceMinDistance), 1.2f, "");
        public ConfigEntry<float> PlaceDefaultDistance { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(PlaceDefaultDistance), 2f, "");
        public ConfigEntry<bool> AllowedOutside { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedOutside), false, "");
        public ConfigEntry<bool> AllowedInBase { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedInBase), true, "");
        public  ConfigEntry<bool> AllowedOnGround { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedOnGround), true, "");
        public  ConfigEntry<bool> AllowedOnWall { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedOnWall), false, "");
        public  ConfigEntry<bool> RotationEnabled { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(RotationEnabled), true, "");
        public  ConfigEntry<bool> AllowedOnCeiling { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedOnCeiling), false, "");
        public  ConfigEntry<bool> AllowedInSub { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedInSub), false, "");
        public  ConfigEntry<bool> AllowedOnConstructables { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(AllowedOnConstructables), false, "");
        public  ConfigEntry<bool> UseCustomBoundingBox { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(UseCustomBoundingBox), false, "");
        public ConfigEntry<Vector3> BoundingCenter { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(BoundingCenter), new Vector3(0f, 0, 0f), "");
        public ConfigEntry<Vector3> BoundingSize { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(BoundingSize), new Vector3(0, 0, 0), "");
        public ConfigEntry<string> ModName { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(ModName), "FCSDemo", "");
        public ConfigEntry<List<ModEntry>> Prefabs { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(Prefabs), new List<ModEntry>(), "");
        public  ConfigEntry<bool> ControlEmissionStrength { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(ControlEmissionStrength), false,"");
        public ConfigEntry<float> EmissionStrength { get; } = Main.BepInExConfigFile?.Bind(Mod.ModName, nameof(EmissionStrength), 5f,"");
    }
}
