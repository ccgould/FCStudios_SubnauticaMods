using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono;
using FCS_AlterraHub.Mods.FCSDataBox.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using Story;
using UnityEngine;
using Object = UnityEngine.Object;
using SearchOption = System.IO.SearchOption;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif


namespace FCS_AlterraHub.Configuration
{
    internal class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static bool _audioLoaded;
        public const string ModPackID = "AlterraHub";

        internal static string SaveDataFilename => $"{ModPackID}SaveData.json";
        
        private const string KnownDevicesFilename = "KnownDevices.json";
        internal const string AssetBundleName = "fcsalterrahubbundle";



        internal const string AlterraHubClassID = "AlterraHub";

        internal const string KitClassID = "FCSKit";
        internal const string KitFriendly = "FCS Kit";
        internal const string KitDescription = "A Kit for FCS items";
        internal const string KitPrefabName = "MainConstructionKit";

        internal const string DebitCardClassID = "DebitCard";
        internal const string DebitCardFriendly = "Alterra Debit Card";
        internal const string DebitCardDescription = "This card lets you access your Alterra Account. You must have this card with you to make Alterra Hub purchases.";
        internal const string CardPrefabName = "CreditCard";
        internal static TechType DebitCardTechType { get; set; }

        internal const string OreConsumerClassID = "OreConsumer";
        internal const string OreConsumerFriendly = "Alterra Ore Consumer";
        internal const string OreConsumerDescription = " Turns your ores into credits to use at the Alterra Hub. The Ore Consumer is always very hungry: keep it well fed.";
        internal const string OreConsumerPrefabName = "OreConsumer";

        internal const string AlterraHubDepotClassID = "AlterraHubDepot";
        internal const string AlterraHubDepotFriendly = "AlterraHub Depot";
        internal const string AlterraHubDepotDescription = "N/A";
        internal const string AlterraHubDepotPrefabName = "AlterraHubDepot";
        internal const string AlterraHubDepotTabID = "AHD";

        internal const string DronePortPadHubNewClassID = "DronePortPad";
        internal const string DronePortPadHubNewKitClassID = "DronePortPad_Kit";
        internal const string DronePortPadHubNewFriendly = "Drone Port Pad";
        internal const string DronePortPadHubNewDescription = "N/A";
        internal const string DronePortPadHubNewTabID = "DPP";

        internal static TechType AlterraHubDepotTechType { get; set; }
        internal static TechType AlterraHubDepotFragmentTechType { get; set; }

        internal static TechType OreConsumerFragmentTechType { get; set; }
        internal const string OreConsumerTabID = "OC";

        internal static TechType OreConsumerTechType { get; set; }
        public static TechType AlterraHubTechType { get; set; }

        internal static Action<SaveData> OnDataLoaded { get; set; }
        internal static Action<List<KnownDevice>> OnDevicesDataLoaded { get; set; }

#if SUBNAUTICA
        internal static TechData AlterraHubDepotIngredients => new()
#elif BELOWZERO
                internal static RecipeData AlterraHubDepotIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData OreConsumerIngredients => new()
#elif BELOWZERO
                internal static RecipeData OreConsumerIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 1),
                new Ingredient(TechType.CopperWire, 1),
                new Ingredient(TechType.Lubricant, 1)
            }
        };

        public static Dictionary<TechType, float> HeightRestrictions { get; set; } = new()
        {
            {TechType.BloodVine,.03f},
            {TechType.Creepvine,.03f},
            {TechType.MelonPlant,.5f},
            {TechType.AcidMushroom,0.3752623f},
            {TechType.WhiteMushroom,0.3752623f},
            {TechType.PurpleTentacle,0.398803f},
            {TechType.BulboTree,0.1638541f},
            {TechType.OrangeMushroom,0.3261985f},
            {TechType.PurpleVasePlant,0.3261985f},
            {TechType.HangingFruitTree,0.1932445f},
            {TechType.PurpleVegetablePlant,0.3793474f},
            {TechType.FernPalm,0.3770215f},
            {TechType.OrangePetalsPlant,0.2895765f},
            {TechType.PinkMushroom,0.5093553f},
            {TechType.PurpleRattle,0.5077053f},
            {TechType.PinkFlower,0.3104943f},
            //{TechType.PurpleTentacle,0.2173283f},
            {TechType.SmallKoosh,0.3104943f},
            {TechType.GabeSFeather,0.2198986f},
            {TechType.MembrainTree,0.1574817f},
            {TechType.BluePalm,0.4339138f},
            {TechType.EyesPlant,0.2179814f},
            {TechType.RedBush,0.1909147f},
            {TechType.RedGreenTentacle,0.2514144f},
            {TechType.RedConePlant,0.1909147f},
            {TechType.SpikePlant,0.2668017f},
            {TechType.SeaCrown,0.2668017f},
            {TechType.PurpleStalk,0.2668017f},
            {TechType.RedBasketPlant,0.2455478f},
            {TechType.ShellGrass,0.2455478f},
            {TechType.SpottedLeavesPlant,0.2455478f},
            {TechType.RedRollPlant,0.2082229f},
            {TechType.PurpleBranches,0.3902903f},
            {TechType.SnakeMushroom,0.3902903f},
            {TechType.PurpleFan,0.2761627f},
            {TechType.SmallFan,0.2761627f},
            {TechType.JellyPlant,0.2761627f},
            {TechType.PurpleBrainCoral, 0.3731633f}
        };

        public static FCSGamePlaySettings GamePlaySettings
        {
            get => _gamePlaySettings ??= new FCSGamePlaySettings();
            set => _gamePlaySettings = value;
        }

        public static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), AlterraHubClassID);
        }

        internal static void LoadDevicesData()
        {
            //if (_knownDevicesLoaded) return;
            //QuickLogger.Info("Loading Known Devices Save Data...");
            //ModUtils.LoadSaveData<List<KnownDevice>>(KnownDevicesFilename, GetSaveFileDirectory(), data =>
            //{
            //    QuickLogger.Info("Known Devices Save Data Loaded");
            //    OnDevicesDataLoaded?.Invoke(data);
            //    _knownDevicesLoaded = true;
            //});
        }

        internal static void CollectKnownDevices()
        {

            var items = new HashSet<KnownDevice>();

#if SUBNAUTICA
            var path = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "../..", "Unknown Worlds/Subnautica/Subnautica/SavedGames"));
#elif BELOWZERO
            var path = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "../..", "Unknown Worlds/Subnautica Below Zero/SubnauticaZero/SavedGames"));
#endif
            QuickLogger.Debug($"User storage: {path}");

            if (string.IsNullOrWhiteSpace(path))
            {
                QuickLogger.Debug("Path is null");
                return;
            }

            if (Directory.Exists(path))
            {
                string[] allfiles = Directory.GetFiles(path, "KnownDevices.json", SearchOption.AllDirectories);
                //QModServices.Main.AddCriticalMessage($"All Files Count: {allfiles.Length}");

                foreach (var file in allfiles)
                {
                    var save = File.ReadAllText(file);
                    var json = JsonConvert.DeserializeObject<List<KnownDevice>>(save);
                    foreach (KnownDevice device in json)
                    {
                        items.Add(device);
                    }
                }
            }

            //QModServices.Main.AddCriticalMessage($"Found: {items.Count} Devices");

            OnDevicesDataLoaded?.Invoke(items.ToList());
        }


        public static bool SaveDevices(List<KnownDevice> knownDevices)
        {
            
            try
            {
                ModUtils.Save(knownDevices, KnownDevicesFilename, GetSaveFileDirectory(), OnSaveComplete);
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} | StackTrace: {e.StackTrace}");
                return false;
            }
        }

        public static Dictionary<string, Sound> AudioClips = new();
        private static AccountDetails _tempAccountDetails;
        private static FCSGamePlaySettings _gamePlaySettings = new();
        public static TechType FCSDataBoxTechType { get; set; }


        public static bool SaveGamePlaySettings()
        {
            try
            {
                QuickLogger.Debug("Saving Gameplay Settings",true);

                if (GamePlaySettings == null)
                {
                    GamePlaySettings = new FCSGamePlaySettings();
                }

                
                ModUtils.Save(GamePlaySettings, "settings.json", GetSaveFileDirectory(), OnSaveComplete);

                if (File.Exists(Path.Combine(GetSaveFileDirectory(), "settings.json")))
                {
                    QuickLogger.Debug("Saved Gameplay Settings", true);
                }
                else
                {
                    QuickLogger.DebugError("Gameplay Settings Save Not Found", true);
                }
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} | StackTrace: {e.StackTrace}");
                return false;
            }
        }

        internal static void LoadGamePlaySettings()
        {
            QuickLogger.Info("Loading Game Play Settings...");
            ModUtils.LoadSaveData<FCSGamePlaySettings>("settings.json", GetSaveFileDirectory(), data =>
            {
                QuickLogger.Info($"Finished loading Game Play Settings : {JsonConvert.SerializeObject(data, Formatting.Indented)}");
                GamePlaySettings = data;
            });

            if(GamePlaySettings == null)
            {
                SaveGamePlaySettings();
            }

            NotifyGamePlayLoaded();
        }

        private static void NotifyGamePlayLoaded()
        {
            ModGamePlaySettingsLoaded = true;
            QuickLogger.Debug("Notifying");
            OnGamePlaySettingsLoaded?.Invoke(GamePlaySettings);
        }

        public static bool ModGamePlaySettingsLoaded { get; set; }

        public static Action<FCSGamePlaySettings> OnGamePlaySettingsLoaded { get; set; }

        internal static void OnSaveComplete()
        {
            _saveObject?.StartCoroutine(SaveCoroutine());
        }

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            Object.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                QuickLogger.Debug($"Registered Devices Returned: {FCSAlterraHubService.PublicAPI.GetRegisteredDevices().Count}");

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    if (controller.Value.PackageId == ModPackID)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>) controller.Value).Save(newSaveData);
                    }
                }

                if (AlterraFabricatorStationController.Main != null)
                {
                    foreach (AlterraTransportDroneEntry entry in AlterraFabricatorStationController.Main.SaveDrones())
                    {
                        if(entry != null)
                            newSaveData.AlterraTransportDroneEntries.Add(entry);
                    }
                }

                QuickLogger.Debug("Attempting to save bases",true);
                newSaveData.BaseSaves = BaseManager.Save().ToList();
                QuickLogger.Debug($"Save 1 {Player_Patches.FCSPDA}", true);
                Player_Patches.FCSPDA.Save(newSaveData);
                QuickLogger.Debug("Save 2", true);
                QuickLogger.Debug("Bases saved", true);

                if (_tempAccountDetails != null)
                {
                    newSaveData.AccountDetails = _tempAccountDetails;
                    _tempAccountDetails = null;
                }
                else
                {
                    newSaveData.AccountDetails = CardSystem.main.SaveDetails();
                }

                QuickLogger.Debug("After Saved Details", true);
                _saveData = newSaveData;

                SaveGamePlaySettings();

                ModUtils.Save(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }
        
        internal static void PurgeSave()
        {
            _saveData = null;
            GamePlaySettings = new FCSGamePlaySettings();
            ModGamePlaySettingsLoaded = false;
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), data =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });


            if (_saveData != null)
            {
                CardSystem.main.Load(_saveData.AccountDetails);
            }
        }
        
        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static OreConsumerDataEntry GetOreConsumerDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.OreConsumerEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new OreConsumerDataEntry { Id = id };
        }
        
        internal static FCSPDAEntry GetAlterraHubSaveData()
        {
            LoadData();

            var saveData = GetSaveData();

            return saveData.FCSPDAEntry ?? new FCSPDAEntry();
        }

        internal static BaseSaveData GetBaseSaveData(string instanceId)
        {
            LoadData();

            var saveData = GetSaveData();
            
            if (saveData.BaseSaves == null) return null;

            foreach (var entry in saveData.BaseSaves)
            {
                if (string.IsNullOrEmpty(entry.InstanceID)) continue;

                if (entry.InstanceID == instanceId)
                {
                    return entry;
                }
            }


            return null;
        }

        public static string GetIconPath(string classId)
        {
            return Path.Combine(GetAssetPath(), $"{classId}.png");
        }

        public static void LoadAudioFiles()
        {
            AudioClips.Add("AH-Mission01-Pt1",AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt1.wav")));
            AudioClips.Add("AH-Mission01-Pt2",AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt2.wav")));
            AudioClips.Add("AH-Mission01-Pt3",AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt3.wav")));
        }
        
        internal static IEnumerator SpawnAlterraFabStation(FCSGamePlaySettings fcsGamePlaySettings)
        {
            while (LargeWorldStreamer.main?.cellManager?.streamer?.globalRoot == null)
            {
                //QuickLogger.Error("LargeWorldStreamer not ready to make station",true);
                yield return null;
            }
            
            var spawnLocation = new Vector3(82.70f, -316.9f, -1434.7f);
            var spawnRotation = Quaternion.Euler(348.7f, 326.24f, 43.68f);
            var station = SpawnHelper.SpawnTechType(AlterraStationTechType, spawnLocation,spawnRotation);

           

            fcsGamePlaySettings.IsStationSpawned = true;
            yield break;
        }


        public static PingType AlterraHubStationPingType { get; set; }
        public static TechType StaffKeyCardTechType { get; set; }
        public static TechType DronePortPadHubNewTechType { get; set; }
        public static TechType AlterraTransportDroneTechType { get; set; }
        public static PingType AlterraTransportDronePingType { get; set; }
        public static TechType AlterraStationTechType { get; set; }

        public static void DeepCopySave(AccountDetails accountDetails)
        {
            _tempAccountDetails = new AccountDetails(accountDetails);
        }

        public static AlterraHubDepotEntry GetAlterraHubDepotEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraHubDepotEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraHubDepotEntry { Id = id };
        }

        public static AlterraDronePortEntry GetAlterraDronePortSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraDronePortEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraDronePortEntry { Id = id };
        }

        public static AlterraTransportDroneEntry GetAlterraTransportDroneSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraTransportDroneEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraTransportDroneEntry { Id = id };
        }
    }


    public class FCSGamePlaySettings
    {
        public bool IsOreConsumerFragmentSpawned = false; 
        public List<string> AlterraHubDepotPowercellSlot = new();
        public KeyPadAccessSave AlterraHubDepotDoors = new();
        public bool BreakerOn;
        public bool IsPDAUnlocked;
        public HashSet<int> FixedPowerBoxes = new();
        public HashSet<string> Conditions = new();
        public bool TransDroneSpawned { get; set; }
        public Dictionary<string,int> DronePortAssigns { get; set; } = new();
        public bool IsStationSpawned { get; set; }
        public Dictionary<string, DataBoxData> DataBoxes { get; set; } = new();

        public bool ConditionMet(string condition)
        {
            return Conditions.Contains(condition);
        }

        public void SetCondition(string condition)
        {
            Conditions.Add(condition);
            Mod.SaveGamePlaySettings();
        }
    }

    public class DataBoxData
    {
        public bool Used { get; set; }
        public TechType TechType { get; set; }
    }

    public class KeyPadAccessSave
    {
        public bool KeyPad1;
        public bool KeyPad2;
    }

    public struct KnownDevice
    {
        public string PrefabID { get; set; }
        public string DeviceTabId { get; set; }
        public int ID { get; set; }

        public override string ToString()
        {
            return $"{DeviceTabId}{ID:D3}";
        }
    }
}