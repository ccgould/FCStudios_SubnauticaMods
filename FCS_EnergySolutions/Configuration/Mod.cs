using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_EnergySolutions.AlterraGen.Enumerators;
using FCS_EnergySolutions.AlterraGen.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_EnergySolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static Dictionary<TechType, float> _vanillaBioChargeValues;
        #endregion
        
        internal static string ModName => "FCSEnergySolutions";
        internal static string SaveDataFilename => $"FCSEnergySolutionsSaveData.json";
        internal const string ModBundleName = "fcsenergysolutionsbundle";

        internal const string AlterraGenModTabID = "AG";
        internal const string AlterraGenModFriendlyName = "Alterra Gen";
        internal const string AlterraGenModName = "AlterraGen";
        internal static string AlterraGenKitClassID => $"{AlterraGenModName}_Kit";
        internal static string AlterraGenModClassName => AlterraGenModName;
        internal static string AlterraGenModPrefabName => AlterraGenModName;

//#if SUBNAUTICA
//        internal static TechData AlterraGenIngredients => new TechData
//#elif BELOWZERO
//                internal static RecipeData AlterraGenIngredients => new RecipeData
//#endif
//        {
//            craftAmount = 1,
//            Ingredients =
//            {
//                new Ingredient(TechType.Titanium, 1),
//                new Ingredient(TechType.Quartz, 1),
//                new Ingredient(TechType.Lubricant, 1),
//                new Ingredient(TechType.Battery, 1),
//                new Ingredient(TechType.Silicone, 1)
//            }
//        };

        internal const string ModDescription ="";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static Dictionary<TechType,float> GetBioChargeValues()
        {
            if (_vanillaBioChargeValues == null)
            {
                Type baseBioReactorType = typeof(BaseBioReactor);
                _vanillaBioChargeValues = (Dictionary<TechType, float>)AccessTools.Field(baseBioReactorType, "charge").GetValue(baseBioReactorType);
            }

            if (_vanillaBioChargeValues == null)
            {
                QuickLogger.Error("Failed to get vanilla bio charge values using stored values");
            }

            return _vanillaBioChargeValues ?? FuelTypes.Charge;
        }

        internal static void Save()
        {


            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<AlterraGenController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
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
                if (string.IsNullOrEmpty(entry.ID)) continue;

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

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

        #endregion
    }
}
