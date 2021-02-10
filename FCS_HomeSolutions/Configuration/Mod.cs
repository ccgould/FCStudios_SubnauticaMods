using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Mono.PaintTool;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_HomeSolutions.Configuration
{
    internal static class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static List<PaintToolController> _registeredPaintTool;
        
        internal const string ModName = "FCSHomeSolutions";
        internal const string ModFriendlyName = "Home Solutions";
        internal const string ModBundleName = "fcshomesolutionsbundle";

        internal const string DecorationItemTabId = "DI";

        internal const string RecyclerClassID = "Recycler";
        internal const string RecyclerFriendly = "Recycler";
        internal const string RecyclerDescription = "Recycle your trash and get your resources back";
        internal const string RecyclerPrefabName = "Recycler";
        internal const string RecyclerKitClassID = "Recycler_Kit";
        internal const string RecyclerTabID = "RR";

        internal const string TrashReceptacleClassID = "TrashReceptacle";
        internal const string TrashReceptacleFriendly = "Trash Receptacle";
        internal const string TrashReceptacleDescription = "Use the Trash Receptacle to quickly send your trash to the recycler from the inside of your base";
        internal const string TrashReceptaclePrefabName = "TrashReceptacle";
        internal const string TrashReceptacleKitClassID = "TrashReceptacle_Kit";
        internal const string TrashReceptacleTabID = "TR";

        internal const string FireExtinguisherRefuelerClassID = "FireExtinguisherRefueler";
        internal const string FireExtinguisherRefuelerFriendly = "Fire Extinguisher Refueler";
        internal const string FireExtinguisherRefuelerDescription = "Use the Fire Extinguisher Refueler to refill any fire extinguisher";
        internal const string FireExtinguisherRefuelerPrefabName = "FireExtinguisherRefueler";
        internal const string FireExtinguisherRefuelerKitClassID = "FireExtinguisherRefueler_Kit";
        internal const string FireExtinguisherRefuelerTabID = "FER";
        
        internal const string PaintToolClassID = "PaintTool";
        internal const string PaintToolFriendly = "Alterra Paint Tool";
        internal const string PaintToolDescription = "Change the color of Primary and Secondary surfaces, and LED lights (requires Paint Can). Only suitable for Alterra FCStudios products.";
        internal const string PaintToolPrefabName = "PaintTool";
        internal const string PaintToolKitClassID = "PaintTool_Kit";

        internal const string BaseOperatorClassID = "BaseOperator";
        internal const string BaseOperatorFriendly = "Alterra Base Operator";
        internal const string BaseOperatorDescription = "Controls various Base functions (Lights, Beacon, Power, etc, etc)";
        internal const string BaseOperatorPrefabName = "BaseOperator";
        internal const string BaseOperatorKitClassID = "BaseOperator_Kit";
        internal const string BaseOperatorTabID = "BO";

        internal const string HoverLiftPadClassID = "HoverLiftPad";
        internal const string HoverLiftPadFriendly = "Alterra Hover Lift Pad";
        internal const string HoverLiftPadDescription = "Get from one elevation to the next with ease. Een your PRAWN suit along so you don’t get lonely.";
        internal const string HoverLiftPrefabName = "HoverLiftPad";
        internal static string HoverLiftPadKitClassID = $"{HoverLiftPadClassID}_Kit";
        internal const string HoverLiftPadTabID = "HLP";

        internal const string SmartPlanterPotClassID = "SmartPlanterPot";
        internal const string SmartPlanterPotFriendly = "Smart Planter Pot";
        internal static string SmartPlanterPotDescription { get; } = "All drinks are on the house. (Drinks not included)";
        internal const string SmartPlanterPotPrefabName = "SmartPlanterPot";
        internal static string SmartPlanterPotKitClassID = $"{SmartPlanterPotClassID}_Kit";
        internal const string SmartPlanterPotTabID = "SMP";
        
        internal const string MiniFountainFilterClassID = "MiniFountainFilter";
        internal const string MiniFountainFilterFriendly = "Mini Filter & Fountain";
        internal const string MiniFountainFilterDescription = "A smaller water filtration system for your base or cyclops.";
        internal const string MiniFountainFilterPrefabName = "MiniFountainFilter";
        internal static string MiniFountainFilterKitClassID = $"{MiniFountainFilterClassID}_Kit";
        internal const string MiniFountainFilterTabID = "MFF";

        internal const string SeaBreezeClassID = "Seabreeze";
        internal const string SeaBreezeFriendly = "Seabreeze";
        internal const string SeaBreezeDescription = "Refrigeration unit for perishable food and cool, refreshing water.";
        internal const string SeaBreezePrefabName = "SeaBreezeFCS32";
        internal static string SeaBreezeKitClassID = $"{SeaBreezeClassID}_Kit";
        internal const string SeaBreezeTabID = "SB";

        internal const string QuantumTeleporterClassID = "QuantumTeleporter";
        internal const string QuantumTeleporterFriendly = "Quantum Teleporter";
        internal const string QuantumTeleporterDescription = "Teleport to other Quantum Teleporter units inside your base or to an entirely different base.";
        internal const string QuantumTeleporterPrefabName = "QuantumTeleporter";
        internal static string QuantumTeleporterKitClassID = $"{QuantumTeleporterClassID}_Kit";
        internal const string QuantumTeleporterTabID = "QT";        
        
        internal const string AlienChefClassID = "AlienChef";
        internal const string AlienChefFriendly = "Alien Chef";
        internal const string AlienChefDescription = "Forget the Fabricator: let the Alien Chef do the cooking and curing for you. 200 cooked fish for a party? No problem!";
        internal const string AlienChefPrefabName = "AlienChef";
        internal static string AlienChiefKitClassID = $"{AlienChefClassID}_Kit";
        internal const string AlienChiefTabID = "AC";

        internal const string Cabinet1ClassID = "CabinetWide";
        internal const string Cabinet1Friendly = "Wide Floor Cabinet";
        internal const string Cabinet1Description = "A stylish furniture piece for storage and decoration";
        internal const string Cabinet1PrefabName = "Cabinet_01";
        internal static string Cabinet1KitClassID = $"{Cabinet1ClassID}_Kit";
        internal const string CabinetTabID = "CB";

        internal const string Cabinet2ClassID = "CabinetMediumTall";
        internal const string Cabinet2Friendly = "Medium Vertical Cabinet";
        internal const string Cabinet2Description = "A stylish furniture piece for storage and decoration";
        internal const string Cabinet2PrefabName = "Cabinet_02";
        internal static string Cabinet2KitClassID = $"{Cabinet2ClassID}_Kit";


        internal const string Cabinet3ClassID = "CabinetTall";
        internal const string Cabinet3Friendly = "Tall Vertical Cabinet";
        internal const string Cabinet3Description = "A stylish furniture piece for storage and decoration";
        internal const string Cabinet3PrefabName = "Cabinet_03";
        internal static string Cabinet3KitClassID = $"{Cabinet3ClassID}_Kit";



        internal const string PaintCanClassID = "PaintCan";
        internal const string PaintCanFriendly = "Paint Can";
        internal const string PaintCanDescription = "Plug into your Paint Tool to color FCStudios Appliances, Furniture, Machines, and LED Lights";
        internal const string PaintCanPrefabName = "PaintCan";

        internal const string EmptyObservationTankClassID = "EmptyObservationTank";
        internal const string EmptyObservationTankFriendly = "Observation Tank";
        internal const string EmptyObservationTankDescription = "A tank with something to observe. A perfect object for Tables, Cabinets, and Shelves of all kinds.";
        internal const string EmptyObservationTankPrefabName = "ObservationTank";
        public static string EmptyObservationTankKitClassID = $"{EmptyObservationTankClassID}_kit";

        internal const string AlterraMiniBathroomClassID = "AlterraMiniBathroom";
        internal const string AlterraMiniBathroomFriendly = "Alterra Mini Bathroom";
        internal const string AlterraMiniBathroomDescription = "A lavatory and shower for convenient hygiene.";
        internal const string AlterraMiniBathroomPrefabName = "AlterraMiniBathroom";
        public static string AlterraMiniBathroomKitClassID = $"{AlterraMiniBathroomClassID}_kit";


        internal static readonly Dictionary<TechType, TechType> CuredCreatureList = new Dictionary<TechType, TechType>(TechTypeExtensions.sTechTypeComparer)
        {
            {
                TechType.Bladderfish,
                TechType.CuredBladderfish
            },
            {
                TechType.Boomerang,
                TechType.CuredBoomerang
            },
            {
                TechType.LavaBoomerang,
                TechType.CuredLavaBoomerang
            },
            {
                TechType.Eyeye,
                TechType.CuredEyeye
            },
            {
                TechType.LavaEyeye,
                TechType.CuredLavaEyeye
            },
            {
                TechType.GarryFish,
                TechType.CuredGarryFish
            },
            {
                TechType.HoleFish,
                TechType.CuredHoleFish
            },
            {
                TechType.Hoopfish,
                TechType.CuredHoopfish
            },
            {
                TechType.Hoverfish,
                TechType.CuredHoverfish
            },
            {
                TechType.Oculus,
                TechType.CuredOculus
            },
            {
                TechType.Peeper,
                TechType.CuredPeeper
            },
            {
                TechType.Reginald,
                TechType.CuredReginald
            },
            {
                TechType.Spadefish,
                TechType.CuredSpadefish
            },
            {
                TechType.Spinefish,
                TechType.CuredSpinefish
            }
        };

#if SUBNAUTICA
        internal static TechData EmptyObservationTankIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData EmptyObservationTankIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(EmptyObservationTankKitClassID.ToTechType(), 1)
            }
        };

#if SUBNAUTICA
        internal static TechData LedLightStickLongIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData LedLightStickLongIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("LedLightStickLong_Kit".ToTechType(), 1)
            }
        };

#if SUBNAUTICA
        internal static TechData LedLightStickShortIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData LedLightStickShortIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("LedLightStickShort_Kit".ToTechType(), 1)
            }
        };


#if SUBNAUTICA
        internal static TechData LedLightStickWallIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData LedLightStickWallIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("LedLightStickWall_Kit".ToTechType(), 1)
            }
        };

#if SUBNAUTICA
        internal static TechData MiniFountainFilterKitIngredients => new TechData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Bleach, 2),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#elif BELOWZERO
        internal static RecipeData MiniFountainFilterKitIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Bleach, 2),
                new Ingredient(TechType.FiberMesh, 2),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Glass, 1)
            }
        };
#endif
        

#if SUBNAUTICA
        internal static TechData PaintToolIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData PaintToolIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(PaintToolKitClassID.ToTechType(), 1),
            }
        };


#if SUBNAUTICA
        internal static TechData BaseOperatorIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData BaseOperatorIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(BaseOperatorKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData CurtainIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData CurtainIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("Curtain_Kit".ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData HoverLiftPadIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData HoverLiftPadIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(HoverLiftPadKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData TrashReceptacleIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData TrashReceptacleIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TrashReceptacleKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData TrashRecyclerIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData TrashRecyclerIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(RecyclerKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData SmartPlanterIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData SmartPlanterIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(SmartPlanterPotKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData AlienChiefIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AlienChiefIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(AlienChiefKitClassID.ToTechType(), 1),
            }
        };


        internal static Action<SaveData> OnDataLoaded { get; set; }

        internal static string SaveDataFilename => $"{ModName}SaveData.json";

        public static TechType CurtainTechType { get; internal set; }
        
        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetAudioFolderPath()
        {
            return Path.Combine(GetAssetPath(), "Audio");
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                if (_registeredPaintTool != null)
                {
                    foreach (PaintToolController controller in _registeredPaintTool)
                    {
                        controller.Save(newSaveData, serializer);
                    }
                }

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    QuickLogger.Debug($"Attempting to save device: {controller.Value?.UnitID} | Package Valid: { controller.Value?.PackageId == ModName}");
                    
                    if (controller.Value != null && controller.Value.PackageId == ModName)
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

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static DecorationDataEntry GetDecorationDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DecorationEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DecorationDataEntry{ Id = id };
        }

        internal static PaintToolDataEntry GetPaintToolDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PaintToolEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PaintToolDataEntry() { Id = id };
        }

        internal static AlienChiefDataEntry GetAlienChiefSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlienChiefDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlienChiefDataEntry { Id = id };
        }

        internal static void RegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                QuickLogger.Debug($"Registered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (!_registeredPaintTool.Contains(paintTool))
            {
                _registeredPaintTool.Add(paintTool);
            }
                
        }

        internal static void UnRegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (_registeredPaintTool.Contains(paintTool))
            {
                QuickLogger.Debug($"UnRegistered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool.Remove(paintTool);
            }

        }

        internal static bool IsModPatched(string mod)
        {
            return FCSAlterraHubService.PublicAPI.IsModPatched(mod);
        }

        internal static PlanterDataEntry GetPlanterDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PlanterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PlanterDataEntry { Id = id };
        }

        internal static MiniFountainFilterDataEntry GetMiniFountainFilterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MiniFountainFilterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new MiniFountainFilterDataEntry { Id = id };
        }

        internal static SeaBreezeDataEntry GetSeabreezeSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.SeaBreezeDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new SeaBreezeDataEntry { Id = id };
        }

        internal static TrashRecyclerDataEntry GetTrashRecyclerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.TrashRecyclerEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new TrashRecyclerDataEntry() { Id = id };
        }

        internal static QuantumTeleporterDataEntry GetQuantumTeleporterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.QuantumTeleporterEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new QuantumTeleporterDataEntry() { Id = id };
        }

        internal static CurtainDataEntry GetCurtainDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.CurtainEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new CurtainDataEntry() { Id = id };
        }

        internal static BaseOperatorDataEntry GetBaseOperatorDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.BaseOperatorEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new BaseOperatorDataEntry() { Id = id };
        }

        public static CabinetDataEntry GetCabinetSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.CabinetDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new CabinetDataEntry() { Id = id };
        } 
        
        public static ObservationTankDataEntry GetObservationTankDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.ObservationTankDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new ObservationTankDataEntry() { Id = id };
        }

        public static LedLightDataEntry GetLedLightEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.LedLightDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new LedLightDataEntry() { Id = id };
        }
        
        public static FEXRDataEntry FEXREntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.FEXRDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new FEXRDataEntry() { Id = id };
        }

        public static SignDataEntry GetSignDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.SignDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new SignDataEntry() { Id = id };
        }        
        
        public static AlterraMiniBathroomDataEntry GetAlterraMiniBathroomSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.AlterraMiniBathroomEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new AlterraMiniBathroomDataEntry() { Id = id };
        }
    }
}
