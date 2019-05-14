using FCS_Alterra_Refrigeration_Solutions.Configuration;
using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Managers;
using FCS_Alterra_Refrigeration_Solutions.Models.Prefabs;
using FCS_Alterra_Refrigeration_Solutions.Models.Prefabs.Filters;
using FCSCommon.Exceptions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities.Language;
using Harmony;
using SMLHelper.V2.Assets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FCS_Alterra_Refrigeration_Solutions
{
    public static class LoadItems
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        /// <summary>
        /// The prefab for the FCS Server Rack
        /// </summary>
        public static GameObject FcsARSolutionPrefab { get; set; }

        public static ARSolutionsSeaBreeze ARSolutions_SEA_BREEZE_PREFAB_OBJECT { get; set; }

        public static GameObject FcsARSolutionItemPrefab { get; set; }

        public static List<TechType> AllowedItems { get; set; }

        public static List<EatableEntities> EatableEnties { get; set; }

        public static AssetBundle ASSETBUNDLE { get; set; }

        public static GameObject LongTermFilterPrefab { get; set; }

        public static GameObject ShortTermFilterPrefab { get; set; }

        public static Dictionary<TechType, ModPrefab> AllowedFilters { get; set; } = new Dictionary<TechType, ModPrefab>();

        public static TechType ShortTermFilterTechType { get; set; }
        public static TechType LongTermFilterTechType { get; set; }
        public static ModStrings ModStrings { get; private set; }

        /// <summary>
        /// Patches the new objects into the game
        /// </summary>
        public static void Patch()
        {
            //Log.Info(GameObjectExtensions.GetFoodValue("WorldEntities/Doodads/Coral_reef/Coral_reef_jelly_plant_01_01"));

            // 1) INITIALIZE HARMONY
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSAlterraRefrigerationSolutions");

            // == Get the Prefabs == //
            if (GetPrefabs())
            {

                // === Create the Sea breeze Prefab == //
                var arSolutionsSeaBreeze = new ARSolutionsSeaBreeze(Information.ModName, Information.ModFriendly, Information.ModDescription);
                arSolutionsSeaBreeze.RegisterItem();
                arSolutionsSeaBreeze.Patch();
                ARSolutions_SEA_BREEZE_PREFAB_OBJECT = arSolutionsSeaBreeze;

                Log.Info($"//  ======================{FcsARSolutionPrefab.name}======================  //");


                var gameObj = FcsARSolutionPrefab.FindChild("model").FindChild("FilterHolderDoor");

                foreach (Transform renderer in gameObj.transform)
                {
                    Renderer sRenderers = renderer.GetComponent<Renderer>();
                    sRenderers.enabled = false;
                    Log.Info($"Disabled {renderer.name} render");
                }

                var shortTermFilter = new ShortTermFilter("ShortTermFilter", "Short Term Filter",
                    "This is a filter for use in the Alterra Refrigeration Solutions Mod. This filter lasts for two Planet 4546B days.");
                shortTermFilter.RegisterItem();
                shortTermFilter.Patch();

                AddTechTypes(shortTermFilter);

                var longTermFilter = new LongTermFilter("LongTermFilter", "Long Term Filter",
                    "This is a filter for use in the Alterra Refrigeration Solutions Mod. This filter lasts for thirty Planet 4546B days.");
                longTermFilter.RegisterItem();
                longTermFilter.Patch();

                AddTechTypes(longTermFilter);

                Log.Info("//  =========== Allowed Tech Types =========== //");

                foreach (var item in AllowedFilters)
                {
                    Log.Info(item.ToString());
                }
                Log.Info("//  =========== Allowed Tech Types =========== //");

                //Load Langauge
            }

            else
            {
                throw new PatchTerminatedException("Error loading finding a prefab");
            }

            Log.Info("Loading Allowed TechTypes");
            AllowedItems = FileManager.LoadFoodTypes("seaBreeze.json");
            EatableEnties = FileManager.LoadEatableEntities("FoodValues.json");
            Log.Info($"Eatable Entries Count: {EatableEnties.Count}");

            Log.Info($"// ******* Transform Data ********* //");

            foreach (Transform transform in FcsARSolutionPrefab.transform)
            {
                Log.Info(transform.name);
            }
            Log.Info($"// ******* Transform Data ********* //");

        }

        private static void AddTechTypes(ModPrefab obj)
        {
            var gameObject = obj.GetGameObject();

            // Set the TechType value on the TechTag
            TechTag tag = gameObject.GetComponent<TechTag>();

            AllowedFilters.Add(tag.type, obj);
        }

        /// <summary>
        /// Loads the prefabs from the asset bundle
        /// </summary>
        /// <returns></returns>
        private static bool GetPrefabs()
        {
            // == Get the ARSolution prefab == //

            var assetBundle = AssetHelper.Asset(Information.ModName, "fcsarsolutionsbundle");

            if (assetBundle != null)
            {
                ASSETBUNDLE = assetBundle;
            }
            else
            {
                return false;
            }

            var arSolutionsSeaBreezePrefab = ASSETBUNDLE.LoadAsset<GameObject>("ARSolutions").FindChild("SeaBreeze");

            if (arSolutionsSeaBreezePrefab != null)
            {
                FcsARSolutionPrefab = arSolutionsSeaBreezePrefab;
            }
            else
            {
                return false;
            }

            var arSolutionsSeaBreezeItemPrefab = ASSETBUNDLE.LoadAsset<GameObject>("ARSItem");

            if (arSolutionsSeaBreezeItemPrefab != null)
            {
                FcsARSolutionItemPrefab = arSolutionsSeaBreezeItemPrefab;
            }
            else
            {
                return false;
            }

            var longTermFilter = ASSETBUNDLE.LoadAsset<GameObject>("LongTerm_Filter");

            if (longTermFilter != null)
            {
                LongTermFilterPrefab = longTermFilter;
            }
            else
            {
                return false;
            }

            var shortTermFilter = ASSETBUNDLE.LoadAsset<GameObject>("ShortTerm_Filter");

            if (shortTermFilter != null)
            {
                ShortTermFilterPrefab = shortTermFilter;
            }
            else
            {
                return false;
            }
            return true;
        }

        private static void LoadLanguage()
        {
            //  == Load the language settings == //
            LanguageSystem.GetCurrentSystemLanguageInfo();
            var currentLang = LanguageSystem.CultureInfo.Name;

            //TODO Locate Language dic its not set
            var languages = LanguageSystem.LoadCurrentRegion<ModStrings>(Path.Combine(Information.LANGUAGEDIRECTORY, "languages.json"));

            var _modStrings = languages.Single(x => x.Region.Equals(currentLang));

            if (_modStrings != null)
            {
                ModStrings = _modStrings;
            }
            else
            {
                ModStrings = new ARModStrings();
                ModStrings.LoadDefault();
                Log.Error($"Language {currentLang} not found in the languages.json");
            }
        }
    }


}
