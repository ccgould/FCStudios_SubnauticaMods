using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        #endregion

        internal static string ModName => "FCSLifeSupportSolutions";
        internal static string SaveDataFilename => $"FCSLifeSupportSolutionsSaveData.json";
        internal const string ModBundleName = "fcslifesupportsolutionsbundle";

        internal const string EnergyPillVendingMachineTabID = "EPV";
        internal const string EnergyPillVendingMachineFriendlyName = "Energy Pill Vending Machine";
        internal const string EnergyPillVendingMachineName = "EnergyPillVendingMachine";
        internal const string EnergyPillVendingMachineDescription = "Vending Machine for Alterra Energy Pills. Get the performance boost you need when you’re extremely hungry or thirsty.";
        internal static string EnergyPillVendingMachineKitClassID => $"{EnergyPillVendingMachineName}_Kit";
        internal static string EnergyPillVendingMachineClassName => EnergyPillVendingMachineName;
        internal static string EnergyPillVendingMachinePrefabName => "EnergyPillVendingMachine";

        internal const string MiniMedBayTabID = "MMB";
        internal const string MiniMedBayFriendlyName = "Mini Med Bay";
        internal const string MiniMedBayName = "MiniMedBay";
        internal const string MiniMedBayDescription = "Autonomous Medical Bay for rapid diagnosis and healing. This model contains an integrated 6-slot Medical Kit Fabricator.";
        internal static string MiniMedBayKitClassID => $"{MiniMedBayClassName}_Kit";
        internal static string MiniMedBayClassName => MiniMedBayName;
        internal static string MiniMedBayPrefabName => MiniMedBayName;

        internal const string BaseUtilityUnitTabID = "BUU";
        internal const string BaseUtilityUnitFriendlyName = "Base Utility Unit";
        internal const string BaseUtilityUnitName = "BaseUtilityUnit";
        internal const string BaseUtilityUnitDescription = "Supplies Oxygen and pressurized Water to your entire base. With pressurized water, the Mini Fountain Filter can operate at any height above the water line.";
        internal static string BaseUtilityUnityKitClassID => $"{BaseUtilityUnitName}_Kit";
        internal static string BaseUtilityUnityClassName => BaseUtilityUnitName;
        internal static string BaseUtilityUnityPrefabName => BaseUtilityUnitName;

        internal const string BaseOxygenTankClassID = "BaseOxygenTank";
        internal const string BaseOxygenTankFriendly = "Base Oxygen Tank";
        internal const string BaseOxygenTankDescription = "Provides pressurized oxygen when connected to surface air. Designed for small habitats; larger habitats may require more units.";
        internal const string BaseOxygenTankPrefabName = "oxTank";
        internal const string BaseOxygenTankTabID = "OXT";


#if SUBNAUTICA
        internal static TechData EnergyPillVendingMachineIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData EnergyPillVendingMachineIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(EnergyPillVendingMachineKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData MiniMedBayIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData MiniMedBayIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(MiniMedBayKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData BaseOxygenTankIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData BaseOxygenTankIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.DoubleTank, 1),
                new Ingredient(TechType.Pipe, 1),
                new Ingredient(TechType.CopperWire, 1),
                new Ingredient(TechType.Silicone, 1),
            }
        };

#if SUBNAUTICA
        internal static TechData BaseUtilityUnitIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData BaseUtilityUnitIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(BaseUtilityUnityKitClassID.ToTechType(), 1),
            }
        };

        public static TechType RedEnergyPillTechType { get; set; }
        public static TechType GreenEnergyPillTechType { get; set; }
        public static TechType BlueEnergyPillTechType { get; set; }

        internal const string ModDescription ="";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static void Save(ProtobufSerializer serializer)
        {

            if (!IsSaving())
            { 
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    if (controller.Value.PackageId == ModName)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
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

        internal static MiniMedBayEntry GetMiniMedBaySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MiniMedBayEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new MiniMedBayEntry() { Id = id };
        }

        internal static EnergyPillVendingMachineEntry GetEnergyPillVendingMachineSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.EnergyPillVendingMachineEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new EnergyPillVendingMachineEntry() { Id = id };
        }

        public static BaseUtilityEntry GetBaseUtilityUnitSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.BaseUtilityUnitEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new BaseUtilityEntry() { Id = id };
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

        public static BaseOxygenTankEntry GetOxygenTankSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.BaseOxygenTankEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new BaseOxygenTankEntry() { Id = id };
        }
    }
}
