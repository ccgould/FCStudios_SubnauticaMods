using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Utilities;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static int _seabreezeCount;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "FCS_ARSSeaBreeze";
        internal const string LegacySaveFolderName = "ARSSeaBreezeFCS32";
        internal const string BundleName = "arsseabreezefcs32modbundle";
        internal const string SeaBreezeTabID = "SB";
        internal const string FriendlyName = "ARS Sea Breeze FCS32";
        internal const string Description = "Alterra Refrigeration Sea Breeze will keep your items fresh longer!";
        internal const string ClassID = "ARSSeaBreezeFCS32";
        internal const string ModFolderName = ModName;
        internal static string ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal const string SeaBreezeKitClassID = "SeaBreezeKit_SB";
        internal const string FreonDescription = "Freon gives your SeaBreeze cooling on Planet 4546B.";
        internal const string FreonClassID = "Freon_ARS";
        internal const string FreonFriendlyName = "Freon";
        internal static event Action<SaveData> OnDataLoaded;
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string SaveDataFilename => $"{ClassID}SaveData.json";
        

#if SUBNAUTICA
        internal static TechData SeaBreezeIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.PowerCell, 1),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Glass, 1),
                new Ingredient(Mod.FreonClassID.ToTechType(), 1)
            }
        };

#elif BELOWZERO
        internal static RecipeData SeaBreezeIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.PowerCell, 1),
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Glass, 1),
                new Ingredient(Mod.FreonClassID.ToTechType(), 1)
            }
        };

#endif

        #endregion

        #region Internal Methods
        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, "config.json");
        }

        internal static string GetNewSeabreezeName()
        {
            QuickLogger.Debug($"Get Seabreeze New Name");
            return $"{FriendlyName} {_seabreezeCount++}";
        }

        internal static void Save()
        {


            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<ARSolutionsSeaBreezeController>();

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
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), LegacySaveFolderName);
        }

        internal static SaveDataEntry GetSaveData(string id)
        {
            LoadData(SaveDataFilename);

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

        internal static void LoadData(string saveDataFilename)
        {
            QuickLogger.Info("Loading Save Data...");
            
            ModUtils.LoadSaveData<SaveData>(saveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        internal static ModConfiguration LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile());
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

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }
        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
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

        private static void CreateModConfiguration()
        {
            var config = new ModConfiguration();

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigFileName), saveDataJson);
        }

        private static ModConfiguration LoadConfigurationData()
        {
            try
            {
                // == Load Configuration == //
                string configJson = File.ReadAllText(ConfigurationFile().Trim());

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                // == LoadData == //
                return JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
            }
            catch
            {
                QuickLogger.Error($"Failed to load config. Fallback to default values.");
                return new ModConfiguration();
            }
        }
        #endregion
    }

    internal class Options : ModOptions
    {
        private ModModes _modMode;
        private bool _useBasePower;
        private const string ModModeID = "SBModMode";
        private const string UsePowerID = "SBUsePower";


        public Options() : base("SeaBreeze Settings")
        {
            ToggleChanged += OnToggleChanged;
            ChoiceChanged += Options_ChoiceChanged;
            _useBasePower = QPatch.Configuration.UseBasePower;
            _modMode = QPatch.Configuration.ModMode;
        }

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            switch (e.Id)
            {
                case ModModeID:
                    _modMode = QPatch.Configuration.ModMode = (ModModes)System.Enum.Parse(typeof(ModModes), e.Value, true);
                    break;
            }

            Mod.SaveModConfiguration();
        }

        public void OnToggleChanged(object sender, ToggleChangedEventArgs e)
        {

            switch (e.Id)
            {
                case ModModeID:
                    _useBasePower = QPatch.Configuration.UseBasePower = e.Value;
                    break;
            }

            Mod.SaveModConfiguration();
        }
        
        public override void BuildModOptions()
        {
            AddChoiceOption<ModModes>(ModModeID,"Seabreeze GameMode", _modMode);
            AddToggleOption(UsePowerID,"Use Base Power",_useBasePower);
        }
    }
}
