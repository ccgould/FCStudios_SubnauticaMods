using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Display;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using UWE;

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
        internal const string ServerFormattingStationDescription = "Use this machine to filter your servers so they only store what you want them to accept.";
        internal const string ServerFormattingStationClassID = "DSSFormatStation";
        internal const string ServerFormattingStationKitClassID = "FormatStation_Kit";
        internal const string ServerFormattingStationPrefabName = "ServerFormatMachine";

        internal const string ServerFriendlyName = "NetShelter Server 100";
        internal const string ServerDescription = "The NetShelter Server 100 provides you with 48 free slots to securely access and store your items";
        internal const string ServerClassID = "DSSServer";
        internal const string ServerPrefabName = "Server";

        internal const string OperatorFriendlyName = "DSS Operator";
        internal const string OperatorDescription = "The DSS Operator allows your DSS system to connect to other devices to perform repeated task and performs other crafting operations";
        internal const string OperatorClassID = "DSSOperator";
        internal const string OperatorPrefabName = "DSSOperator";
        internal const string OperatorKitClassID = "DSSOperator_Kit";

        internal const string ItemDisplayFriendlyName = "DSS Item Display";
        internal const string ItemDisplayDescription = "The DSS Item display allows you to view how much of one item you have in your system.";
        internal const string ItemDisplayClassID = "DSSItemDisplay";
        internal const string ItemDisplayPrefabName = "ItemDisplay";
        internal const string ItemDisplayKitID = "ItemDisplay_Kit";

        internal static Action<bool> OnAntennaBuilt;
        private static TechType _seaBreezeTechType;

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
                new Ingredient(TechType.MapRoomHUDChip, 1)
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
                new Ingredient(TechType.ComputerChip, 1),
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

#if SUBNAUTICA
        internal static TechData DSSOperatorIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSOperatorIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Titanium, 3)
            }
        };

#if SUBNAUTICA
        internal static TechData ItemDisplayIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ItemDisplayIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Titanium, 2)
            }
        };

        internal static Action OnBaseUpdate { get; set; }
        internal static Action<DSSRackController> OnContainerUpdate { get; set; }
        internal static List<TechType> AllTechTypes  = new List<TechType>();
        public static LootDistributionData LootDistributionData { get; set; }

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();
                //GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDataSoluationsSave>();
                var controllers = GameObject.FindObjectsOfType<DataStorageSolutionsController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                CleanServers();

                newSaveData.Servers = Servers;
                newSaveData.Bases = BaseManager.GetSaveData().ToList();
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
                if(string.IsNullOrEmpty(entry.ID)) continue;

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

        internal static BaseSaveData GetBaseSaveData(string instanceId)
        {
            LoadData();

            var saveData = GetSaveData();

            if (saveData.Bases == null) return null;

            foreach (var entry in saveData.Bases)
            {
                if(string.IsNullOrEmpty(entry.InstanceID)) continue;

                if (entry.InstanceID == instanceId)
                {
                    return new BaseSaveData{BaseName = entry.BaseName, InstanceID = entry.InstanceID, AllowDocking = entry.AllowDocking, HasBreakerTripped = entry.HasBreakerTripped};
                }
            }

            return null;
        }

        internal static void LoadData()
        {
            if (_saveData != null) return;
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


        private static void GetResourceForSpecial(TechType techType, List<TechType> items)
        {
            switch (techType)
            {
                case TechType.LimestoneChunk:
                    items.Add(TechType.Titanium);
                    items.Add(TechType.Copper);
                    break;
                case TechType.ShaleChunk:
                    items.Add(TechType.Diamond);
                    items.Add(TechType.Gold);
                    items.Add(TechType.Lithium);
                    break;
                case TechType.SandstoneChunk:
                    items.Add(TechType.Silver);
                    items.Add(TechType.Gold);
                    items.Add(TechType.Lead);
                    break;
                case TechType.DrillableSalt:
                    items.Add(TechType.Salt);
                    break;
                case TechType.DrillableQuartz:
                    items.Add(TechType.Quartz);
                    break;
                case TechType.DrillableCopper:
                    items.Add(TechType.Copper);
                    break;
                case TechType.DrillableTitanium:
                    items.Add(TechType.Titanium);
                    break;
                case TechType.DrillableLead:
                    items.Add(TechType.Lead);
                    break;
                case TechType.DrillableSilver:
                    items.Add(TechType.Silver);
                    break;
                case TechType.DrillableDiamond:
                    items.Add(TechType.Diamond);
                    break;
                case TechType.DrillableGold:
                    items.Add(TechType.Gold);
                    break;
                case TechType.DrillableMagnetite:
                    items.Add(TechType.Magnetite);
                    break;
                case TechType.DrillableLithium:
                    items.Add(TechType.Lithium);
                    break;
                case TechType.DrillableMercury:
                    items.Add(TechType.MercuryOre);
                    break;
                case TechType.DrillableUranium:
                    items.Add(TechType.Uranium);
                    break;
                case TechType.DrillableAluminiumOxide:
                    items.Add(TechType.AluminumOxide);
                    break;
                case TechType.DrillableNickel:
                    items.Add(TechType.Nickel);
                    break;
                case TechType.DrillableSulphur:
                    items.Add(TechType.Sulphur);
                    break;
                case TechType.DrillableKyanite:
                    items.Add(TechType.Kyanite);
                    break;
            }
        }

        public static List<TechType> Resources = new List<TechType>()
        {
            TechType.AluminumOxide,
            TechType.Sulphur,
            TechType.Diamond,
            TechType.Kyanite,
            TechType.Lead,
            TechType.Lithium ,
            TechType.Magnetite,
            TechType.Nickel,
            TechType.Quartz,
            TechType.Silver,
            TechType.UraniniteCrystal,
            TechType.Salt,
            TechType.Titanium,
            TechType.Copper,
            TechType.Gold,
            TechType.LimestoneChunk,
            TechType.ShaleChunk,
            TechType.SandstoneChunk,
            TechType.DrillableSalt,
            TechType.DrillableQuartz,
            TechType.DrillableCopper,
            TechType.DrillableTitanium,
            TechType.DrillableLead,
            TechType.DrillableSilver,
            TechType.DrillableDiamond,
            TechType.DrillableGold,
            TechType.DrillableMagnetite,
            TechType.DrillableLithium,
            TechType.DrillableMercury,
            TechType.DrillableUranium,
            TechType.DrillableAluminiumOxide,
            TechType.DrillableNickel,
            TechType.DrillableSulphur,
            TechType.DrillableKyanite,
            "FCSGlass".ToTechType()
        };

        internal static Dictionary<BiomeType, List<TechType>> GetAllBiomesData()
        {
            Dictionary<BiomeType, List<TechType>> biomeLoot = new Dictionary<BiomeType, List<TechType>>();

            foreach (BiomeType biomeType in Enum.GetValues(typeof(BiomeType)))
            {
                if (LootDistributionData == null)
                {
                    QuickLogger.Debug("LootDistributionData is null");
                    return null;
                }

                if (!LootDistributionData.GetBiomeLoot(biomeType, out LootDistributionData.DstData data))
                {
                    QuickLogger.Debug("DstData is null");
                    continue;
                }

                foreach (LootDistributionData.PrefabData prefabData in data.prefabs)
                {
                    if (prefabData.classId.ToLower() != "none")
                    {
                        if (WorldEntityDatabase.TryGetInfo(prefabData.classId, out WorldEntityInfo wei))
                        {
                            if (wei == null)
                            {
                                QuickLogger.Debug("WorldEntityInfo is null");
                                continue;
                            }

                            if (Resources.Contains(wei.techType))
                            {
                                if (!biomeLoot.ContainsKey(biomeType))
                                    biomeLoot[biomeType] = new List<TechType>();

                                biomeLoot[biomeType].Add(wei.techType);

                                QuickLogger.Debug($"Added Resource: {wei.techType} in biome {biomeType}");
                            }
                        }
                    }
                }


                var tempDictionary = new Dictionary<BiomeType, List<TechType>>();

                foreach (KeyValuePair<BiomeType, List<TechType>> pair in biomeLoot)
                {
                    var loot = new List<TechType>();
                    foreach (TechType techType in pair.Value)
                    {
                        if (techType.ToString().EndsWith("Chunk", StringComparison.OrdinalIgnoreCase) ||
                            techType.ToString().StartsWith("Drillable", StringComparison.OrdinalIgnoreCase))
                        {
                            GetResourceForSpecial(techType, loot);
                        }
                        else
                        {
                            loot.Add(techType);
                        }
                    }

                    tempDictionary.Add(pair.Key, loot.Distinct().ToList());
                }

                var newList = new Dictionary<string, List<string>>();

                foreach (KeyValuePair<BiomeType, List<TechType>> biomeData in tempDictionary)
                {
                    var name = biomeData.Key.AsString(true).Split('_')[0];
                    if (!newList.ContainsKey(name))
                    {
                        var g = new List<string>();

                        foreach (var techType in biomeData.Value)
                        {
                            g.Add(techType.AsString());
                        }
                        newList.Add(name, g);
                    }
                    else
                    {
                        foreach (TechType techType in biomeData.Value)
                        {
                            if (!newList[name].Contains(techType.AsString()))
                            {
                                newList[name].Add(techType.AsString());
                            }

                            if (!AllTechTypes.Contains(techType))
                            {
                                AllTechTypes.Add(techType);
                            }
                        }
                    }
                }
            }

            AllTechTypes = AllTechTypes.Where(x => !CraftData.IsBuildableTech(x) && !_blackList.Contains(x)).ToList();

            //CreateItemButtons();
            return biomeLoot;
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

        internal static void CreateItemButtons()
        {
            foreach (TechType techType in AllTechTypes)
            {
                if(CraftData.IsBuildableTech(techType) || _blackList.Contains(techType)) continue;
                GameObject buttonPrefab = GameObject.Instantiate(DSSModelPrefab.OperatorItemPrefab);
                var button = buttonPrefab.AddComponent<OperationInterfaceButton>();
                button.BtnName = "ItemTechBTN";
                button.Tag = new OperatorButtonData { Button = button, TechType = techType };
                button.TextLineOne = $"Send {Language.main.Get(techType)}";
                buttonPrefab.GetComponentInChildren<Text>().text = Language.main.Get(techType);
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(techType);
                ItemTechTypeButtons.Add(button);
            }
        }

        public static List<TechType> _blackList = new List<TechType>
        {
            TechType.BaseCorridor,
            TechType.Accumulator,
            TechType.Terraformer,
            TechType.CurrentGenerator,
            TechType.PowerGlide,
            TechType.Seamoth,
            TechType.LootSensorFragment,
            TechType.LootSensorMetal,
            TechType.LootSensorLithium,
            TechType.Cyclops,
            TechType.StarshipCargoCrate,
            TechType.StarshipCircuitBox,
            TechType.StarshipMonitor,
            TechType.HullReinforcementModule,
            TechType.HullReinforcementModule2,
            TechType.HullReinforcementModule3,
            TechType.Nanowires,
            TechType.BaseCorridorGlass,
            TechType.SeamothReinforcementModule,
            TechType.FiltrationMachine,
            TechType.Exosuit,
            TechType.RocketBase,
            TechType.RocketBaseLadder,
            TechType.RocketStage1,
            TechType.RocketStage2,
            TechType.RocketStage3,
            TechType.Transfuser
        };

        public static List<OperationInterfaceButton> ItemTechTypeButtons { get; set; } = new List<OperationInterfaceButton>();

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

        internal static void AddBlackListFilter(Filter filter)
        {
            var dockingList = QPatch.Configuration.Config.DockingBlackList;
            
            var result = dockingList.Any(x => x.IsSame(filter));

            if (!result)
            {
                dockingList.Add(filter);
            }

            if (!dockingList.Contains(filter))
            {
                
            }

            SaveModConfiguration();
        }

        internal static void RemoveBlackListFilter(Filter filter)
        {
            var dockingList = QPatch.Configuration.Config.DockingBlackList;

            foreach (Filter enabledFilter in dockingList)
            {
                if (enabledFilter.IsSame(filter))
                {
                    dockingList.Remove(enabledFilter);
                }
            }
            SaveModConfiguration();
        }

        internal static bool IsFilterAdded(Filter compareFilter)
        {
            foreach (Filter filter in QPatch.Configuration.Config.DockingBlackList)
            {
                if (filter.IsSame(compareFilter))
                {
                    QuickLogger.Debug("Filter is in the list",true);
                    return true;
                }
            }
            QuickLogger.Debug("Filter is not in the list", true);

            return false;
        }

        internal static bool IsFilterAddedWithType(TechType techType)
        {
            foreach (Filter filter in QPatch.Configuration.Config.DockingBlackList)
            {
                if (filter.IsTechTypeAllowed(techType))
                {
                    return true;
                }
            }

            return false;
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

        public static TechType GetSeaBreezeTechType()
        {
            if (_seaBreezeTechType == TechType.None)
            {
                _seaBreezeTechType = "ARSSeaBreezeFCS32".ToTechType();
            }

            return _seaBreezeTechType;
        }
    }

    internal class Config
    {
        [JsonProperty] internal int ServerStorageLimit { get; set; } = 48;
        [JsonProperty] internal float AntennaPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float ScreenPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal bool ShowAllItems { get; set; }
        [JsonProperty] internal float RackPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float ServerPowerUsage { get; set; } = 0.05f;
        [JsonProperty] internal bool PullFromDockedVehicles { get; set; } = true;
        [JsonProperty] internal float CheckVehiclesInterval { get; set; } = 2.0f;
        [JsonProperty] internal int ExtractMultiplier { get; set; }

        [JsonProperty] internal float ExtractInterval = 0.25f;
        [JsonProperty] internal bool AllowFood { get; set; }

        private HashSet<Filter> _dockingBlackList = new HashSet<Filter>();

        [JsonProperty]
        internal HashSet<Filter> DockingBlackList
        {
            get => _dockingBlackList;
            set
            {
                _dockingBlackList = FilterList.GetNewVersion(value);
                Mod.SaveModConfiguration();
            }
        }
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }

    internal class Options : ModOptions
    {
        //private ModModes _modMode;
        private const string ExtractMultiplierID = "DSSEMulti";
        //private const string AllowFoodToggle = "DSSAllowFood";


        public Options() : base("Data Storage Solutions Settings")
        {
            ChoiceChanged += Options_ChoiceChanged;
            //ToggleChanged += Options_ToggleChanged;
        }

        //private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        //{
        //    switch (e.Id)
        //    {
        //        case ExtractMultiplierID:
        //            QPatch.Configuration.Config.AllowFood = e.Value;
        //            break;
        //    }

        //    Mod.SaveModConfiguration();
        //}

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            switch (e.Id)
            {
                case ExtractMultiplierID:
                    QPatch.Configuration.Config.ExtractMultiplier = e.Index;
                    break;
            }

            Mod.SaveModConfiguration();
        }

        public override void BuildModOptions()
        {
            AddChoiceOption(ExtractMultiplierID, "Extract Multiplier", new []
            {
                "x0",
                "x5",
                "x10",
                "x20"
            }, QPatch.Configuration.Config.ExtractMultiplier);

            //AddToggleOption(AllowFoodToggle, "Allow Food", QPatch.Configuration.Config.AllowFood);
        }
    }
}
