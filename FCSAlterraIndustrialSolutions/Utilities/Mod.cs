using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Utilities
{
    public static class Mod
    {
        public const string SaveDataFilename = "DeepDrillerSaveData.json";
        private static ModSaver _saveObject;
        private static DeepDrillerSaveData _deepDrillerSaveData;
        public static event Action<DeepDrillerSaveData> OnDeepDrillerDataLoaded;

        #region Deep Driller
        public static void SaveDeepDriller()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                DeepDrillerSaveData newSaveData = new DeepDrillerSaveData();

                var drills = GameObject.FindObjectsOfType<DeepDrillerController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _deepDrillerSaveData = newSaveData;

                ModUtils.Save<DeepDrillerSaveData>(_deepDrillerSaveData, SaveDataFilename, Information.GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        public static void LoadDeepDrillerData()
        {
            Log.Info("Loading Save Data...");
            ModUtils.LoadSaveData<DeepDrillerSaveData>(SaveDataFilename, Information.GetSaveFileDirectory(), (data) =>
            {
                _deepDrillerSaveData = data;
                Log.Info("Save Data Loaded");
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
    }
}
