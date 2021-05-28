using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.AlterraHubFabricatorBuilding.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
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

        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        
        private const string KnownDevicesFilename = "KnownDevices.json";
        internal const string AssetBundleName = "fcsalterrahubbundle";
        internal const string ModName = "AlterraHub";


        internal const string AlterraHubClassID = "AlterraHub";

        internal const string KitClassID = "FCSKit";
        internal const string KitFriendly = "FCS Kit";
        internal const string KitDescription = "A Kit for FCS items";
        internal const string KitPrefabName = "MainConstructionKit";

        internal const string DebitCardClassID = "DebitCard";
        internal const string DebitCardFriendly = "Alterra Debit Card";
        internal const string DebitCardDescription = "A card that stores your money.";
        internal const string CardPrefabName = "CreditCard";
        internal static TechType DebitCardTechType { get; set; }

        internal const string BioFuelClassID = "FCSBioFuel";
        internal const string BioFuelFriendly = "Bio Fuel";
        internal const string BioFuelDescription = "A tank of high-quality Bio Fuel, suitable for use in all Bioreactors.";
        internal const string BioFuelPrefabName = "Liquid_Biofuel";

        internal const string OreConsumerClassID = "OreConsumer";
        internal const string OreConsumerFriendly = "Alterra Ore Consumer";
        internal const string OreConsumerDescription = " Turns your ores into credits to use at the Alterra Hub. The Ore Consumer is always very hungry: keep it well fed.";
        internal const string OreConsumerPrefabName = "OreConsumer";

        internal const string AlterraHubDepotClassID = "AlterraHubDepot";
        internal const string AlterraHubDepotFriendly = "AlterraHub Depot";
        internal const string AlterraHubDepotDescription = "N/A";
        internal const string AlterraHubDepotPrefabName = "AlterraHubDepot";
        internal const string AlterraHubDepotTabID = "AHD";

        internal static TechType AlterraHubDepotTechType { get; set; }
        internal static TechType AlterraHubDepotFragmentTechType { get; set; }

        internal static TechType OreConsumerFragmentTechType { get; set; }
        internal const string OreConsumerTabID = "OC";

        internal static TechType OreConsumerTechType { get; set; }
        public static TechType AlterraHubTechType { get; set; }

        internal static Action<SaveData> OnDataLoaded { get; set; }
        internal static Action<List<KnownDevice>> OnDevicesDataLoaded { get; set; }

#if SUBNAUTICA
        internal static TechData AlterraHubDepotIngredients => new TechData
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
        internal static TechData OreConsumerIngredients => new TechData
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

        public static Dictionary<TechType, float> HeightRestrictions { get; set; } = new Dictionary<TechType, float>
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
            get => _gamePlaySettings ?? (_gamePlaySettings = new FCSGamePlaySettings());
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

        public static Dictionary<string, Sound> AudioClips = new Dictionary<string, Sound>();
        private static AccountDetails _tempAccountDetails;
        private static FCSGamePlaySettings _gamePlaySettings = new FCSGamePlaySettings();


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
                NotifyGamePlayLoaded();
            });

            if(GamePlaySettings == null)
            {
                SaveGamePlaySettings();
            }
        }

        private static void NotifyGamePlayLoaded()
        {
            QuickLogger.Debug("Notifying");
            OnGamePlaySettingsLoaded?.Invoke(GamePlaySettings);
        }

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
            GameObject.DestroyImmediate(_saveObject.gameObject);
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
                    if (controller.Value.PackageId == ModName)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>) controller.Value).Save(newSaveData);
                    }
                }

                QuickLogger.Debug("Attempting to save bases",true);
                newSaveData.BaseSaves = BaseManager.Save().ToList();
                FCSPDAController.Instance.Save(newSaveData);
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
            AudioClips.Add("AH-Mission01-Pt1",AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt1.wav")));
            AudioClips.Add("AH-Mission01-Pt2",AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt2.wav")));
            AudioClips.Add("AH-Mission01-Pt3",AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt3.wav")));
        }

        internal static IEnumerator SpawnAlterraFabStation()
        {
            GameObject gameObject = GameObject.Instantiate(Buildables.AlterraHub.AlterraHubFabricatorPrefab);
            gameObject.AddComponent<AlterraFabricatorStationController>();
            LargeWorldEntity.Register(gameObject);
            gameObject.transform.position = new Vector3(-809.10f, -241.30f, -387.70f);
            Helpers.WorldHelpers.CreateBeacon(gameObject, AlterraHubStationPingType,"Alterra Hub Station");
            MaterialHelpers.ApplyGlassShaderTemplate(gameObject, "_glass", Mod.ModName);

            yield break;
        }



        public static bool IsOreConsumerSpawned { get; set; }
        public static PingType AlterraHubStationPingType { get; set; }

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
    }

    public class FCSGamePlaySettings
    {
        public bool IsOreConsumerFragmentSpawned = false; 
        public List<string> AlterraHubDepotPowercellSlot = new List<string>();
        public KeyPadAccessSave AlterraHubDepotDoors = new KeyPadAccessSave();
        public bool BreakerOn;
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