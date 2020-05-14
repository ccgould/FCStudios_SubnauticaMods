using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_HydroponicHarvesters;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Mono;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Model;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Options;
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
        internal const string LargeDescription = "A large hydroponic harvester that allows you to store 4 DNA samples to clone.";
        internal const string LargeClassID = "LargeHydroponicHarvester";
        internal const string LargeHydroHarvKitClassID = "LargeHydroHarv_Kit";
        internal const string LargePrefabName = "HydroponicHarvesterLarge";

        internal const string MediumFriendlyName = "Medium Hydroponic Harvester";
        internal const string MediumDescription = "A medium hydroponic harvester that allows you to store 2 DNA samples to clone.";
        internal const string MediumClassID = "MediumHydroponicHarvester";
        internal const string MediumHydroHarvKitClassID = "MediumHydroHarv_Kit";
        internal const string MediumPrefabName = "HydroponicHarvesterMedium";

        internal const string SmallFriendlyName = "Small Hydroponic Harvester";
        internal const string SmallDescription = "A small hydroponic harvester that allows you to store 1 DNA sample to clone.";
        internal const string SmallClassID = "SmallHydroponicHarvester";
        internal const string SmallHydroHarvKitClassID = "SmallHydroHarv_Kit";
        internal const string SmallPrefabName = "HydroponicHarvesterSmall";

        internal const string FloraKleenClassID = "FloraKleen";
        internal const string FloraKleenFriendlyName = "FloraKleen";
        internal const string FloraKleenDescription = "FloraKleen® removes fertilizer residues that can accumulate over time in hydroponic systems, growing media and potting soils. Use FloraKleen® monthly to purge your hydroponic Harvestor system or potted plants of excess salts that can accumulate as a result of regular fertilizer application. FloraKleen® is an excellent final flush and can be used to dissolve mineral and salt buildup.";

        public static List<DNASample> EatableDNASamples = new List<DNASample>
        {
            new DNASample("Bulbo Tree","BulboTree",TechType.BulboTreePiece),
            new DNASample("Bulb Bush","BulbBush",TechType.KooshChunk),
            new DNASample("Chinese Potato","ChinesePotato",TechType.PurpleVegetable),
            new DNASample("Marblemelon Plant","MarblemelonPlant",TechType.Melon),
            new DNASample("Lantern Tree","LanternTree",TechType.HangingFruit),
        };

        public static List<DNASample> UsableDNASamples = new List<DNASample>
        {
            new DNASample("Creepvine","Creepvine",TechType.CreepvineSeedCluster),
            new DNASample("Deep Shroom","DeepShroom",TechType.WhiteMushroom),
            new DNASample("Coral Chunk","CoralChunk",TechType.CoralChunk),
            new DNASample("Acid Mushroom","AcidMushroom",TechType.AcidMushroom),
            new DNASample("Blood Oil","BloodOil",TechType.BloodOil),
            new DNASample("Sulphur Plant","SulphurPlant",TechType.CrashPowder),
            new DNASample("Table Coral","BlueJeweledDisk",TechType.JeweledDiskPiece),
            new DNASample("Creepvine","CreepvinePiece",TechType.CreepvinePiece),
            new DNASample("Gel Sack","GelSack",TechType.JellyPlant),
            new DNASample("Tree Mushroom Piece","TreeMushroomPiece",TechType.TreeMushroomPiece),
        };

        public static List<DNASample> DecorSamples = new List<DNASample>
        {
            new DNASample("Blue Palm","BluePalm",TechType.BluePalmSeed),
            new DNASample("Cave Bush","CaveBush",TechType.PurpleBranchesSeed),
            new DNASample("Eye Stalk","EyeStalk",TechType.EyesPlantSeed),
            new DNASample("Fern Palm","FernPalm",TechType.FernPalmSeed),
            new DNASample("Furled Papyrus","FurledPapyrus",TechType.RedRollPlantSeed),
            new DNASample("Gabe's Feather","GabesFeather",TechType.GabeSFeatherSeed),
            new DNASample("Ghost Weed","GhostWeed",TechType.RedGreenTentacleSeed),
            new DNASample("Grub Basket","GrubBasket",TechType.OrangePetalsPlantSeed),
            new DNASample("Jaffa Cup","JaffaCup",TechType.OrangeMushroomSpore),
            new DNASample("Jellyshroom","Jellyshroom",TechType.SnakeMushroomSpore),
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
            new DNASample("Tiger Plant","TigerPlant",TechType.SpikePlantSeed),
            new DNASample("Veined Nettle","VeinedNettle",TechType.PurpleFanSeed),
            new DNASample("Violet Beau","VioletBeau",TechType.PurpleStalkSeed),
            new DNASample("Voxel Shrub","VoxelShrub",TechType.PinkFlowerSeed),
            new DNASample("Writhing Weed","WrithingWeed",TechType.PurpleTentacleSeed),
            new DNASample("Brain Coral","BrainCoral",TechType.PurpleBrainCoralPiece),
            new DNASample("Pink Cap","PinkCap",TechType.PinkMushroomSpore),

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
                new Ingredient(TechType.Silicone, 4),
                new Ingredient(TechType.AramidFibers, 4),
                new Ingredient(TechType.EnameledGlass, 3),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.PrecursorKey_Purple, 1)
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
                new Ingredient(TechType.Silicone, 2),
                new Ingredient(TechType.AramidFibers, 2),
                new Ingredient(TechType.EnameledGlass, 2),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.PrecursorKey_Purple, 1)
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
                new Ingredient(TechType.Silicone, 1),
                new Ingredient(TechType.AramidFibers, 1),
                new Ingredient(TechType.EnameledGlass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.PrecursorKey_Purple, 1)
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

        public static void SaveModConfiguration()
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
    }

    internal class Config
    {
        [JsonProperty] internal  float EnergyCost = 1500f;
        [JsonProperty] internal int LargeStorageLimit { get; set; } = 100;
        [JsonProperty] internal int MediumStorageLimit { get; set; } = 50;
        [JsonProperty] internal int SmallStorageLimit { get; set; } = 25;
        [JsonProperty] internal bool GetsDirtyOverTime { get; set; } = true;
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }

}

internal class Options : ModOptions
{
    private bool _getsDirtyOverTime;
    private const string DirtyOverTimeID = "HHDirtyOverTime";


    public Options() : base("Hydroponic Harvester Settings")
    {
        ToggleChanged += OnToggleChanged;
        _getsDirtyOverTime = QPatch.Configuration.Config.GetsDirtyOverTime;
    }

    public void OnToggleChanged(object sender, ToggleChangedEventArgs e)
    {

        switch (e.Id)
        {
            case DirtyOverTimeID:
                _getsDirtyOverTime = QPatch.Configuration.Config.GetsDirtyOverTime = e.Value;
                break;
        }

        Mod.SaveModConfiguration();
    }

    public override void BuildModOptions()
    {
        AddToggleOption(DirtyOverTimeID, "Gets Dirty Overtime", _getsDirtyOverTime);
    }
}
