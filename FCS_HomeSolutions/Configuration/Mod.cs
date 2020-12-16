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
        internal const string TrashReceptacleDescription = "Use the Trash Receptacle to quickly send you trash to the recycler form the inside of your base";
        internal const string TrashReceptaclePrefabName = "TrashReceptacle";
        internal const string TrashReceptacleKitClassID = "TrashReceptacle_Kit";
        internal const string TrashReceptacleTabID = "TR";

        internal const string PaintToolClassID = "PaintTool";
        internal const string PaintToolFriendly = "Alterra Paint Tool";
        internal const string PaintToolDescription = "Change the color of FCStudios mods";
        internal const string PaintToolPrefabName = "PaintTool";
        internal const string PaintToolKitClassID = "PaintTool_Kit";

        internal const string BaseOperatorClassID = "BaseOperator";
        internal const string BaseOperatorFriendly = "Alterra Base Operator";
        internal const string BaseOperatorDescription = "Control your base from one giant screen";
        internal const string BaseOperatorPrefabName = "BaseOperator";
        internal const string BaseOperatorKitClassID = "BaseOperator_Kit";
        internal const string BaseOperatorTabID = "BO";

        internal const string HoverLiftPadClassID = "HoverLiftPad";
        internal const string HoverLiftPadFriendly = "Alterra Hover Lift Pad";
        internal const string HoverLiftPadDescription = "Get from one platform to the other with ease and why not bring your prawn suit with you";
        internal const string HoverLiftPrefabName = "HoverLiftPad";
        internal static string HoverLiftPadKitClassID = $"{HoverLiftPadClassID}_Kit";
        internal const string HoverLiftPadTabID = "HLP";

        internal const string SmartPlanterPotClassID = "SmartPlanterPot";
        internal const string SmartPlanterPotFriendly = "Smart Planter Pot";
        internal static string SmartPlanterPotDescription { get; } = "Just like another other planter but with color changing abilities.";
        internal const string SmartPlanterPotPrefabName = "SmartPlanterPot";
        internal static string SmartPlanterPotKitClassID = $"{SmartPlanterPotClassID}_Kit";
        internal const string SmartPlanterPotTabID = "SMP";
        
        internal const string MiniFountainFilterClassID = "MiniFountainFilter";
        internal const string MiniFountainFilterFriendly = "Mini Fountain Filter";
        internal const string MiniFountainFilterDescription = "A smaller water filtration system for your base or cyclops.";
        internal const string MiniFountainFilterPrefabName = "MiniFountainFilter";
        internal static string MiniFountainFilterKitClassID = $"{MiniFountainFilterClassID}_Kit";
        internal const string MiniFountainFilterTabID = "MFF";

        internal const string SeaBreezeClassID = "Seabreeze";
        internal const string SeaBreezeFriendly = "Seabreeze";
        internal const string SeaBreezeDescription = "Alterra Refrigeration Sea Breeze will keep your items fresh longer!";
        internal const string SeaBreezePrefabName = "SeaBreezeFCS32";
        internal static string SeaBreezeKitClassID = $"{SeaBreezeClassID}_Kit";
        internal const string SeaBreezeTabID = "SB";

        internal const string QuantumTeleporterClassID = "QuantumTeleporter";
        internal const string QuantumTeleporterFriendly = "Quantum Teleporter";
        internal const string QuantumTeleporterDescription = "A teleporter that allows you to teleport from one base to another.";
        internal const string QuantumTeleporterPrefabName = "QuantumTeleporter";
        internal static string QuantumTeleporterKitClassID = $"{QuantumTeleporterClassID}_Kit";
        internal const string QuantumTeleporterTabID = "QT";        
        
        internal const string AlienChiefClassID = "AlienChief";
        internal const string AlienChiefFriendly = "Alien Chief";
        internal const string AlienChiefDescription = "An easy way to cook surplus of food";
        internal const string AlienChiefPrefabName = "AlienChief";
        internal static string AlienChiefKitClassID = $"{AlienChiefClassID}_Kit";
        internal const string AlienChiefTabID = "AC";

        internal const string PaintCanClassID = "PaintCan";
        internal const string PaintCanFriendly = "Paint Can";
        internal const string PaintCanDescription = "More paint for you paint tool. Use the paint can to allow you to paint devices with your paint tool";
        internal const string PaintCanPrefabName = "PaintCan";


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

        internal static HoverLiftDataEntry GetHoverLiftSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.HoverLiftDataEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new HoverLiftDataEntry { Id = id };
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
    }
}
