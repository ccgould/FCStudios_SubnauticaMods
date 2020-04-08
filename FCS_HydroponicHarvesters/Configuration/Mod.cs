using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_HydroponicHarvesters.Mono;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Model;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "HydroponicHarvester";
        internal static string ModFolderName => $"FCS_{ModName}s";
        internal const string BundleName = "hydroponicharvesterbundle";
        internal const string HydroHarvTabID = "HH";
        internal const string ModFriendlyName = "Hydroponic Harvester";

        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string AssetFolder => Path.Combine(ModName, "Assets");
        
        internal const string LargeFriendlyName = "Large Hydroponic Harvester";
        internal const string LargeDescription = "A demo mod for FCStudios";
        internal const string LargeClassID = "LargeHydroponicHarvester";
        internal const string LargeHydroHarvKitClassID = "LargeHydroHarv_Kit";
        internal const string LargePrefabName = "HydroponicHarvesterLarge";

        internal const string MediumFriendlyName = "Medium Hydroponic Harvester";
        internal const string MediumDescription = "A demo mod for FCStudios";
        internal const string MediumClassID = "MediumHydroponicHarvester";
        internal const string MediumHydroHarvKitClassID = "MediumHydroHarv_Kit";
        internal const string MediumPrefabName = "HydroponicHarvesterMedium";

        internal const string SmallFriendlyName = "Small Hydroponic Harvester";
        internal const string SmallDescription = "A demo mod for FCStudios";
        internal const string SmallClassID = "SmallHydroponicHarvester";
        internal const string SmallHydroHarvKitClassID = "SmallHydroHarv_Kit";
        internal const string SmallPrefabName = "HydroponicHarvesterSmall";

        public static List<DNASample> DNASamples = new List<DNASample>
        {
            new DNASample("Coral Chunk","CoralChunk",TechType.CoralChunk),
            new DNASample("Acid Mushroom","AcidMushroom",TechType.AcidMushroomSpore),
            new DNASample("Blood Oil","BloodOil",TechType.BloodOil),
            new DNASample("Blue Palm","BluePalm",TechType.BluePalmSeed),
            new DNASample("Bulbo Tree","BulboTree",TechType.BulboTreePiece),
            new DNASample("Bulb Bush","BulbBush",TechType.KooshChunk),
            new DNASample("Cave Bush","CaveBush",TechType.PurpleBranchesSeed),
            new DNASample("Chinese Potato","ChinesePotato",TechType.PurpleVegetable),
            new DNASample("Creepvine","Creepvine",TechType.CreepvineSeedCluster),
            new DNASample("Deep Shroom","DeepShroom",TechType.AcidMushroomSpore),
            new DNASample("Eye Stalk","EyeStalk",TechType.EyesPlantSeed),
            new DNASample("Fern Palm","FernPalm",TechType.FernPalmSeed),
            new DNASample("Furled Papyrus","FurledPapyrus",TechType.RedRollPlantSeed),
            new DNASample("Gabe's Feather","GabesFeather",TechType.GabeSFeatherSeed),
            new DNASample("Ghost Weed","GhostWeed",TechType.RedGreenTentacleSeed),
            new DNASample("Grub Basket","GrubBasket",TechType.OrangePetalsPlantSeed),
            new DNASample("Jaffa Cup","JaffaCup",TechType.OrangeMushroomSpore),
            new DNASample("Jellyshroom","Jellyshroom",TechType.SnakeMushroomSpore),
            new DNASample("Lantern Tree","LanternTree",TechType.HangingFruit),
            new DNASample("Marblemelon Plant","MarblemelonPlant",TechType.MelonSeed),
            new DNASample("Membrain Tree","MembrainTree",TechType.MembrainTreeSeed),
            new DNASample("Ming Plant","MingPlant",TechType.PurpleVasePlantSeed),
            new DNASample("Pygmy Fan","PygmyFan",TechType.SmallFanSeed),
            new DNASample("Redwort","Redwort",TechType.RedBushSeed),
            new DNASample("Regress Shell","RegressShell",TechType.RedConePlantSeed),
            new DNASample("Rouge Cradle","RougeCradle",TechType.RedBasketPlantSeed),
            new DNASample("Sea Crown","SeaCrown",TechType.SeaCrownSeed),
            new DNASample("Spiked Horn Grass","SpikedHornGrass",TechType.ShellGrassSeed),
            new DNASample("Speckled Rattler","SpeckledRattler",TechType.PurpleRattleSpore),
            new DNASample("Spotted Dockleaf","SpottedDockleaf",TechType.SpottedLeavesPlantSeed),
            new DNASample("Sulphur Plant","SulphurPlant",TechType.CrashPowder),
            new DNASample("Tiger Plant","TigerPlant",TechType.SpikePlantSeed),
            new DNASample("Veined Nettle","VeinedNettle",TechType.PurpleFanSeed),
            new DNASample("Violet Beau","VioletBeau",TechType.PurpleStalkSeed),
            new DNASample("Voxel Shrub","VoxelShrub",TechType.PinkFlowerSeed),
            new DNASample("Writhing Weed","WrithingWeed",TechType.PurpleTentacleSeed)
        };

        public static List<Vector3> LargeBubblesLocations = new List<Vector3>
        {
            new Vector3(0.779f, 1.03f, 0.766f),
            new Vector3(0.779f, 1.03f, -0.791f),
            new Vector3(-0.763f, 1.03f, 0.645f),
            new Vector3(-0.714f, 1.03f, -0.745f)
        };

        public static List<Vector3> MediumBubblesLocations = new List<Vector3>
        {
            new Vector3(0.373f, 1.03f, -0.667f),
            new Vector3(0.373f, 1.03f, 0.645f),
            new Vector3(-0.448f, 1.03f, 0.645f),
            new Vector3(-0.448f, 1.03f, -0.667f)
        };

        public static List<Vector3> SmallBubblesLocations = new List<Vector3>
        {
            new Vector3(0.49f, 1.03f, 0.602f),
            new Vector3(0.49f, 1.03f, -0.36f),
            new Vector3(-0.556f, 1.03f, 0.629f),
            new Vector3(-0.517f, 1.03f, -0.402f)
        };

        #region Ingredients

#if SUBNAUTICA
        internal static TechData LargeHydroHarvIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData LargeHydroHarvIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Kyanite, 9)
            }
        };

#if SUBNAUTICA
        internal static TechData MediumHydroHarvIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData MediumHydroHarvIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Kyanite, 9)
            }
        };

#if SUBNAUTICA
        internal static TechData SmallHydroHarvIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData SmallHydroHarvIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Kyanite, 9)
            }
        };
        
        #endregion

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods

        internal static void Save()
        {


            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<HydroHarvController>();

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
            var config = new ConfigFile { Config = new Config() };

            var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigFileName), saveDataJson);
        }

        private static ConfigFile LoadConfigurationData()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // == LoadData == //
            return JsonConvert.DeserializeObject<ConfigFile>(configJson, settings);
        }

        internal static ConfigFile LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }
    }

    internal class Config
    {
        [JsonProperty] internal  float EnergyCost = 1500f;
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
