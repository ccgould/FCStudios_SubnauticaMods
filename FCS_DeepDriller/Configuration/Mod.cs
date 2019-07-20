using FCS_DeepDriller.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal static class Mod
    {
        internal static string ModName => "FCS_DeepDriller";
        internal static string ModFriendlyName => "FCS Deep Driller";
        internal static string ModDecription => "Let's dig down to the deep down deep dark!";
        internal const string SaveDataFilename = "DeepDrillerSaveData.json";
        internal static string DeepDrillerGameObjectName { get; } = "DeepDriller";

        private static ModSaver _saveObject;

        private static DeepDrillerSaveData _deepDrillerSaveData;

        internal static event Action<DeepDrillerSaveData> OnDeepDrillerDataLoaded;

        #region Deep Driller
        public static void SaveDeepDriller()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                DeepDrillerSaveData newSaveData = new DeepDrillerSaveData();

                var drills = GameObject.FindObjectsOfType<FCSDeepDrillerController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _deepDrillerSaveData = newSaveData;

                ModUtils.Save<DeepDrillerSaveData>(_deepDrillerSaveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        public static void LoadDeepDrillerData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<DeepDrillerSaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _deepDrillerSaveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDeepDrillerDataLoaded?.Invoke(_deepDrillerSaveData);
            });
        }

        public static DeepDrillerSaveData GetDeepDrillerSaveData()
        {
            return _deepDrillerSaveData ?? new DeepDrillerSaveData();
        }

        public static DeepDrillerSaveDataEntry GetDeepDrillerSaveData(string id)
        {
            var saveData = GetDeepDrillerSaveData();
            foreach (var entry in saveData.Entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }
            return new DeepDrillerSaveDataEntry() { Id = id };
        }
        #endregion

        public static void OnSaveComplete()
        {
            _saveObject.StartCoroutine(SaveCoroutine());
        }

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        public static bool IsSaving()
        {
            return _saveObject != null;
        }

        /// <summary>
        /// The location of the QMods directory
        /// </summary>
        public static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");

        public static string MODFOLDERLOCATION => GetModPath();
        public static string BundleName => "fcsdeepdrillermodbundle";

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModName);
        }

        public static string GetAssetPath()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModInfoPath()
        {
            return Path.Combine(GetModPath(), "mod.json");
        }

        public static string GetGlobalBundle()
        {
            return Path.Combine(Path.Combine(QMODFOLDER, "FCSTechWorkBench"), "globalmaterials");
        }

        private static string GetConfigPath()
        {
            return Path.Combine(GetModPath(), "Configurations");
        }

        private static string GetLanguagePath()
        {
            return Path.Combine(GetModPath(), "Language");

        }

        public static string GetConfigFile(string modName)
        {
            return Path.Combine(GetConfigPath(), $"{modName}.json"); ;
        }

        public static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }
    }
}
