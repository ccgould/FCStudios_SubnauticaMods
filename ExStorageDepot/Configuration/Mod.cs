using ExStorageDepot.Buildable;
using ExStorageDepot.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ExStorageDepot.Configuration
{
    internal class Mod
    {
        private static ModSaver _saveObject;
        private static ExStorageDepotSaveData _exStorageDepotSaveData;
        internal static event Action<ExStorageDepotSaveData> OnExStorageDepotLoaded;
        internal const string SaveDataFilename = "ExStorageDepotSaveData.json";

        public static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ExStorageDepotBuildable.ModName);
        }

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

        #region ExStorageDepot
        public static void SaveExStorageDepot()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                ExStorageDepotSaveData newSaveData = new ExStorageDepotSaveData();

                var storages = GameObject.FindObjectsOfType<ExStorageDepotController>();

                QuickLogger.Debug($"Storages count {storages.Length}");

                foreach (var storage in storages)
                {
                    storage.Save(newSaveData);
                }

                _exStorageDepotSaveData = newSaveData;

                ModUtils.Save<ExStorageDepotSaveData>(_exStorageDepotSaveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        public static void LoadExStorageDepotData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<ExStorageDepotSaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _exStorageDepotSaveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnExStorageDepotLoaded?.Invoke(_exStorageDepotSaveData);
            });
        }

        public static ExStorageDepotSaveData GetExStorageDepotSaveData()
        {
            return _exStorageDepotSaveData ?? new ExStorageDepotSaveData();
        }

        public static ExStorageDepotSaveDataEntry GetExStorageDepotSaveData(string id)
        {
            LoadExStorageDepotData();

            var saveData = GetExStorageDepotSaveData();

            foreach (var entry in saveData.Entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ExStorageDepotSaveDataEntry() { Id = id };
        }
        #endregion

    }
}
