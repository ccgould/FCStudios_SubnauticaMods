using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AIMarineTurbine.Configuration
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        #endregion

        #region Internal Properties
        internal const string BundleName = "aimarineturbinemodbundle";

        internal const string JetStreamFriendlyName = "AI JetStreamT242";
        internal const string JetStreamDescription = "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.";
        internal const string JetStreamClassID = "AIJetStreamT242";

        internal const string MarineTurbinesFriendlyName = "Marine Turbines";
        internal const string ModFolderName = "FCS_MarineTurbine";

        internal const string MarineMonitorClassID = "AIMarineMonitor";
        internal const string MarineMonitorFriendlyName = "Marine Monitor";
        internal const string MarineMonitorDescriptription = "Why go outside and get wet? Get your turbine status and control your turbine from inside!";

        internal const string MarineTurbinesTabID = "MT";
        internal const string JetstreamKitClassID = "JetStreamT242Kit_MT";
        internal const string MarineMontiorKitClassID = "MarineMonitorKit_MT";

        internal const string SaveDataFilename = "MiniFountainFilterSaveData.json";

        internal static TechData JetstreamKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.CopperWire, 1),
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Lubricant, 2)
            }
        };


        internal static TechData MarineMonitorKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.Battery, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };

        internal static string MODFOLDERLOCATION => GetModPath();
        
        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        //internal static void Save()
        //{
        //    if (!IsSaving())
        //    {
        //        _saveObject = new GameObject().AddComponent<ModSaver>();

        //        SaveData newSaveData = new SaveData();

        //        var controllers = GameObject.FindObjectsOfType<MiniFountainFilterController>();

        //        foreach (var controller in controllers)
        //        {
        //            controller.Save(newSaveData);
        //        }

        //        _saveData = newSaveData;

        //        ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
        //    }
        //}

        //internal static bool IsSaving()
        //{
        //    return _saveObject != null;
        //}

        //internal static void OnSaveComplete()
        //{
        //    _saveObject.StartCoroutine(SaveCoroutine());
        //}

        //internal static string GetSaveFileDirectory()
        //{
        //    return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        //}

        //internal static SaveDataEntry GetSaveData(string id)
        //{
        //    LoadData();

        //    var saveData = GetSaveData();

        //    foreach (var entry in saveData.Entries)
        //    {
        //        if (entry.ID == id)
        //        {
        //            return entry;
        //        }
        //    }

        //    return new SaveDataEntry() { ID = id };
        //}

        //internal static SaveData GetSaveData()
        //{
        //    return _saveData ?? new SaveData();
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

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
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
        public static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }


        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        internal static List<SerializableColor> SerializedColors()
        {
            return JsonConvert.DeserializeObject<List<SerializableColor>>(File.ReadAllText(Path.Combine(GetAssetFolder(), "colors.json")));
        }

        #endregion
    }
}
