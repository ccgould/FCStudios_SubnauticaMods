using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub.Configuration
{
    internal class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        internal static string SaveDataFilename => $"{ModName}SaveData.json";

        private const string KnownDevicesFilename = "KnownDevices.json";
        internal const string AssetBundleName = "fcsalterrahubbundle";
        internal const string ModName = "AlterraHub";
        internal const string ModID = "AHB";

        internal const string ModClassID = "AlterraHub";
        internal const string ModFriendly = "Alterra Hub";
        internal const string ModDescription = "AlterraHub your central location for all your Alterra needs!";
        internal const string ModPrefabName = "AlterraHub";

        internal const string KitClassID = "FCSKit";
        internal const string KitFriendly = "FCS Kit";
        internal const string KitDescription = "A Kit for FCS items";
        internal const string KitPrefabName = "MainConstructionKit";

        internal const string DebitCardClassID = "DebitCard";
        internal const string DebitCardFriendly = "Alterra Debit Card";
        internal const string DebitCardDescription = "A card that stores you money.";
        internal const string CardPrefabName = "CreditCard";
        internal static TechType DebitCardTechType { get; set; }

        internal const string BioFuelClassID = "FCSBioFuel";
        internal const string BioFuelFriendly = "Bio Fuel";
        internal const string BioFuelDescription = "A tank of bio fuel to be used in bio reactors";
        internal const string BioFuelPrefabName = "Liquid_Biofuel";

        internal const string OreConsumerClassID = "OreConsumer";
        internal const string OreConsumerFriendly = "Alterra Ore Consumer";
        internal const string OreConsumerDescription = "Ore consumer yum yum give me more.The Ore consumer takes your ores and turns it into money.";
        internal const string OreConsumerPrefabName = "OreConsumer";
        internal const string OreConsumerKitClassID = "OreConsumer_Kit";
        
        
        internal static TechType OreConsumerTechType { get; set; }

        internal static Action<SaveData> OnDataLoaded { get; set; }
        internal static Action<Dictionary<string,string>> OnDevicesDataLoaded { get; set; }

#if SUBNAUTICA
        internal static TechData AlterraHubIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AlterraHubIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.Titanium, 5),
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
        
        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModClassID);
        }

        internal static void LoadDevicesData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<Dictionary<string, string>>(KnownDevicesFilename, GetSaveFileDirectory(), (data) =>
            {
                QuickLogger.Info("Save Data Loaded");
                OnDevicesDataLoaded?.Invoke(data);
            });
        }

        public static bool SaveDevices(Dictionary<string, string> knownDevices)
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

        private static void OnSaveComplete()
        {

        }

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IFCSSave<SaveData>>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                newSaveData.AccountDetails = CardSystem.main.AccountDetails;

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

            CardSystem.main.Load(_saveData.AccountDetails);
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

            return new OreConsumerDataEntry() { Id = id };
        }
        internal static AlterraHubDataEntry GetAlterraHubSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraHubEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraHubDataEntry() { Id = id };
        }
    }
}