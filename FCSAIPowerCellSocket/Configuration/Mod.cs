using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCSAIPowerCellSocket.Model;
using FCSAIPowerCellSocket.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAIPowerCellSocket.Configuration
{
    internal static class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;

        internal static string ClassID => FCSTechFabricator.Configuration.PowerCellSocketClassID;
        internal static string ModName => "PowerCellSocket";
        internal static string ModFriendlyName => "Alterra Industrial Powercell Socket";
        internal static string ModDescription => "Alterra Industrial wall mounted powercell socket for emergency power";
        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string GameObjectName => FCSTechFabricator.Configuration.PowerCellSocketClassID; // Same name as the class
        internal static string ModFolderName => $"FCS_{ModName}";
        internal static string BundleName => "aipowercellsocketbundle";

        #region Save

        internal static void SaveMod()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var drills = GameObject.FindObjectsOfType<AIPowerCellSocketController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
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

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static SaveDataEntry GetSaveData(string id)
        {
            try
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
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
            return new SaveDataEntry() { Id = id };
        }
        #endregion

        internal static void OnSaveComplete()
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

        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        /// <summary>
        /// The location of the QMods directory
        /// </summary>
        internal static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");

        internal static string MODFOLDERLOCATION => GetModPath();

        internal static Action<SaveData> OnDataLoaded { get; private set; }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModInfoPath()
        {
            return Path.Combine(GetModPath(), "mod.json");
        }

        internal static string GetGlobalBundle()
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

        internal static string GetConfigFile(string modName)
        {
            return Path.Combine(GetConfigPath(), $"{modName}.json"); ;
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }
    }
}
