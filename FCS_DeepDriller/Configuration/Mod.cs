using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using FCS_DeepDriller.Mono.MK1;
using FCSCommon.Extensions;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal static class Mod
    {
        internal static LootDistributionData LootDistributionData { get; set; }

        internal const string ModClassID = "FCS_DeepDriller";
        internal const string ModFriendlyName = "FCS Deep Driller";
        internal const string ModFolderName = "FCS_DeepDriller";
        internal const string ModDecription = "Let's dig down to the deep down deep dark!";
        internal const string SaveDataFilename = "DeepDrillerSaveData.json";
        internal const string BundleName = "fcsdeepdrillermodbundle";

        internal const string DeepDrillerGameObjectName = "DeepDriller";

        internal const string SandSpawnableClassID = "Sand_DD";
        internal const string DeepDrillerKitClassID = "DeepDrillerKit_DD";
        internal const string DeepDrillerKitFriendlyName = "Deep Driller";
        internal const string FocusAttachmentKitClassID = "FocusAttachment_DD";
        internal const string FocusAttachmentFriendlyName = "Focus Attachment";
        internal const string SolarAttachmentKitClassID = "SolarAttachment_DD";
        internal const string SolarAttachmentFriendlyName = "Solar Attachment";
        internal const string BatteryAttachmentKitClassID = "BatteryAttachment_DD";
        internal const string BatteryAttachmentFriendlyName = "Battery Attachment";

        internal const string DrillerMK1ModuleClassID = "DrillerMK1_DD";
        internal const string DrillerMK1ModuleFriendlyName = "Driller MK1";
        internal const string DrillerMK2ModuleClassID = "DrillerMK2_DD";
        internal const string DrillerMK2ModuleFriendlyName = "Driller MK2";
        internal const string DrillerMK3ModuleClassID = "DrillerMK3_DD";
        internal const string DrillerMK3ModuleFriendlyName = "Driller MK3";

        internal const string DeepDrillerTabID = "DD";
        
        internal static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");
        internal static string MODFOLDERLOCATION => GetModPath();
        
        private static ModSaver _saveObject;

        private static DeepDrillerSaveData _deepDrillerSaveData;
        public static string MK1Description => $"This upgrade allows deep driller to drill {QPatch.Configuration.Mk1OrePerDay} resources per day.";
        public static string MK2Description => $"This upgrade allows deep driller to drill {QPatch.Configuration.Mk2OrePerDay} resources per day.";
        public static string MK3Description => $"This upgrade allows deep driller to drill {QPatch.Configuration.Mk3OrePerDay} resources per day.";

        internal static event Action<DeepDrillerSaveData> OnDeepDrillerDataLoaded;

#if SUBNAUTICA
        internal static TechData DeepDrillerKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.MapRoomHUDChip, 1),
                new Ingredient(TechType.Titanium, 2),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.ExosuitDrillArmModule, 1),
                new Ingredient(TechType.Lubricant, 1),
                new Ingredient(TechType.VehicleStorageModule, 1),
            }
        };
        internal static TechData FocusAttachmentKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 3),
                new Ingredient(TechType.WiringKit, 2),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static TechData BatteryAttachmentKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Silicone, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static TechData SolarAttachmentKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static TechData DrillerMK1Ingredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Lithium, 2),
                new Ingredient(TechType.Diamond, 1),
                new Ingredient(TechType.Titanium, 3)
            }
        };
        internal static TechData DrillerMK2Ingredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(Mod.DrillerMK1ModuleClassID.ToTechType(), 1),
                new Ingredient(TechType.Diamond, 4),
                new Ingredient(TechType.AluminumOxide, 1),
                new Ingredient(TechType.Titanium, 4)
            }
        };
        internal static TechData DrillerMK3Ingredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(Mod.DrillerMK2ModuleClassID.ToTechType(), 1),
                new Ingredient(TechType.Diamond, 4),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AluminumOxide, 3),
                new Ingredient(TechType.Magnetite, 3),
                new Ingredient(TechType.EnameledGlass, 2)
            }
        };
#elif BELOWZERO
        internal static RecipeData DeepDrillerKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.MapRoomHUDChip, 1),
                new Ingredient(TechType.Titanium, 2),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.ExosuitDrillArmModule, 1),
                new Ingredient(TechType.Lubricant, 1),
                new Ingredient(TechType.VehicleStorageModule, 1),
            }
        };
        internal static RecipeData FocusAttachmentKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 3),
                new Ingredient(TechType.WiringKit, 2),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static RecipeData BatteryAttachmentKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Silicone, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static RecipeData SolarAttachmentKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };
        internal static RecipeData DrillerMK1Ingredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Lithium, 2),
                new Ingredient(TechType.Diamond, 1),
                new Ingredient(TechType.Titanium, 3)
            }
        };
        internal static RecipeData DrillerMK2Ingredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(Mod.DrillerMK1ModuleClassID.ToTechType(), 1),
                new Ingredient(TechType.Diamond, 4),
                new Ingredient(TechType.AluminumOxide, 1),
                new Ingredient(TechType.Titanium, 4)
            }
        };
        internal static RecipeData DrillerMK3Ingredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(Mod.DrillerMK2ModuleClassID.ToTechType(), 1),
                new Ingredient(TechType.Diamond, 4),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AluminumOxide, 3),
                new Ingredient(TechType.Magnetite, 3),
                new Ingredient(TechType.EnameledGlass, 2)
            }
        };
#endif


        #region Deep Driller
        public static void SaveDeepDriller()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                DeepDrillerSaveData newSaveData = new DeepDrillerSaveData();

                var drills = GameObject.FindObjectsOfType<FCSDeepDrillerController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _deepDrillerSaveData = newSaveData;

                ModUtils.Save<DeepDrillerSaveData>(_deepDrillerSaveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        public static void LoadDeepDrillerData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<DeepDrillerSaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _deepDrillerSaveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDeepDrillerDataLoaded?.Invoke(_deepDrillerSaveData);
            });
        }

        public static DeepDrillerSaveData GetDeepDrillerSaveData()
        {
            return _deepDrillerSaveData ?? new DeepDrillerSaveData();
        }

        public static DeepDrillerSaveDataEntry GetDeepDrillerSaveData(string id)
        {
            LoadDeepDrillerData();

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

        public static bool IsSaving()
        {
            return _saveObject != null;
        }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }
        
        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModInfoPath()
        {
            return Path.Combine(GetModPath(), "mod.json");
        }

        internal static string GetGlobalBundle()
        {
            return Path.Combine(Path.Combine(QMODFOLDER, "FCSTechWorkBench"), "globalmaterials");
        }
        
        private static string GetConfigPath()
        {
            return Path.Combine(GetModPath(), "config.json");
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
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModClassID);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(GetConfigPath());
        }

        private static void CreateModConfiguration()
        {
            var config = new DeepDrillerCfg();

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.Combine(MODFOLDERLOCATION, GetConfigPath()), saveDataJson);
        }

        private static DeepDrillerCfg LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(GetConfigPath().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<DeepDrillerCfg>(configJson, settings);
        }

        internal static DeepDrillerCfg LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }
    }
}
