using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Spawnables;
using FCS_HomeSolutions.Structs;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HomeSolutions.Configuration
{
    internal static class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static List<PaintToolController> _registeredPaintTool;
        private static HashSet<QuantumPowerBankController> _registeredQuantumPowerBank = new();

        internal const string ModPackID = "FCSHomeSolutions";
        internal const string ModFriendlyName = "Home Solutions";
        internal const string ModBundleName = "fcshomesolutionsbundle";

        internal const string DecorationItemTabId = "DI";

        internal const string EmptyObservationTankClassID = "EmptyObservationTank";
        internal const string EmptyObservationTankFriendly = "Observation Tank";

        internal const string EmptyObservationTankDescription =
            "A tank with something to observe. A perfect object for Tables, Cabinets, and Shelves of all kinds.";

        internal const string EmptyObservationTankPrefabName = "ObservationTank";
        public static string EmptyObservationTankKitClassID = $"{EmptyObservationTankClassID}_kit";

        public static Dictionary<string, SoundEntry> AudioClips = new();

        internal static Dictionary<TechType, TechType> CuredCreatureList =
            new Dictionary<TechType, TechType>(TechTypeExtensions.sTechTypeComparer)
            {
#if SUBNAUTICA
                {
                    TechType.Bladderfish,
                    TechType.CuredBladderfish
                },
                {
                    TechType.Boomerang,
                    TechType.CuredBoomerang
                },
                {
                    TechType.LavaBoomerang,
                    TechType.CuredLavaBoomerang
                },
                {
                    TechType.Eyeye,
                    TechType.CuredEyeye
                },
                {
                    TechType.LavaEyeye,
                    TechType.CuredLavaEyeye
                },
                {
                    TechType.GarryFish,
                    TechType.CuredGarryFish
                },
                {
                    TechType.HoleFish,
                    TechType.CuredHoleFish
                },
                {
                    TechType.Hoopfish,
                    TechType.CuredHoopfish
                },
                {
                    TechType.Hoverfish,
                    TechType.CuredHoverfish
                },
                {
                    TechType.Oculus,
                    TechType.CuredOculus
                },
                {
                    TechType.Peeper,
                    TechType.CuredPeeper
                },
                {
                    TechType.Reginald,
                    TechType.CuredReginald
                },
                {
                    TechType.Spadefish,
                    TechType.CuredSpadefish
                },
                {
                    TechType.Spinefish,
                    TechType.CuredSpinefish
                }
#else
                {
                    TechType.ArcticPeeper,
                    TechType.CuredArcticPeeper
                },
                {
                    TechType.ArrowRay,
                    TechType.CuredArrowRay
                },
                {
                    TechType.Bladderfish,
                    TechType.CuredBladderfish
                },
                {
                    TechType.Boomerang,
                    TechType.CuredBoomerang
                },
                {
                    TechType.DiscusFish,
                    TechType.CuredDiscusFish
                },
                {
                    TechType.FeatherFish,
                    TechType.CuredFeatherFish
                },
                {
                    TechType.FeatherFishRed,
                    TechType.CuredFeatherFishRed
                },
                {
                    TechType.Hoopfish,
                    TechType.CuredHoopfish
                },
                {
                    TechType.NootFish,
                    TechType.CuredNootFish
                },
                {
                    TechType.Spinefish,
                    TechType.CuredSpinefish
                },
                {
                    TechType.SpinnerFish,
                    TechType.CuredSpinnerfish
                },
                {
                    TechType.Symbiote,
                    TechType.CuredSymbiote
                },
                {
                    TechType.Triops,
                    TechType.CuredTriops
                }

#endif
            };

#if SUBNAUTICA
        internal static TechData EmptyObservationTankIngredients => new TechData
#elif BELOWZERO
        internal static RecipeData EmptyObservationTankIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(EmptyObservationTankKitClassID.ToTechType(), 1)
            }
        };


#if SUBNAUTICA
        internal static TechData MiniFountainFilterKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Bleach, 2),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#elif BELOWZERO
        internal static RecipeData MiniFountainFilterKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
#if SUBNAUTICA
                new Ingredient(TechType.Bleach, 2),
#else
                new Ingredient(TechType.Salt, 2),
#endif
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#endif

#if SUBNAUTICA
        internal static TechData CurtainIngredients => new TechData
#elif BELOWZERO
        internal static RecipeData CurtainIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("Curtain_Kit".ToTechType(), 1),
            }
        };

        internal static Action<SaveData> OnDataLoaded { get; set; }

        internal static string SaveDataFilename => $"{ModPackID}SaveData.json";

        public static TechType CurtainTechType { get; internal set; }
        public static Dictionary<string, string> PrefabClassIDS { get; set; } = new Dictionary<string, string>();


        public static Dictionary<TechType, decimal> PeeperBarFoods = new Dictionary<TechType, decimal>();

        internal static Dictionary<TechType, TechType> CustomFoods = new Dictionary<TechType, TechType>();

        private static void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                QuickLogger.Debug(
                    $"Craftable: {craftNode.id} | {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

                if (string.IsNullOrWhiteSpace(craftNode.id)) continue;
                if (craftNode.techType0 != TechType.None)
                {
                    CustomFoods.Add(craftNode.techType0, craftNode.techType0);
                }

                if (craftNode.childCount > 0)
                {
                    GetCraftTreeData(craftNode);
                }
            }
        }

        internal static void GetFoodCustomTrees()
        {
            if (CustomFoods == null)
            {
                CustomFoods = new Dictionary<TechType, TechType>();
            }

            if (CustomFoods.Any()) return;

            var smlCTPatcher = typeof(CraftTreeHandler).Assembly.GetType("SMLHelper.V2.Patchers.CraftTreePatcher");
            var customTrees = (Dictionary<CraftTree.Type, ModCraftTreeRoot>) smlCTPatcher
                .GetField("CustomTrees", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            foreach (KeyValuePair<CraftTree.Type, ModCraftTreeRoot> entry in customTrees)
            {
                QuickLogger.Debug($"Crafting Tree: {entry.Key}");
                if (QPatch.Configuration.AlienChiefCustomFoodTrees.Contains(entry.Key.ToString()))
                {
                    GetCraftTreeData(entry.Value.CraftNode);
                }
            }
        }


        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetAudioFolderPath()
        {
            return Path.Combine(GetAssetPath(), "Audio");
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModPackID);
        }

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                if (_registeredPaintTool != null)
                {
                    foreach (PaintToolController controller in _registeredPaintTool)
                    {
                        controller.Save(newSaveData, serializer);
                    }
                }

                if (_registeredQuantumPowerBank != null)
                {
                    foreach (QuantumPowerBankController controller in _registeredQuantumPowerBank)
                    {
                        controller.Save(newSaveData, serializer);
                    }
                }

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    QuickLogger.Debug(
                        $"Attempting to save device: {controller.Value?.UnitID} | Package Valid: {controller.Value?.PackageId == ModPackID}");

                    if (controller.Value != null && controller.Value.PackageId == ModPackID)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>) controller.Value).Save(newSaveData, serializer);
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

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }

            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static DecorationDataEntry GetDecorationDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DecorationEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DecorationDataEntry {Id = id};
        }

        internal static PaintToolDataEntry GetPaintToolDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PaintToolEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PaintToolDataEntry() {Id = id};
        }

        internal static AlienChiefDataEntry GetAlienChiefSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlienChiefDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlienChiefDataEntry {Id = id};
        }

        internal static StoveDataEntry GetStoveSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.StoveDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new StoveDataEntry {Id = id};
        }

        internal static void RegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                QuickLogger.Debug($"Registered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (!_registeredPaintTool.Contains(paintTool))
            {
                _registeredPaintTool.Add(paintTool);
            }
        }

        internal static void UnRegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (_registeredPaintTool.Contains(paintTool))
            {
                QuickLogger.Debug($"UnRegistered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool.Remove(paintTool);
            }
        }

        internal static bool IsModPatched(string mod)
        {
            return FCSAlterraHubService.PublicAPI.IsModPatched(mod);
        }

        internal static PlanterDataEntry GetPlanterDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PlanterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PlanterDataEntry {Id = id};
        }

        internal static MiniFountainFilterDataEntry GetMiniFountainFilterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MiniFountainFilterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new MiniFountainFilterDataEntry {Id = id};
        }

        internal static SeaBreezeDataEntry GetSeabreezeSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.SeaBreezeDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new SeaBreezeDataEntry {Id = id};
        }

        internal static TrashRecyclerDataEntry GetTrashRecyclerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.TrashRecyclerEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new TrashRecyclerDataEntry() {Id = id};
        }

        internal static QuantumTeleporterDataEntry GetQuantumTeleporterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.QuantumTeleporterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new QuantumTeleporterDataEntry() {Id = id};
        }

        internal static CurtainDataEntry GetCurtainDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.CurtainEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new CurtainDataEntry() {Id = id};
        }

        internal static BaseOperatorDataEntry GetBaseOperatorDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.BaseOperatorEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new BaseOperatorDataEntry() {Id = id};
        }

        public static CabinetDataEntry GetCabinetSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.CabinetDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new CabinetDataEntry() {Id = id};
        }

        public static ObservationTankDataEntry GetObservationTankDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ObservationTankDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ObservationTankDataEntry() {Id = id};
        }

        public static LedLightDataEntry GetLedLightEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.LedLightDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new LedLightDataEntry() {Id = id};
        }

        public static FEXRDataEntry FEXREntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.FEXRDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new FEXRDataEntry() {Id = id};
        }

        public static SignDataEntry GetSignDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.SignDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new SignDataEntry() {Id = id};
        }

        public static ShowerDataEntry GetShowerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ShowerEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ShowerDataEntry() {Id = id};
        }

        public static PeeperLoungeBarEntry GetPeeperLoungeBarSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PeeperLoungeBarEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PeeperLoungeBarEntry() {Id = id};
        }

        public static TrashReceptacleDataEntry GetTrashReceptacleSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.TrashReceptacleEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new TrashReceptacleDataEntry() {Id = id};
        }

        public static ElevatorDataEntry GetElevatorDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ElevatorDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ElevatorDataEntry() {Id = id};
        }

        public static HologramDataEntry GetHologramDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.HologramDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new HologramDataEntry() {Id = id};
        }

        //public static T GetDataEntrySaveData<T>(string id,List<T> entries) where T : new()
        //{
        //    LoadData();

        //    Type type = typeof(T);

        //    foreach (var entry in entries)
        //    {
        //        foreach (PropertyInfo property in type.GetProperties())
        //        {
        //            if (property.Name == "Id")
        //            {
        //                if (type.GetProperty(property.Name)?.GetValue(entry) == id)
        //                {
        //                    return (T)Convert.ChangeType(entry, typeof(T));
        //                }
        //            }
        //        }
        //    }

        //    return new T();
        //}
        public static JukeBoxDataEntry JukeBoxEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.JukeBoxDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new JukeBoxDataEntry() {Id = id};
        }

        public static StairsDataEntry GetStairsEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.StairsEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new StairsDataEntry() {Id = id};
        }

        public static DisplayBoardDataEntry DisplayBoardEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DisplayBoardDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DisplayBoardDataEntry() {Id = id};
        }

        public static QuantumPowerBankDataEntry GetQuantumPowerBankEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.QuantumPowerBankEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new QuantumPowerBankDataEntry() {Id = id};
        }

        public static void RegisterQuantumPowerBank(QuantumPowerBankController quantumPowerBankController)
        {
            _registeredQuantumPowerBank.Add(quantumPowerBankController);
        }

        public static void UnRegisterQuantumPowerBank(QuantumPowerBankController quantumPowerBankController)
        {
            _registeredQuantumPowerBank.Remove(quantumPowerBankController);
        }

        public static QuantumPowerBankChargerDataEntry GetQuantumPowerBankChargerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.QuantumPowerBankChargerDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new QuantumPowerBankChargerDataEntry() {Id = id};
        }

        public static QuantumTeleporterVehiclePadDataEntry GetQuantumTeleporterVehiclePadSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.QuantumTeleporterVehiclePadDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new QuantumTeleporterVehiclePadDataEntry() {Id = id};
        }
    }
}