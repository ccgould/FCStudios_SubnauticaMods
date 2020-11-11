using System;
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

        internal const string HoverLiftPadClassID = "HoverLiftPad";
        internal const string HoverLiftPadFriendly = "Alterra Hover Lift Pad";
        internal const string HoverLiftPadDescription = "Get from one platform to the other with ease and why not bring your prawn suit with you";
        internal const string HoverLiftPrefabName = "HoverLiftPad";
        internal static string HoverLiftPadKitClassID = $"{HoverLiftPadClassID}_Kit";
        internal const string HoverLiftPadTabID = "HLP";

        internal const string SmartPlanterPotClassID = "SmartPlanterPot";
        internal const string SmartPlanterPotFriendly = "Smart Planter Pot";
        internal static string SmartPlanterPotDescription { get; } =
            "Just like another other planter but with color changing abilities.";
        internal const string SmartPlanterPotPrefabName = "SmartPlanterPot";
        internal static string SmartPlanterPotKitClassID = $"{SmartPlanterPotClassID}_Kit";
        internal const string SmartPlanterPotTabID = "SMP";

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


        internal static Action<SaveData> OnDataLoaded { get; set; }

        internal static string SaveDataFilename => $"{ModName}SaveData.json";

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

        private static void OnSaveComplete()
        {

        }
        
        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IFCSSave<SaveData>>();

                if (_registeredPaintTool != null)
                {
                    foreach (PaintToolController controller in _registeredPaintTool)
                    {
                        controller.Save(newSaveData);
                    }
                }

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData,serializer);
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

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        public static DecorationDataEntry GetDecorationDataEntrySaveData(string id)
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

        public static bool IsModPatched(string mod)
        {
            return FCSAlterraHubService.PublicAPI.IsModPatched(mod);
        }

        public static PlanterDataEntry GetPlanterDataEntrySaveData(string id)
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
    }
}
