using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
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
        private static List<DNASampleData> _hydroponicKnownTech;
        private static TechType _sandBagTechType;
        private static TechType _alterraStorageTechType;

        #endregion

        internal static LootDistributionData LootDistributionData { get; set; }
        internal static string ModPackID => "FCSProductionSolutions";
        internal static string SaveDataFilename => $"FCSProductionSolutionsSaveData.json";
        internal const string ModBundleName = "fcsproductionsolutionsbundle";

        internal const string HydroponicHarvesterModTabID = "HH";
        internal const string HydroponicHarvesterModFriendlyName = "Hydroponic Harvester";
        internal const string HydroponicHarvesterModName = "HydroponicHarvester";
        public const string HydroponicHarvesterModDescription = "3 chambers for growing DNA Samples scanned by the Matter Analyzer. Suitable for interior and exterior use.";
        internal static string HydroponicHarvesterKitClassID => $"{HydroponicHarvesterModName}_Kit";
        internal static string HydroponicHarvesterModClassName => HydroponicHarvesterModName;
        internal static string HydroponicHarvesterModPrefabName => HydroponicHarvesterModName;

        internal const string MatterAnalyzerTabID = "MA";
        internal const string MatterAnalyzerFriendlyName = "Matter Analyzer";
        internal const string MatterAnalyzerModName = "MatterAnalyzer";
        public const string MatterAnalyzerDescription = "Builds detailed scans of seeds and non-living material for duplication in the Hydroponic Harvester and Replicator";
        internal static string MatterAnalyzerKitClassID => $"{MatterAnalyzerModName}_Kit";
        internal static string MatterAnalyzerClassName => MatterAnalyzerModName;
        internal static string MatterAnalyzerPrefabName => MatterAnalyzerModName;

        internal const string ReplicatorTabID = "RM";
        internal const string ReplicatorFriendlyName = "Replicator";
        internal const string ReplicatorModName = "Replicator";
        public const string ReplicatorDescription = "Duplicates non-living material scanned by the Matter Analyzer.";
        internal static string ReplicatorKitClassID => $"{ReplicatorModName}_Kit";
        internal static string ReplicatorClassName => ReplicatorModName;
        internal static string ReplicatorPrefabName => "Replica-Fabricator";


        internal const string DeepDrillerMk3TabID = "DD";
        internal const string DeepDrillerMk3FriendlyName = "Deep Driller MK3";
        internal const string DeepDrillerMk3ModName = "DeepDrillerMK3";
        internal static string DeepDrillerMk3KitClassID => $"{DeepDrillerMk3ClassName}_Kit";
        internal const string DeepDrillerMk3ClassName = "DeepDrillerMk3";
        internal const string DeepDrillerMk3PrefabName = "DeepDrillerMK3";
        internal const string DeepDrillerMk3Description = "Automated drill platform suitable for all biomes with integrated solar and thermal generators. Requires lubricant.";
        internal const string SandSpawnableClassID = "Sand_DD";

        internal const string DSSAutoCrafterTabID = "ACU";
        internal const string DSSAutoCrafterFriendlyName = "Auto Crafter";
        internal const string DSSAutoCrafterClassName = "AutoCrafter";
        internal const string DSSAutoCrafterPrefabName = "AutoCraftMachine";
        internal static string DSSAutoCrafterKitClassID => $"{DSSAutoCrafterClassName}_Kit";
        internal const string DSSAutoCrafterDescription = "Avoid long hours in front of the Fabricator. Queue up a list of multiple items or just keep yourself automatically stocked on an important one.";


#if SUBNAUTICA
        internal static TechData DSSAutoCrafterIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSAutoCrafterIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSAutoCrafterKitClassID.ToTechType(), 1),
            }
        };



        internal static TechType GetSandBagTechType()
        {
            if (_sandBagTechType == TechType.None)
            {
                _sandBagTechType = SandSpawnableClassID.ToTechType();
            }

            return _sandBagTechType;
        }


#if SUBNAUTICA
        internal static TechData HydroponicHarvesterIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData HydroponicHarvesterIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(HydroponicHarvesterKitClassID.ToTechType(), 1),
            }
        };

        public static List<TechType> Craftables { get; set; } = new List<TechType>();

        internal const string ModDescription = "";

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
                    if (controller.Value.PackageId == ModPackID)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
                }

                newSaveData.HydroponicHarvesterKnownTech = _hydroponicKnownTech;

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
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
                _hydroponicKnownTech = _saveData.HydroponicHarvesterKnownTech;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static List<DNASampleData> GetHydroponicKnownTech()
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

        internal static DSSAutoCrafterDataEntry GetDSSAutoCrafterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSAutoCrafterDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSAutoCrafterDataEntry() { ID = id };
        }

        internal static HashSet<TechType> IsNonePlantableAllowedList = new HashSet<TechType>{
            TechType.StalkerTooth,
            TechType.GasPod,
            TechType.JeweledDiskPiece,
            TechType.Floater,
            TechType.TreeMushroomPiece,
            TechType.CoralChunk,
            TechType.CrashPowder,
        };
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

        public static void AddHydroponicKnownTech(DNASampleData data)
        {
            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<DNASampleData>();
            }
            if(IsHydroponicKnownTech(data.TechType, out var sampleData)) return;
            _hydroponicKnownTech.Add(data);
            BaseManager.GlobalNotifyByID(HydroponicHarvesterModTabID,"UpdateDNA");
            BaseManager.GlobalNotifyByID(ReplicatorTabID,"UpdateDNA");
            BaseManager.GlobalNotifyByID(MatterAnalyzerTabID,"UpdateDNA");
        }

        public static List<DNASampleData> GetKnownDNA()
        {
            return _hydroponicKnownTech;
        }

        public static bool IsHydroponicKnownTech(TechType techType, out DNASampleData data)
        {
            QuickLogger.Debug($"Checking if {techType} is known tech",true);
            data = new DNASampleData();
            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<DNASampleData>();
            }

            foreach (DNASampleData sampleData in _hydroponicKnownTech)
            {
                if (sampleData.TechType == techType || sampleData.PickType == techType)
                {
                    data = sampleData;
                    QuickLogger.Debug($"{techType} is known tech", true);

                    return true;
                }
            }
            QuickLogger.Debug($"{techType} is not known tech", true);
            return false;
        }

        public static TechType AlterraStorageTechType()
        {
            if (_alterraStorageTechType == TechType.None)
            {
                _alterraStorageTechType = "AlterraStorage".ToTechType();
            }

            return _alterraStorageTechType;
        }

        public static bool CreateNewDNASampleData(TechType techType,out DNASampleData data)
        {
            data = new DNASampleData();

            if (WorldHelpers.KnownPickTypesContains(techType))
            {
                if (IsNonePlantableAllowedList.Contains(techType))
                {
                    data = new DNASampleData { PickType = techType, TechType = techType };
                    return true;
                }

                var result = WorldHelpers.GetPickTypeData(techType);
                data = new DNASampleData { PickType = result.ReturnType, TechType = techType, IsLandPlant = result.IsLandPlant };
                return true;
            }
            return false;
        }
    }

    internal struct DNASampleData
    {
        public TechType TechType { get; set; }
        public TechType PickType { get; set; }
        public bool IsLandPlant { get; set; }
    }
}