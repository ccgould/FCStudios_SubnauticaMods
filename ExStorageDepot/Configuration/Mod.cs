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
        internal static readonly string ModName = FCSTechFabricator.Configuration.ExStorageClassID;
        internal static readonly string ClassID = FCSTechFabricator.Configuration.ExStorageClassID;
        internal static readonly string ModFolderName = $"FCS_{ModName}";
        internal const string ModFriendly = "Ex-Storage Depot";
        internal const string BundleName = "exstoragedepotunitmodbundle";
        internal const string ModDesc = "Alterra Storage Solutions Ex-Storage Depot allows you to store a large amount of items outside your base.";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string GameObjectName => "Ex-StorageDepotUnit";

        public static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
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

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
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
