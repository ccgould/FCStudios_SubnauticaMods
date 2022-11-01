using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.Mods.IonCubeGenerator.Interfaces;
using FCS_ProductionSolutions.Structs;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static List<FCSDNASampleData> _hydroponicKnownTech;
        private static TechType _sandBagTechType;

        #endregion

        internal static LootDistributionData LootDistributionData { get; set; }
        internal static string ModPackID => "FCSProductionSolutions";
        internal static string SaveDataFilename => $"FCSProductionSolutionsSaveData.json";
        internal const string ModBundleName = "fcsproductionsolutionsbundle";
        
        internal const string MatterAnalyzerTabID = "MA";
        internal const string MatterAnalyzerFriendlyName = "Matter Analyzer";
        internal const string MatterAnalyzerModName = "MatterAnalyzer";
        public const string MatterAnalyzerDescription = "Builds detailed scans of seeds and non-living material for duplication in the Hydroponic Harvester and Replicator";
        internal static string MatterAnalyzerKitClassID => $"{MatterAnalyzerModName}_Kit";
        internal static string MatterAnalyzerClassName => MatterAnalyzerModName;
        internal static string MatterAnalyzerPrefabName => MatterAnalyzerModName;
        
        internal const string SandSpawnableClassID = "Sand_DD";

        internal static TechType GetSandBagTechType()
        {
            if (_sandBagTechType == TechType.None)
            {
                _sandBagTechType = SandSpawnableClassID.ToTechType();
            }

            return _sandBagTechType;
        }

        public static List<TechType> Craftables { get; set; } = new();

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                QuickLogger.Debug($"=================== Saving {ModPackID} ===================");
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();
                newSaveData.CraftingOperations = CraftingOperations;
                newSaveData.HydroponicHarvesterKnownTech = _hydroponicKnownTech;
                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    if (controller.Value.PackageId == ModPackID)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
                }
                
                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
                QuickLogger.Debug($"=================== Saved {ModPackID} ===================");
            }
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                CraftingOperations = _saveData.CraftingOperations;
                _hydroponicKnownTech = _saveData.HydroponicHarvesterKnownTech;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static List<FCSDNASampleData> GetHydroponicKnownTech()
        {
            return _hydroponicKnownTech;
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
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModPackID);
        }

        internal static HydroponicHarvesterDataEntry GetHydroponicHarvesterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.HydroponicHarvesterEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new HydroponicHarvesterDataEntry() { ID = id };
        }

        internal static DeepDrillerSaveDataEntry  GetDeepDrillerMK2SaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DeepDrillerMk2Entries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DeepDrillerSaveDataEntry() { Id = id };
        }

        internal static MatterAnalyzerDataEntry GetMatterAnalyzerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MatterAnalyzerEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new MatterAnalyzerDataEntry() { ID = id };
        }

        internal static ReplicatorDataEntry GetReplicatorSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ReplicatorEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new ReplicatorDataEntry() { ID = id };
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

        internal static AutoCrafterDataEntry GetDSSAutoCrafterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AutoCrafterDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new AutoCrafterDataEntry() { ID = id };
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
            PurgeData();
            _saveObject = null;
        }

        #endregion

        public static void AddHydroponicKnownTech(FCSDNASampleData data)
        {
            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<FCSDNASampleData>();
            }
            if(IsHydroponicKnownTech(data.TechType, out var sampleData)) return;
            _hydroponicKnownTech.Add(data);
            BaseManager.GlobalNotifyByID(HydroponicHarvesterPatch.HydroponicHarvesterModTabID,"UpdateDNA");
            BaseManager.GlobalNotifyByID(ReplicatorBuildable.ReplicatorTabID, "UpdateDNA");
            BaseManager.GlobalNotifyByID(MatterAnalyzerTabID,"UpdateDNA");
        }

        public static List<FCSDNASampleData> GetKnownDNA()
        {
            return _hydroponicKnownTech;
        }

        public static bool IsHydroponicKnownTech(TechType techType, out FCSDNASampleData data)
        {
            QuickLogger.Debug($"Checking if {techType} is known tech",true);
            data = new FCSDNASampleData();

            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<FCSDNASampleData>();
            }

            foreach (FCSDNASampleData sampleData in _hydroponicKnownTech)
            {
                if (sampleData.IsSame(techType))
                {
                    data = sampleData;
                    QuickLogger.Debug($"{techType} is known tech", true);

                    return true;
                }
            }
            QuickLogger.Debug($"{techType} is not known tech", true);
            return false;
        }

        public static bool CreateNewDNASampleData(TechType techType,out FCSDNASampleData data)
        {
            data = new FCSDNASampleData();

            if (WorldHelpers.KnownPickTypesContains(techType))
            {
                if (WorldHelpers.IsNonePlantableAllowedList.Contains(techType))
                {
                    data = new FCSDNASampleData { PickType = techType, TechType = techType };
                    return true;
                }

                var result = WorldHelpers.GetPickTypeData(techType);
                data = new FCSDNASampleData { PickType = result.ReturnType, TechType = techType, IsLandPlant = result.IsLandPlant };
                return true;
            }
            return false;
        }

        public static void Purge()
        {
            //Delayed purging because harvester data was being purged before save in hardcore mode
            QuickLogger.Debug("Pending Purging Production Data", true);
            _isPurging = true;
        }

        private static void PurgeData()
        {
            if (!_isPurging) return;
            QuickLogger.Debug("Purging Production Data", true);
            _saveData = null;
            _hydroponicKnownTech = new List<FCSDNASampleData>();
            _isPurging = false;
        }

        public static void RemoveCraftingOperation(string unitID)
        {
            CraftingOperations.Remove(unitID);
        }

        public static bool AddCraftingOperation(CraftingOperation operation)
        {
            if(CraftingOperations.ContainsKey(operation.ParentMachineUnitID)) return false;
            CraftingOperations.Add(operation.ParentMachineUnitID, operation);
            return true;
        }

        public static CraftingOperation FindCraftingOperation(string unitID)
        {
            return !CraftingOperations.ContainsKey(unitID) ? null : CraftingOperations[unitID];
        }

        private static Dictionary<string, CraftingOperation> CraftingOperations = new();
        private static bool _isPurging;

        public static DeepDrillerLightDutySaveDataEntry GetDeepDrillerLightDutySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DeepDrillerLightDutyEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DeepDrillerLightDutySaveDataEntry() { Id = id };
        }

        public static ICubeGeneratorSaveHandler GetIonCubeGeneratorSaveData(string id)
        {
            LoadData();
            foreach (IonCubeGeneratorSaveDataEntry ionCubeGeneratorSaveDataEntry in Mod.GetSaveData().IonCubeGeneratorEntries)
            {
                if (!string.IsNullOrEmpty(ionCubeGeneratorSaveDataEntry.Id) && ionCubeGeneratorSaveDataEntry.Id == id)
                {
                    return ionCubeGeneratorSaveDataEntry;
                }
            }

            return new IonCubeGeneratorSaveDataEntry
            {
                Id = id
            };
        }
    }
}