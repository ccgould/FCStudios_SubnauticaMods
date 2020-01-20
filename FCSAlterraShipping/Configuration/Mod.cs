using System;
using System.Collections;
using System.IO;
using FCSAlterraShipping.Models;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAlterraShipping.Configuration
{
    internal static class Mod
    {
        internal const string ModName = "AlterraShipping";
        internal const string BundleName = "alterrashippingmodbundle";
        internal const string FriendlyName = "Alterra Shipping";
        internal const string Description = "Shipping all your parcels.";
        internal const string ClassID = "FCSAlterraShipping";
        internal const string GameObjectName = "AlterraShippingMod";
        internal const string AlterraShippingTabID = "ASU";
        internal const string AlterraShippingKitClassID = "ASShippingKit_AS";

#if SUBNAUTICA
        internal static TechData AlterraShippingIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.VehicleStorageModule, 1),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.Beacon, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#elif BELOWZERO
        internal static RecipeData AlterraShippingIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.VehicleStorageModule, 1),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.Beacon, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#endif

        internal static string ModDirectoryName => $"FCS_{ModName}";
        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string ModFolderName => $"FCS_{ModName}";

        private static ModSaver _saveObject;

        private static SaveData _saveData;

        //#region New Save System
        //internal static void SaveMod()
        //{
        //    if (!IsSaving())
        //    {
        //        _saveObject = new GameObject().AddComponent<ModSaver>();

        //        SaveData newSaveData = new SaveData();

        //        var drills = GameObject.FindObjectsOfType<Al>();

        //        foreach (var drill in drills)
        //        {
        //            drill.Save(newSaveData);
        //        }

        //        _saveData = newSaveData;

        //        ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
        //    }
        //}

        //internal static void LoadData()
        //{
        //    QuickLogger.Info("Loading Save Data...");
        //    ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
        //    {
        //        _saveData = data;
        //        QuickLogger.Info("Save Data Loaded");
        //        OnDataLoaded?.Invoke(_saveData);
        //    });
        //}

        //internal static SaveData GetSaveData()
        //{
        //    return _saveData ?? new SaveData();
        //}

        //internal static SaveDataEntry GetSaveData(string id)
        //{
        //    LoadData();

        //    var saveData = GetSaveData();

        //    foreach (var entry in saveData.Entries)
        //    {
        //        if (entry.Id == id)
        //        {
        //            return entry;
        //        }
        //    }

        //    return new SaveDataEntry() { Id = id };
        //}
        //#endregion

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

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

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

        internal static string GetModInfoPath()
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

