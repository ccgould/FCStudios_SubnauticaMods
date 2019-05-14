using FCSCommon.Exceptions;
using FCSCommon.Helpers;
using FCSCommon.Utilities.Language;
using FCSTechWorkBench.Configuration;
using FCSTechWorkBench.Handlers.Language;
using FCSTechWorkBench.Logging;
using FCSTechWorkBench.Models.Prefabs;
using FCSTechWorkBench.Models.TechPreb;
using Harmony;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FCSTechWorkBench
{
    public class LoadItems : MonoBehaviour
    {

        #region Public Properties

        public static LoadItems Instance { get; } = new LoadItems();
        public static AssetBundle ASSETBUNDLE { get; set; }
        public static TechWorkBench FCSTECHWORKBENCH_PREFAB_OBJECT { get; set; }
        public static AIDeepDrillerBattery AIDEEPDRILLERBATTERY_PREFAB_OBJECT { get; set; }
        public static AIDeepDrillerSolar AIDEEPDRILLERSOLAR_PREFAB_OBJECT { get; set; }
        public static FCSTechWorkBenchModStrings FCSTechWorkBenchModStrings { get; private set; }
        public static GameObject FCSTechWorkBenchPrefab { get; set; }
        public static GameObject FCSAIDeepDrillerBatteryPrefab { get; set; }
        public static GameObject AIDeepDrillerSolarPrefab { get; set; }

        #endregion

        #region Internal Properties
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;
        private AssetBundle _bundle;

        #endregion

        #region Public Methods

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>

        public void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create($"com.FCStudios.{Information.ModName}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            //Load the language
            FCSTechWorkBenchModStrings = LoadLanguage<FCSTechWorkBenchModStrings>(Information.ModName) as FCSTechWorkBenchModStrings;

            LoadMaterialBundle();

            // == Get the Prefabs == //
            if (GetPrefabs())
            {

                // === Create the jetStream == //
                if (FCSTechWorkBenchModStrings != null)
                {
                    var techWorkBench = new TechWorkBench(Information.ModName, Information.ModFriendly,
                        FCSTechWorkBenchModStrings.Description);
                    techWorkBench.Register();
                    techWorkBench.Patch();
                    FCSTECHWORKBENCH_PREFAB_OBJECT = techWorkBench;

                    var aIDeepDrillerBattery = new AIDeepDrillerBattery(Information.AIDeepDrillerBatteryName, Information.AIDeepDrillerBatteryFriendly,
                        FCSTechWorkBenchModStrings.AIDeepDrillerBatteryDescription);
                    aIDeepDrillerBattery.Register();
                    AIDEEPDRILLERBATTERY_PREFAB_OBJECT = aIDeepDrillerBattery;

                    var aIDeepDrillerSolar = new AIDeepDrillerSolar(Information.AIDeepDrillerSolarName, Information.AIDeepDrillerSolarFriendly,
                        FCSTechWorkBenchModStrings.AIDeepDrillerSolarDescription);
                    aIDeepDrillerSolar.Register();
                    AIDEEPDRILLERSOLAR_PREFAB_OBJECT = aIDeepDrillerSolar;
                }
                else
                {
                    throw new PatchTerminatedException("Error loading a prefab");
                }
            }
            else
            {
                throw new PatchTerminatedException("Error Get Prefab failed");
            }

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the prefabs from the asset bundle
        /// </summary>
        /// <returns></returns>
        private bool GetPrefabs()
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "globalmaterials"));
            if (myLoadedAssetBundle == null)
            {
                Log.Info("Failed to load AssetBundle!");
            }
            else
            {
                GlobalBundle = myLoadedAssetBundle;
                var prefab = myLoadedAssetBundle.LoadAllAssets();
            }
            


            //var globalBundle = AssetHelper.Asset(Information.ModName, "globalmaterials");

            //if (globalBundle != null)
            //{
            //    GlobalBundle = globalBundle;
            //}
            //else
            //{
            //    return false;
            //}

            var assetBundle = AssetHelper.Asset(Information.ModName, Information.ModBundleName);

            if (assetBundle != null)
            {
                ASSETBUNDLE = assetBundle;
            }
            else
            {
                return false;
            }

            var fcsTechWorkBenchPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.ModBundleRoot);

            if (fcsTechWorkBenchPrefab != null)
            {
                FCSTechWorkBenchPrefab = fcsTechWorkBenchPrefab;
            }
            else
            {
                return false;
            }

            var fcsAiDeepDrillerBatteryPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.AIDeepDrillerBatteryName);

            if (fcsAiDeepDrillerBatteryPrefab != null)
            {
                FCSAIDeepDrillerBatteryPrefab = fcsAiDeepDrillerBatteryPrefab;
            }
            else
            {
                return false;
            }

            var fcsAiDeepDrillerSolarPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.AIDeepDrillerSolarName);

            if (fcsAiDeepDrillerSolarPrefab != null)
            {
                AIDeepDrillerSolarPrefab = fcsAiDeepDrillerSolarPrefab;
            }
            else
            {
                return false;
            }

            return true;
        }

        public AssetBundle GlobalBundle { get; set; }

        private void LoadMaterialBundle()
        {

            ////StartCoroutine(LoadMaterials());
            ////var fullPath = "file:///" + Path.Combine(Information.GetAssetPath(), "globalmaterials");
            //var fullPath = @"F:/Program Files/Epic Games/Subnautica/QMods/FCSTechWorkBench/Assets/globalmaterials";
            //Log.Info(fullPath);

            ////WWW www = new WWW(fullPath);

            //Log.Info("Made it");

            //if (www.error != null)
            //{
            //    throw new System.Exception($"Theres an error in: {www.error}");
            //}

            //if (www.assetBundle == null)
            //{
            //    Log.Error("Null");
            //}

            //_bundle = www.assetBundle;

            //_bundle.LoadAllAssets();

            //var path = Path.Combine(Information.MODFOLDERLOCATION, "globalmaterials");

            //Log.Info($"File Exist {path} = {File.Exists(path)}");

            

            //if (g == null)
            //{
            //    Log.Info("Null");
            //}
            //else
            //{
            //    g.LoadAllAssets();

            //    Log.Info($"Loading Materials was successful!");
            //}
        }

        private IEnumerator LoadMaterials()
        {
           
            yield return null;


        }

        private ModStrings LoadLanguage<T>(string mod) where T : ModStrings
        {
            //  == Load the language settings == //
            LanguageSystem.GetCurrentSystemLanguageInfo();
            var currentLang = LanguageSystem.CultureInfo.Name;

            Log.Info(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));


            var languages = LanguageSystem.LoadCurrentRegion<T>(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));

            Log.Info($"languages");

            if (languages != null)
            {
                Log.Info(languages.Count.ToString());

                var modStrings = languages.Single(x => x.Region.Equals(currentLang));
                
                if (modStrings != null)
                {
                    return modStrings;
                }

                Log.Info($"ModStrings returned null");
            }
            else
            {
                Log.Info($"Load Current Region returned null");
            }
            return null;
        }

        #endregion

    }
}
