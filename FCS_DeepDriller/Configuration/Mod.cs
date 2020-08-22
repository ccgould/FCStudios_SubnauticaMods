using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using FCS_DeepDriller.Mono.MK2;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Options;
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
        internal const string BundleName = "fcsdeepdrillermk2modbundle";

        internal const string DeepDrillerGameObjectName = "DeepDriller";
        internal const string MaterialBaseName = "AlterraDeepDrillerMK2";
        internal const string SandSpawnableClassID = "Sand_DD";
        internal const string DeepDrillerKitClassID = "DeepDrillerKit_DD";
        internal const string DeepDrillerKitFriendlyName = "Deep Driller";

        internal const string DeepDrillerTabID = "DD";
        
        internal static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");
        internal static string MODFOLDERLOCATION => GetModPath();
        
        private static ModSaver _saveObject;

        private static DeepDrillerSaveData _deepDrillerSaveData;

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

        internal static void SaveModConfiguration()
        {
            try
            {
                var config = new DeepDrillerCfg();

                var saveDataJson = JsonConvert.SerializeObject(QPatch.Configuration, Formatting.Indented);

                File.WriteAllText(Path.Combine(MODFOLDERLOCATION, GetConfigPath()), saveDataJson);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}\n{e.StackTrace}");
            }
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

    internal class Options : ModOptions
    {
        private const string ToggleID = "RefreshBTN";
        private const string AllowDamageID = "AllowDamage";
        private bool _allowDamage;

        public Options() : base("Deep Driller Settings")
        {
            ToggleChanged += OnToggleChanged;
            _allowDamage = QPatch.Configuration.AllowDamage;
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            
            switch (e.Id)
            {
                case AllowDamageID:
                    _allowDamage = QPatch.Configuration.AllowDamage = e.Value;
                    break;
            }

            Mod.SaveModConfiguration();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(AllowDamageID, "Damage Overtime", _allowDamage);
        }
    }
}
