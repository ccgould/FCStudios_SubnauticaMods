using MAC.OxStation.Mono;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.IO;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Options;
using UnityEngine;

namespace MAC.OxStation.Config
{
    internal static class Mod
    {
        #region Private Members
        private static ModSaver _saveObject;
        private static SaveData _fEHolderSaveData;
        private static bool _isrefrillableTanksInstalled;
        private static bool _techtypeCheck;
        #endregion

        #region Internal Properties

        internal static Action<OxStationController> OnOxstationBuilt;
        internal static Action<OxStationController> OnOxstationDestroyed;
        internal static string ModName => "OxStation";
        internal static string ModScreenName => "OxStationScreen";
        internal static string ModFolderName => "FCS_OxStation";
        internal static string BundleName => "oxstationmodbundle";
        internal const string SaveDataFilename = "OxStationSaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string FriendlyName => "OxStation";
        internal static string ScreenFriendlyName => "OxStation Monitor";
        internal static string Description => "A oxygen producing unit for your habitat.";
        internal static string ScreenDescription => "Monitor all you OxStations from the comfort indoors";
        internal static string ClassID => "OxStation";
        internal static string ScreenClassID => "OxStationScreen";
        internal const string OxstationTabID = "OX";

        internal const string OxstationKitClassID = "OxstationKit_OX";
        internal const string OxstationScreenKitClassID = "OxstationScreenKit_OX";

        internal static event Action<SaveData> OnDataLoaded;

#if SUBNAUTICA
        internal static TechData OxstationIngredients => new TechData
        {
#elif BELOWZERO
        internal static RecipeData OxstationIngredients => new RecipeData
        {
#endif
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 2),
                new Ingredient(TechType.Lubricant, 2),
                new Ingredient(TechType.Tank, 1)
            }
        };
        
#if SUBNAUTICA
        internal static TechData OxstationScreenIngredients => new TechData
        {
#elif BELOWZERO
        internal static RecipeData OxstationScreenIngredients => new RecipeData
        {
#endif
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };

        public static bool RTInstalled
        {
            get
            {
                if (!_techtypeCheck)
                {
                    _isrefrillableTanksInstalled = TechTypeHandler.ModdedTechTypeExists("HighCapacityTankRefill");
                    _techtypeCheck = true;
                }
                return _isrefrillableTanksInstalled;

            }
        }


        #endregion

        #region Internal Methods
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var drills = GameObject.FindObjectsOfType<OxStationController>();

                foreach (var drill in drills)
                {
                    drill.Save(newSaveData);
                }

                _fEHolderSaveData = newSaveData;

                ModUtils.Save<SaveData>(_fEHolderSaveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
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
            return _fEHolderSaveData ?? new SaveData();
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _fEHolderSaveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_fEHolderSaveData);
            });
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }
        internal static void SaveModConfiguration()
        {
            try
            {
                var saveDataJson = JsonConvert.SerializeObject(QPatch.Configuration, Formatting.Indented);

                File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigurationFile()), saveDataJson);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}\n{e.StackTrace}");
            }
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

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static void CreateModConfiguration()
        {
            var config = new Configuration();

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Mod.ConfigurationFile().Trim(), saveDataJson);
        }

        private static Configuration LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<Configuration>(configJson, settings);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile().Trim());
        }

        internal static Configuration LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }
        #endregion
    }

    internal class Options : ModOptions
    {
        private const string AllowDamageID = "OXAllowDamage";
        private const string PlaySFXID = "OxPlaySFX";
        private bool _allowDamage;
        private bool _playSFX;


        public Options() : base("Oxstation Settings")
        {
            ToggleChanged += OnToggleChanged;
            _allowDamage = QPatch.Configuration.DamageOverTime;
            _playSFX = QPatch.Configuration.PlaySFX;
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs e)
        {

            switch (e.Id)
            {
                case AllowDamageID:
                    _allowDamage = QPatch.Configuration.DamageOverTime = e.Value;
                    break;
                case PlaySFXID:
                    _playSFX = QPatch.Configuration.PlaySFX = e.Value;
                    break;
            }

            Mod.SaveModConfiguration();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(AllowDamageID, "Damage Overtime", _allowDamage);
            AddToggleOption(PlaySFXID, "Play SFX", _playSFX);
        }
    }
}
