using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;

using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
using Object = UnityEngine.Object;
using UWE;
using FMOD;
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
        public const string ModPackID = "AlterraHub";

        internal static string SaveDataFilename => $"{ModPackID}SaveData.json";

        internal static string KnownDevicesPath => Application.persistentDataPath + "/FCStudiosKnownDevices.dat";
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
        
        internal const string AlterraHubDepotClassID = "AlterraHubDepot";
        internal const string AlterraHubDepotFriendly = "AlterraHub Depot";
        internal const string AlterraHubDepotDescription = "Holds delivered AlterraHub Orders for pickup.";
        internal const string AlterraHubDepotPrefabName = "AlterraHubDepot";
        internal const string AlterraHubDepotTabID = "AHD";

        internal const string DronePortPadHubNewClassID = "DronePortPad";
        internal const string DronePortPadHubNewKitClassID = "DronePortPad_Kit";
        internal const string DronePortPadHubNewFriendly = "Transport Drone Terminal";
        internal const string DronePortPadHubNewDescription = "Receives Transport Drones to transfer items to and from the AlterraHub Depot.";
        internal const string DronePortPadHubNewTabID = "DPP";

        internal const string AlterraHubStationClassID = "AlterraHubLifePodStation";
        internal const string AlterraHubStationFriendly = "AlterraHub Life Pod";
        internal const string AlterraHubStationDescription = "N/A";
        internal const string AlterraHubStationPrefabName = "AlterraHubLifePodStation";

        internal static TechType AlterraHubDepotTechType { get; set; }
        internal const string OreConsumerTabID = "OC";

        internal static TechType OreConsumerTechType { get; set; }
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
            get
            {
                if (_gamePlaySettings == null)
                {
                    _gamePlaySettings = new FCSGamePlaySettings();
                }

                return _gamePlaySettings;
            }
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

        internal static void CollectKnownDevices()
        {
            if (File.Exists(KnownDevicesPath))
            {
                // Declare the hashtable reference.
                List<KnownDevice> addresses;

                FileStream fs = null;

                try
                {
                    // Open the file containing the data that you want to deserialize.
                    fs = new FileStream(KnownDevicesPath, FileMode.Open);

                    BinaryFormatter formatter = new BinaryFormatter();

                    addresses = (List<KnownDevice>)formatter.Deserialize(fs);
                    OnDevicesDataLoaded?.Invoke(addresses);
                }
                catch (SerializationException e)
                {
                    QuickLogger.Error("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs?.Close();
                }

                foreach (KnownDevice de in addresses)
                {
                    QuickLogger.Debug($"{de.ID} has prefab ID {de.PrefabID}.");
                }
            }
            else
            {
                OnDevicesDataLoaded?.Invoke(new List<KnownDevice>());
            }
        }

        public static void SaveDevices(List<KnownDevice> devices)
        {
            FileStream fs = new FileStream(KnownDevicesPath, FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, devices);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
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

                DroneDeliveryService.Main.Save();
                
                ModUtils.Save(_gamePlaySettings, "settings.json", GetSaveFileDirectory(), OnSaveComplete);

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
                QuickLogger.Debug($"Finished loading Game Play Settings : {JsonConvert.SerializeObject(data, Formatting.Indented)}");
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

                if (DroneDeliveryService.Main != null)
                {
                    foreach (AlterraTransportDroneEntry entry in DroneDeliveryService.Main.SaveDrones())
                    {
                        if(entry != null)
                            newSaveData.AlterraTransportDroneEntries.Add(entry);
                    }
                }

                QuickLogger.Debug("Attempting to save bases",true);
                newSaveData.BaseSaves = BaseManager.Save().ToList();
                Player_Patches.FCSPDA.Save(newSaveData);
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
            FCSAlterraHubService.PublicAPI.OnPurge?.Invoke();
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


            if (_saveData != null && !CardSystem.main.IsLoaded)
            {
                CoroutineHost.StartCoroutine(CardSystem.main.Load(_saveData.AccountDetails));
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

        public static void LoadAudioFiles()
        {
            AudioClips.Add("AH-Mission01-Pt1", AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt1.wav")));
            AudioClips.Add("AH-Mission01-Pt2", AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt2.wav")));
            AudioClips.Add("AH-Mission01-Pt3", AudioUtils.CreateSound(Path.Combine(GetAssetPath(), "Audio", "AH-Mission01-Pt3.wav")));
        }

        public static PingType AlterraHubStationPingType { get; set; }
        public static TechType DronePortPadHubNewTechType { get; set; }
        public static TechType AlterraTransportDroneTechType { get; set; }
        public static PingType AlterraTransportDronePingType { get; set; }
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
        
        public static void RegisterVoices()
        {
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "ElectricalBoxesNeedFixing.mp3"), string.Empty);
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "PDA_Instructions.mp3"), "Hello! My name is Ava, I am your personal assistant embedded in your FCStudios PDA. I have established a connection to the nearest AlterraHub Fabricator Station. Alterra Universal Transponder is unable to establish a connection to an Authorized Alterra Network. AlterraHub now running in Autonomous Mode. Alternate instructions have been loaded to this PDA.");
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "PDA_Account_Instructions.mp3"), "Please create a temporary AlterraHub account. This temporary account is needed because the Alterra Universal Transponder is unable to establish a connection to an Authorized Alterra Network. This temporary account will let you accumulate Alterra Credits and order goods and services from AlterraHub. Don't worry, your temporary account will be merged with your official Alterra Account when in range of an Authorized Alterra Network.");
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "PDA_Account_Created.mp3"), @"Thank you for creating a temporary Alterra Account. You can now order goods and services from enabled AlterraHub Catalogs. Ores deposited in an Alterra Ore Consumer will add credits to your account. The Alterra Depot holds your deliveries for pickup so a convenient location is preferable. The Alterra Transport Drone Terminal recharges the Transport Drone while it delivers your order. Please route base traffic accordingly. Please contact your Project Supervisor if any of these items are not available. For more information on these devices, please see the AlterraHub Encyclopedia on the FCStudios PDA.");
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "PDA_Drone_Instructions.mp3"), "An Alterra Transport Drone Terminal and Alterra Depot are necessary for orders to be delivered. Please notify your Project Supervisor.");
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "Play_NotDebtPaid.mp3"), null /*"Hey Riley! Welcome home to Alterra, permission to land denied.  You still have an outstanding balance on your Alterra account."*/);
            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(), "Audio", "Play_DebtPaid.mp3"), null /*"Hey Riley!  Welcome home to Alterra, permission to land granted. All debt has been paid."*/);
        }

        public static PatreonStatueDataEntry GetPatreonStatueEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PatreonStatueEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PatreonStatueDataEntry() { Id = id };
        }
    }


    public class FCSGamePlaySettings
    {
        public List<string> AlterraHubDepotPowercellSlot = new();
        public KeyPadAccessSave AlterraHubDepotDoors = new();
        public bool BreakerOn;
        public bool IsPDAUnlocked;
        public HashSet<int> FixedPowerBoxes = new();
        public HashSet<string> Conditions = new();
        public bool IsPDAOpenFirstTime = true;
        public bool TransDroneSpawned { get; set; }
        public Dictionary<string,int> DronePortAssigns { get; set; } = new();
        public bool IsStationSpawned { get; set; }
        [JsonProperty] internal Dictionary<string, Shipment> PendingPurchases { get; set; }
        [JsonProperty] internal Shipment CurrentOrder { get; set; }
        public int FabStationBeaconColorIndex { get; set; }
        public bool FabStationBeaconVisible { get; set; } = true;
        public float Rate = 10;
        public bool AutomaticDebitDeduction;

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
    
    public class KeyPadAccessSave
    {
        public bool KeyPad1;
        public bool KeyPad2;
        public bool KeyPad3;
        public bool KeyPad4;
        public float SecurityDoors;
    }

    [Serializable]
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