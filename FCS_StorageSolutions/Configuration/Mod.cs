﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_StorageSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static TechType _dssServerTechType;
        private static List<DSSServerController> _registeredServers;

        #endregion

      
        internal static string ModName => "FCSStorageSolutions";
        internal static string SaveDataFilename => $"FCSStorageSolutionsSaveData.json";
        internal const string ModBundleName = "fcsstoragesolutionsbundle";
        
        internal const string AlterraStorageTabID = "AS";
        internal const string AlterraStorageFriendlyName = "Remote Storage Unit";
        internal const string AlterraStorageModName = "AlterraStorage";
        internal static string AlterraStorageKitClassID => $"{AlterraStorageClassName}_Kit";
        internal const string AlterraStorageClassName = "AlterraStorage";
        internal const string AlterraStoragePrefabName = "AlterraStorage";
        internal const string AlterraStorageDescription = "Freestanding storage unit for 200 items. Suitable for interior and exterior use. Can be used to receive ores directly from an automated drill platform if in range.";

        internal const string DSSTabID = "DSS";
        internal const string DSSServerFriendlyName = "Server";
        internal const string DSSServerClassName = "DSSServer";
        internal const string DSSServerPrefabName = "DSS_Server";
        internal const string DSSServerDescription = "Data Storage for 48 items, formatted to accept all item categories. Place in a Wall Server Rack or Floor Server Rack to connect to Data Storage Network.";



        internal const string ItemTransferUnitTabID= "ITU";
        internal const string ItemTransferUnitFriendlyName = "Item Transfer Unit";
        internal const string ItemTransferUnitClassName = "ItemTransferUnit";
        internal const string ItemTransferUnitPrefabName = "ItemTransferUnit";
        internal const string ItemTransferUnitDescription = "N/A";
        internal const string ItemTransferUnitKitClassID = "ItemTransferUnit_Kit";

        internal const string DSSFormattingStationFriendlyName = "Server Formatting Station";
        internal const string DSSFormattingStationClassName = "DSSFormattingStation";
        internal const string DSSFormattingStationPrefabName = "DSS_ServerFormatMachine";
        internal static string DSSFormattingStationKitClassID => $"{DSSFormattingStationClassName}_Kit";
        internal const string DSSFormattingStationDescription = "This station allows to you format your drive to accept or reject certain items or a category of items. Servers DO NOT have to be formatted.";

        internal const string DSSItemDisplayFriendlyName = "Terminal S23";
        internal const string DSSItemDisplayClassName = "DSSItemDisplay";
        internal const string DSSItemDisplayPrefabName = "DSS_ItemDisplay";
        internal static string DSSItemDisplayKitClassID => $"{DSSItemDisplayClassName}_Kit";
        internal const string DSSItemDisplayDescription = "Small, single-item access terminal for the Data Storage Network. Now with multi-item deposit capability.";

        internal const string DSSAutoCrafterFriendlyName = "Auto Crafter";
        internal const string DSSAutoCrafterClassName = "DSSAutoCrafter";
        internal const string DSSAutoCrafterPrefabName = "DSS_AutoCrafter";
        internal static string DSSAutoCrafterKitClassID => $"{DSSAutoCrafterClassName}_Kit";
        internal const string DSSAutoCrafterDescription = "Avoid long hours in front of the Fabricator. Queue up a list of multiple items or just keep yourself automatically stocked on an important one.";

        internal const string DSSTerminalFriendlyName = "Terminal C48";
        internal const string DSSTerminalClassName = "DSSTerminalMonitor";
        internal const string DSSTerminalPrefabName = "DSS_TerminalMonitor";
        internal static string DSSTerminalKitClassID => $"{DSSTerminalClassName}_Kit";
        internal const string DSSTerminalDescription = "Data Storage Control Terminal. Deposit and Withdraw items, automatically unload vehicles, and access the Data Storage Networks at other habitats and cyclops on the Global Network.";

        internal const string DSSWallServerRackFriendlyName = "Wall Server Rack";
        internal const string DSSWallServerRackClassName = "DSSWallServerRack";
        internal const string DSSWallServerRackPrefabName = "DSS_WallServerRack";
        internal static string DSSWallServerRackKitClassID => $"{DSSWallServerRackClassName}_Kit";
        internal const string DSSWallServerRackDescription = "Holds 6 Data Storage Servers. Requires a wall.";

        internal const string DSSFloorServerRackFriendlyName = "Floor Server Rack";
        internal const string DSSFloorServerRackClassName = "DSSFloorServerRack";
        internal const string DSSFloorServerRackPrefabName = "DSS_FloorServerRack";
        internal static string DSSFloorServerRackKitClassID => $"{DSSFloorServerRackClassName}_Kit";
        internal const string DSSFloorServerRackDescription = "Holds 17 Data Storage Servers. Requires a floor.";


        internal const string DSSAntennaFriendlyName = "Antenna";
        internal const string DSSAntennaClassName = "DSSAntenna";
        internal const string DSSAntennaPrefabName = "DSS_Antenna";
        internal static string DSSAntennaKitClassID => $"{DSSAntennaClassName}_Kit";
        internal const string DSSAntennaDescription = "An antenna connects your habitat’s Data Storage System to the Global Data Storage Network of habitats (with antennae) and cyclops (built-in antennae).";

#if SUBNAUTICA
        internal static TechData DSSFloorServerRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSFloorServerRackIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSFloorServerRackKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData DSSAutoCrafterIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSAutoCrafterIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSAutoCrafterKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData DSSAntennaIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSAntennaIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSAntennaKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData DSSTerminalIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSTerminalIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSTerminalKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData DSSItemDisplayIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSItemDisplayIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSItemDisplayKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData DSSFormattingStationIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSFormattingStationIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSFormattingStationKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData ItemTransferUnitIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ItemTransferUnitIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(ItemTransferUnitKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData AlterraStorageIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AlterraStorageIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(AlterraStorageKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData DSSWallServerRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSWallServerRackIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(DSSWallServerRackKitClassID.ToTechType(), 1),
            }
        };

        public static List<TechType> Craftables { get; set; } = new List<TechType>();

        internal const string ModDescription = "";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                if (_registeredServers != null)
                {
                    foreach (DSSServerController controller in _registeredServers)
                    {
                        controller.Save(newSaveData, serializer);
                    }
                }


                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    QuickLogger.Debug($"Attempting to save device: {controller.Value?.UnitID} | Package Valid: { controller.Value?.PackageId == ModName}");

                    if (controller.Value.PackageId == ModName && !controller.Value.BypassRegisterCheck)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
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
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static AlterraStorageDataEntry GetAlterraStorageSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraStorageDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new AlterraStorageDataEntry() { ID = id };
        }

        internal static DSSServerDataEntry GetDSSServerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSServerDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSServerDataEntry() { ID = id };
        }

        internal static DSSFormattingStationDataEntry GetDSSFormattingStationSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSFormattingStationDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSFormattingStationDataEntry() { ID = id };
        }

        internal static DSSItemDisplayDataEntry GetDSSItemDisplaySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSItemDisplayDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSItemDisplayDataEntry() { ID = id };
        }

        internal static DSSAntennaDataEntry GetDSSAntennaSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSAntennaDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSAntennaDataEntry() { ID = id };
        }
        
        internal static DSSAutoCrafterDataEntry GetDSSAutoCrafterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSAutoCrafterDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSAutoCrafterDataEntry() { ID = id };
        }
        
        internal static DSSTerminalDataEntry GetDSSTerminalSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSTerminalDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSTerminalDataEntry() { ID = id };
        }

        internal static DSSWallServerRackDataEntry GetDSSWallServerRackSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSWallServerRackDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSWallServerRackDataEntry() { ID = id };
        }

        internal static DSSFloorServerRackDataEntry GetDSSFloorServerRackSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DSSFloorServerRackDataEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new DSSFloorServerRackDataEntry() { ID = id };
        }

        internal static ItemTransferUnitDataEntry GetItemTransferUnitSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ItemTransferUnitDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ItemTransferUnitDataEntry() { Id = id };
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

        public static TechType GetDSSServerTechType()
        {
            if (_dssServerTechType == TechType.None)
            {
                _dssServerTechType = DSSServerClassName.ToTechType();
            }

            return _dssServerTechType;
        }

        public static void RegisterServer(DSSServerController server)
        {
            if (_registeredServers == null)
            {
                QuickLogger.Debug($"Registered Server Tool: {server.GetPrefabID()}");
                _registeredServers = new List<DSSServerController>();
            }

            if (!_registeredServers.Contains(server))
            {
                _registeredServers.Add(server);
            }
        }

        public static void UnRegisterServer(DSSServerController server)
        {
            _registeredServers.Remove(server);
        }

    }
}