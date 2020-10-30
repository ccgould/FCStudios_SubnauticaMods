using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.AlterraGen.Enumerators;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Crafting;
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

        internal const string JetStreamT242TabID = "MT";
        internal const string JetStreamT242Description = "The Jet Stream T242 provides power by using the water current.The faster the turbine spins the more power output";
        internal const string JetStreamT242FriendlyName = "JetStreamT242";
        internal const string JetStreamT242ModName = "JetStreamT242";
        internal const string JetStreamT242ClassName = "AIJetStreamT242";
        internal static string JetStreamT242KitClassID => $"{AlterraGenModName}_Kit";
        internal static string JetStreamT242PrefabName => JetStreamT242ModName;

#if SUBNAUTICA
        internal static TechData AlterraGenIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AlterraGenIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(AlterraGenKitClassID.ToTechType(), 1),
            }
        };

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

                var controllers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IFCSSave<SaveData>>();

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

        internal static AlterraGenDataEntry GetAlterraGenSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraGenEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new AlterraGenDataEntry() { ID = id };
        }

        internal static JetStreamT242DataEntry GetJetStreamT242SaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MarineTurbineEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new JetStreamT242DataEntry() { ID = id };
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
