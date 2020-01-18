using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using QuantumTeleporter.Mono;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace QuantumTeleporter.Configuration
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _saveData;

        #endregion

        #region Internal Properties
        internal const string ModName = "Quantum Teleporter";
        internal const string ModFolderName = "FCS_QuantumTeleporter";
        internal const string BundleName = "quantumteleportermodbundle";
        internal const string FriendlyName = "Quantum Teleporter";
        internal const string Description = "A teleporter that allows you to teleport from one base to another";
        internal const string ClassID = "QuantumTeleporter";
        internal const string SaveDataFilename = "QuantumTeleporterSaveData.json";
        internal const string QuantumTeleporterTabID = "QT";

        internal const string QuantumTeleporterKitClassID = "QuantumTeleporterKit_AE";
        internal static string QuantumTeleporterKitText => $"{ModName} Kit";

        internal const string AdvancedTeleporterWiringKitClassID = "AdvancedTeleporterWiringKit_AE";
        internal const string AdvancedTeleporterWiringKitText = "Advanced Teleporter Wiring";

        internal const string TeleporterScannerConnectionKitClassID = "TeleporterScannerConnectionKit_AE";
        internal const string TeleporterScannerConnectionKitText = "Teleporter Scanner Connection";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static TechData QuantumTeleporterKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(AdvancedTeleporterWiringKitClassID.ToTechType(), 1),
                new Ingredient(TeleporterScannerConnectionKitClassID.ToTechType(), 1)
            }
        };
        internal static TechData TeleporterScannerConnectionKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Kyanite, 3),
                new Ingredient(TechType.MapRoomHUDChip, 2),
                new Ingredient(TechType.Compass, 1),
                new Ingredient(TechType.TitaniumIngot, 2)
            }
        };
        internal static TechData AdvancedTeleporterWiringKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Aerogel, 2),
                new Ingredient(TechType.Diamond, 2),
                new Ingredient(TechType.FiberMesh, 2)
            }
        };


        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<QuantumTeleporterController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
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

        internal static SaveDataEntry GetSaveData(string id)
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

            return new SaveDataEntry() { ID = id };
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
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
        internal static string GetAssetFolder()
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

        private static void CreateModConfiguration()
        {
            var config = new ModConfiguration();

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Mod.ConfigurationFile().Trim(), saveDataJson);
        }

        private static ModConfiguration LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile().Trim());
        }

        internal static ModConfiguration LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }

    }
}
