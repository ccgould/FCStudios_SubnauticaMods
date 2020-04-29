using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
using Object = System.Object;

namespace DataStorageSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "DataStorageSolutions";
        internal static string ModFolderName => $"FCS_{ModName}";
        internal const string BundleName = "datastoragesolutionsbundle";
        internal const string DSSTabID = "DSS";
        internal const string ModFriendlyName = "Data Storage Solutions";

        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string AssetFolder => Path.Combine(ModName, "Assets");

        internal const string TerminalFriendlyName = "Terminal C48";
        internal const string TerminalDescription = "Terminal C48 bridges the connection between your Data Storage Solutions Server Racks and your finger tips";
        internal const string TerminalClassID = "DSSTerminal";
        internal const string TerminalKitClassID = "TerminalC48_Kit";
        internal const string TerminalPrefabName = "TerminalMontor";

        internal const string FloorMountedRackFriendlyName = "NetShelter C22 Floor Mounted Rack";
        internal const string FloorMountedRackDescription = "The NetShelter C22 is the floor mounted IT enclosure with basic functionality and features provided as a cost-effective solution. The NetShelter FCS maintains a strong focus on cooling, power distribution, and cable management to provide a reliable rack-mounting environment for mission-critical equipment but with optional features such as side panels or even an unassembled option that reduces the cost of the base enclosure.";
        internal const string FloorMountedRackClassID = "DSSFloorMountedRack";
        internal const string FloorMountedRackKitClassID = "FloorMountedRack_Kit";
        internal const string FloorMountedRackPrefabName = "FloorServerRack";

        internal const string WallMountedRackFriendlyName = "NetShelter C23 Wall Mounted Rack";
        internal const string WallMountedRackDescription = "The NetShelter C23 is the wall mounted IT enclosure with basic functionality and features provided as a cost-effective solution. The NetShelter FCS maintains a strong focus on cooling, power distribution, and cable management to provide a reliable rack-mounting environment for mission-critical equipment but with optional features such as side panels or even an unassembled option that reduces the cost of the base enclosure.";
        internal const string WallMountedRackClassID = "DSSWallMountedRack";
        internal const string WallMountedRackKitClassID = "WallMountedRack_Kit";
        internal const string WallMountedRackPrefabName = "WallServerRack";

        internal const string AntennaFriendlyName = "NetShelter Antenna C66";
        internal const string AntennaDescription = "The NetShelter Antenna C66 provides a reliable connection to all your terminals around the planet";
        internal const string AntennaClassID = "DSSAntenna";
        internal const string AntennaKitClassID = "NetShelterAntennaC66_Kit";
        internal const string AntennaPrefabName = "Antenna";

        internal const string ServerFormattingStationFriendlyName = "Server Format Station";
        internal const string ServerFormattingStationDescription = "Use this machine to filter you servers. so they only store what you want int them.";
        internal const string ServerFormattingStationClassID = "DSSFormatStation";
        internal const string ServerFormattingStationKitClassID = "FormatStation_Kit";
        internal const string ServerFormattingStationPrefabName = "ServerFormatMachine";

        internal const string ServerFriendlyName = "NetShelter Server 100";
        internal const string ServerDescription = "The NetShelter Server 100 provides you with 48 free slots to securely access and store your items";
        internal const string ServerClassID = "DSSServer";
        internal const string ServerPrefabName = "Server";

        internal static Action<bool> OnAntennaBuilt;

        internal static Dictionary<string, ServerData> Servers { get; set; } = new Dictionary<string, ServerData>();
        public static List<string> TrackedServers { get; set; } = new List<string>();
        #endregion

        #region Ingredients

#if SUBNAUTICA
        internal static TechData FloorMountedRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData FloorMountedRackIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData WallMountedRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData WallMountedRackHarvIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData TerminalIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData TerminalIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass , 4),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Titanium, 2),
                new Ingredient(TechType.MapRoomHUDChip, 2)
            }
        };

#if SUBNAUTICA
        internal static TechData AntennaIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AntennaIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.TitaniumIngot, 2),
                new Ingredient(TechType.MapRoomUpgradeScanRange, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData ServerIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ServerIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 2),
                new Ingredient(TechType.Aerogel, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData ServerFormattingStationIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ServerFormattingStationIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Aerogel, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };

        internal static Action OnBaseUpdate { get; set; }
        public static Action OnContainerUpdate { get; set; }

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<DataStorageSolutionsController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                CleanServers();

                newSaveData.Servers = Servers;

                _saveData = newSaveData;

                if (_saveData == null)
                {
                    QuickLogger.Error($"Save Failed for mod: {ModFriendlyName}");
                    return;
                }

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        private static void CleanServers()
        {
            var keysToRemove = Servers.Keys.Except(TrackedServers).ToList();

            foreach (var key in keysToRemove)
                Servers.Remove(key);
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
                Servers = data.Servers;

                foreach (KeyValuePair<string, ServerData> objServer in data.Servers)
                {
                    QuickLogger.Debug($"Server Data: S={objServer.Value.Server?.Count} || F={objServer.Value.ServerFilters?.Count}");
                }

                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, ConfigFileName);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile());
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

        #endregion

        private static void CreateModConfiguration()
        {
            try
            {
                var config = new ConfigFile { Config = new Config() };

                var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

                File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigFileName), saveDataJson);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
            }
        }

        private static ConfigFile LoadConfigurationData()
        {
            try
            {
                // == Load Configuration == //
                string configJson = File.ReadAllText(ConfigurationFile().Trim());

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                // == LoadData == //
                return JsonConvert.DeserializeObject<ConfigFile>(configJson, settings);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Failed to load configuration. Using Default");
                QuickLogger.Error(e.StackTrace);
                return new ConfigFile();
            }
        }

        internal static ConfigFile LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }

        public static string GetAssetPath(string fileName)
        {
            return Path.Combine(GetAssetFolder(), fileName);
        }
    }

    internal class Config
    {
        [JsonProperty] internal  float EnergyCost = 1500f;
        [JsonProperty] internal int ServerStorageLimit { get; set; } = 48;
        [JsonProperty] internal float AntennaPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float ScreenPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal bool ShowAllItems { get; set; }
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
