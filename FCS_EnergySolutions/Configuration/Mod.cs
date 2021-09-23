 using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.Utilities;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Mods.AlterraGen.Enumerators;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_EnergySolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static Dictionary<TechType, float> _vanillaBioChargeValues;
        #endregion

        internal static string ModPackID => "FCSEnergySolutions";
        internal static string SaveDataFilename => $"FCSEnergySolutionsSaveData.json";
        internal const string ModBundleName = "fcsenergysolutionsbundle";

        internal const string TelepowerPylonTabID = "TP";
        internal const string TelepowerPylonFriendlyName = "Telepower Pylon";
        internal const string TelepowerPylonModName = "TelepowerPylon";
        internal const string TelepowerPylonDescription = "With a Telepower Pylon, you can send or receive energy wirelessly across vast distances. Requires one to Send / Push and another to Receive / Pull.";
        internal static string TelepowerPylonKitClassID => $"{TelepowerPylonModName}_Kit";
        internal static string TelepowerPylonClassName => TelepowerPylonModName;
        internal static string TelepowerPylonPrefabName => TelepowerPylonModName;

        internal const string WindSurferOperatorTabID = "WSO";
        internal const string WindSurferOperatorFriendlyName = "Wind Surfer Operator";
        internal const string WindSurferOperatorModName = "WindSurferOperator";
        internal const string WindSurferOperatorDescription = "Build and control up to 15 WindSurfer turbines, broadcasting their power via integrated Telepower Pylon.";
        internal static string WindSurferOperatorKitClassID => $"{WindSurferOperatorModName}_Kit";
        internal static string WindSurferOperatorClassName => WindSurferOperatorModName;
        internal static string WindSurferOperatorPrefabName => WindSurferOperatorModName;

        internal const string WindSurferTabID = "WS";
        internal const string WindSurferFriendlyName = "Wind Surfer";
        internal const string WindSurferModName = "WindSurfer";
        internal const string WindSurferDescription = "Wind turbine floating platform, built with Operator.";
        internal static string WindSurferKitClassID => $"{WindSurferModName}_Kit";
        internal static string WindSurferClassName => WindSurferModName;
        internal static string WindSurferPrefabName => WindSurferModName;

        internal const string WindSurferPlatformTabID = "WSP";
        internal const string WindSurferPlatformFriendlyName = "Wind Surfer Platform";
        internal const string WindSurferPlatformModName = "WindSurferPlatform";
        internal const string WindSurferPlatformDescription = " Floating platform, built with Operator.";
        internal static string WindSurferPlatformKitClassID => $"{WindSurferPlatformModName}_Kit";
        internal static string WindSurferPlatformClassName => WindSurferPlatformModName;
        internal static string WindSurferPlatformPrefabName => WindSurferPlatformModName;


        internal const string AlterraGenModTabID = "AG";
        internal const string AlterraGenModFriendlyName = "Alterra Gen";
        internal const string AlterraGenModName = "AlterraGen";
        internal const string AlterraGenModDescription = "Versatile bio-reactor holding 9 biological items of any size, suitable for interior and exterior use.";
        internal static string AlterraGenKitClassID => $"{AlterraGenModName}_Kit";
        internal static string AlterraGenModClassName => AlterraGenModName;
        internal static string AlterraGenModPrefabName => AlterraGenModName;


        internal const string AlterraSolarClusterModTabID = "ASC";
        internal const string AlterraSolarClusterModFriendlyName = "Alterra Solar Cluster";
        internal const string AlterraSolarClusterModName = "AlterraSolarCluster";

        internal const string AlterraSolarClusterModDescription =
            "The Alterra Solar Cluster is the latest in efficient photon conversion, giving you a reliable, maintenance free power source anywhere the sun goes.";
        internal static string AlterraSolarClusterKitClassID => $"{AlterraSolarClusterModName}_Kit";
        public static string AlterraSolarClusterModClassName => AlterraSolarClusterModName;
        internal static string AlterraSolarClusterModPrefabName => AlterraSolarClusterModName;



        internal const string JetStreamT242TabID = "MT";
        internal const string JetStreamT242Description = "The T242 turbine generates power from water currents, automatically adjusting  for optimal power generation. Note that some biomes may have stronger currents than others.";
        internal const string JetStreamT242FriendlyName = "JetStreamT242";
        internal const string JetStreamT242ModName = "JetStreamT242";
        internal const string JetStreamT242ClassName = "JetStreamT242";
        internal static string JetStreamT242KitClassID => $"{JetStreamT242ModName}_Kit";
        internal static string JetStreamT242PrefabName => JetStreamT242ModName;

        internal const string PowerStorageTabID = "PS";
        internal const string PowerStorageDescription = "Zero maintenance power backup. Stores energy when Base Power is plentiful, discharges energy when Base Power is low.";
        internal const string PowerStorageFriendlyName = "PowerStorage";
        internal const string PowerStorageModName = "PowerStorage";
        internal const string PowerStorageClassName = "PowerStorage";
        internal static string PowerStorageKitClassID => $"{PowerStorageModName}_Kit";
        internal static string PowerStoragePrefabName => PowerStorageModName;

        internal const string UniversalChargerTabID = "UC";
        internal const string UniversalChargerDescription = "Holds up to 10 Power Cells or 10 Batteries and recharges them when Base Power is plentiful";
        internal const string UniversalChargerFriendlyName = "Universal Charger";
        internal const string UniversalChargerModName = "UniversalCharger";
        internal const string UniversalChargerClassName = "UniversalCharger";
        internal static string UniversalChargerKitClassID => $"{UniversalChargerModName}_Kit";
        internal static string UniversalChargerPrefabName => UniversalChargerModName;


        internal const string BioFuelClassID = "FCSBioFuel";
        internal const string BioFuelFriendly = "Bio Fuel";
        internal const string BioFuelDescription = "A tank of high-quality Bio Fuel, suitable for use in all Bioreactors.";
        internal const string BioFuelPrefabName = "Liquid_Biofuel";


#if SUBNAUTICA
        internal static TechData AlterraGenIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AlterraGenIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(AlterraGenKitClassID.ToTechType(), 1),
            }
        };

        public static Action<bool> OnLightsEnabledToggle { get; set; }
        public static Action<string> OnPlatFormRemoved { get; set; }

        internal const string ModDescription ="";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static Dictionary<TechType,float> GetBioChargeValues()
        {
            if (_vanillaBioChargeValues == null)
            {
                Type baseBioReactorType = typeof(BaseBioReactor);
                _vanillaBioChargeValues = (Dictionary<TechType, float>)AccessTools.Field(baseBioReactorType, "charge").GetValue(baseBioReactorType);
            }

            if (_vanillaBioChargeValues == null)
            {
                QuickLogger.Error("Failed to get vanilla bio charge values using stored values");
            }

            return _vanillaBioChargeValues ?? FuelTypes.Charge;
        }

        internal static List<TechType> AllowedBioItems()
        {
            var buffer = new List<TechType>();
            foreach (KeyValuePair<TechType, float> bioChargeValue in GetBioChargeValues())
            {
                buffer.Add(bioChargeValue.Key);
            }

            return buffer;
        }

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    if (controller.Value.PackageId == ModPackID)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
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
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModPackID);
        }

        internal static AlterraGenDataEntry GetAlterraGenSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraGenEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraGenDataEntry() { Id = id };
        }

        internal static TelepowerPylonDataEntry GetTelepowerPylonSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.TelepowerPylonEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new TelepowerPylonDataEntry() { Id = id };
        }        
        
        internal static JetStreamT242DataEntry GetJetStreamT242SaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MarineTurbineEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new JetStreamT242DataEntry() { Id = id };
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
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

        #endregion

        public static PowerStorageDataEntry GetPowerStorageSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PowerStorageEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PowerStorageDataEntry() { Id = id };
        }

        public static AlterraSolarClusterDataEntry GetAlterraSolarClusterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraSolarClusterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraSolarClusterDataEntry() { Id = id };
        }

        public static WindSurferOperatorDataEntry GetWindSurferOperatorSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.WindSurferOperatorEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new WindSurferOperatorDataEntry() { Id = id };
        }

        public static WindSurferDataEntry GetWindSurferSaveData(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null; 
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.WindSurferEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new WindSurferDataEntry() { Id = id };
        }

        public static UniversalChargerDataEntry GetUniversalChargerSaveData(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.UniversalChargerEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new UniversalChargerDataEntry() { Id = id };
        }
    }
}
