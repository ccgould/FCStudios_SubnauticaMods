using MAC.FireExtinguisherHolder.Mono;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.FireExtinguisherHolder.Config
{
    /// <summary>
    /// A class the handles all mod customization, standard information and loading.
    /// </summary>
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static FEHolderSaveData _fEHolderSaveData;
        #endregion

        #region Internal Properties
        internal static string ModName => "FEHolder";
        internal static string ModFolderName => $"FCS_{ModName}";
        internal static string BundleName => "feholdermodbundle";

        internal const string SaveDataFilename = "FEHolderSaveData.json";

        internal static string MODFOLDERLOCATION => GetModPath();

        internal static event Action<FEHolderSaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                FEHolderSaveData newSaveData = new FEHolderSaveData();

                var drills = GameObject.FindObjectsOfType<FEHolderController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _fEHolderSaveData = newSaveData;

                ModUtils.Save<FEHolderSaveData>(_fEHolderSaveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
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

        internal static FEHolderSaveDataEntry GetSaveData(string id)
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

            return new FEHolderSaveDataEntry() { ID = id };
        }

        internal static FEHolderSaveData GetSaveData()
        {
            return _fEHolderSaveData ?? new FEHolderSaveData();
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<FEHolderSaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _fEHolderSaveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_fEHolderSaveData);
            });
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "mod.json");
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

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }
        #endregion
    }
}
