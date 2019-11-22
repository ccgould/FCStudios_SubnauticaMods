using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AMMiniMedBay.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AMMiniMedBay.Configuration
{
    internal static class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;

        internal static string ClassID => FCSTechFabricator.Configuration.MiniMedBayClassID;
        internal static string ModName => "MiniMedBay";
        internal static string ModFriendlyName => "Alterra Medical MiniMedBay";
        internal static string ModDescription => "Alterra is here with all your medical needs.";
        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string GameObjectName => FCSTechFabricator.Configuration.MiniMedBayClassID; // Same name as the class
        internal static string ModFolderName => $"FCS_{ModName}";
        internal static string BundleName => "amminimedbaymodbundle";

        #region Deep Driller
        public static void SaveMod()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var drills = GameObject.FindObjectsOfType<AMMiniMedBayController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        public static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        public static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        public static SaveDataEntry GetSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.Entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new SaveDataEntry() { Id = id };
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

        public static Action<SaveData> OnDataLoaded { get; private set; }

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
