using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Data;
using FCSAlterraIndustrialSolutions.Handlers.Language;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSAlterraIndustrialSolutions.Models.Prefabs;
using FCSAlterraIndustrialSolutions.Patches;
using FCSCommon.Exceptions;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities.Language;
using Harmony;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions
{
    public class LoadItems : MonoBehaviour
    {
        #region Public Properties
        public static AssetBundle ASSETBUNDLE { get; set; }
        public static GameObject JetStreamT242Prefab { get; set; }
        public static GameObject DeepDrillerPrefab { get; set; }
        public static JetStreamT242 JETSTREAMT242_PREFAB_OBJECT { get; set; }
        public static MarineTurbinesMonitor MONITOR_PREFAB_OBJECT { get; set; }
        public static DeepDriller DEEP_DRILLER_PREFAB_OBJECT { get; set; }
        public static GameObject MarineTurbinesMonitorPrefab { get; set; }
        public static GameObject TurbineItemPrefab { get; set; }
        public static Cfg JetStreamT242Config { get; set; }
        public static DeepDrillerCfg DeepDrillConfig { get; set; }
        public static JetStreamT242ModStrings JetStreamT242ModStrings { get; private set; }
        public static MarineMonitorModStrings MarineMonitorModStrings { get; private set; }
        public static DeepDrillerModStrings DeepDrillerModStrings { get; private set; }
        public static GameObject AIItemContainerPrefab { get; set; }
        public static List<TechType> DeepDrillerAllowedModules { get; set; } = new List<TechType>();

        #endregion

        #region Internal Properties
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;
        #endregion

        #region Public Methods

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>

        public static void Patch()
        {
            //Set Rotation
            AISolutionsData.ChangeRotation();

            HarmonyInstance = HarmonyInstance.Create($"com.FCStudios.{Information.ModName}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            SolarPanel_Patcher.AddEventHandlerIfMissing(AlertSolorPanelAwake);
            //BuilderToolConstruct_Patcher.AddEventHandlerIfMissing(AlertedNewBuilderBuild);
            //PowerFXSetTarget_Patcher.AddEventHandlerIfMissing(AlertedSetTarget);

            //Load the language
            JetStreamT242ModStrings = LoadLanguage<JetStreamT242ModStrings>(Information.JetStreamTurbineName) as JetStreamT242ModStrings;
            MarineMonitorModStrings = LoadLanguage<MarineMonitorModStrings>(Information.MarineTurbinesMonitorName) as MarineMonitorModStrings;
            DeepDrillerModStrings = LoadLanguage<DeepDrillerModStrings>(Information.DeepDrillerName) as DeepDrillerModStrings;

            bool FCSTechWorkBench = TechTypeHandler.ModdedTechTypeExists("FCSTechWorkBench");

            if (!FCSTechWorkBench)
            {
                Log.Error($"Mod FCS WorkBench is needed!");
                //throw new PatchTerminatedException("Error finding FCS Work Bench");
            }

            if (!FindAllowedModules())
            {
                Log.Error($"Allowed modules not found!");

                //throw new PatchTerminatedException("Error finding all allow modules in FCS Work Bench");

            }

            //Load Configuration
            bool loaded = LoadConfigs();

            if (!loaded)
            {
                throw new PatchTerminatedException("Error loading Configuration");
            }

            var item = DeepDrillConfig.BiomeOres["safeshallows"];
            var techType = item[1].ToTechType();

            Log.Info($"Found TechType: {techType}");

            // == Get the Prefabs == //
            if (GetPrefabs())
            {
                // === Create the jetStream == //
                if (JetStreamT242ModStrings != null)
                {
                    var jetStream = new JetStreamT242(Information.JetStreamTurbineName, Information.JetStreamTurbineFriendly,
                        JetStreamT242ModStrings.Description);
                    jetStream.Register();
                    jetStream.Patch();
                    JETSTREAMT242_PREFAB_OBJECT = jetStream;
                }
                else
                {
                    throw new PatchTerminatedException("Error JetStreamT242ModStrings is null");
                }

                // === Create the MarineTurbinesMonitor == //
                if (MarineMonitorModStrings != null)
                {
                    var monitor = new MarineTurbinesMonitor(Information.MarineTurbinesMonitorName, Information.MarineTurbinesMonitorFriendly,
                        MarineMonitorModStrings.Description);
                    monitor.Register();
                    monitor.Patch();
                    MONITOR_PREFAB_OBJECT = monitor;


                }
                else
                {
                    throw new PatchTerminatedException("Error MarineMonitorModStrings is null");
                }

                // === Create the DeepDriller == //
                if (DeepDrillerModStrings != null)
                {
                    var driller = new DeepDriller(Information.DeepDrillerName, Information.DeepDrillerFriendly,
                        DeepDrillerModStrings.Description);
                    driller.Register();
                    driller.Patch();
                    DEEP_DRILLER_PREFAB_OBJECT = driller;
                }
                else
                {
                    throw new PatchTerminatedException("Error DeepDrillerModStrings is null");
                }

                //var file = Path.Combine(Information.LANGUAGEDIRECTORY, $"{Information.MarineTurbinesMonitorName}.json");
                //var file = Information.GetConfigFile(Information.DeepDrillerName);

                //var data = JsonConvert.SerializeObject(new DeepDrillerCfg(), Formatting.Indented);

                //File.WriteAllText(file, data);
            }
            else
            {
                throw new PatchTerminatedException("Error loading finding a prefab");
            }
        }

        private static void AlertSolorPanelAwake(SolarPanel obj)
        {
            Log.Info($"Solar Panel Animation Curve:");

            if (obj != null)
            {
                foreach (var key in obj.depthCurve.keys)
                {
                    Log.Info("// ======================================== //");
                    Log.Info(key.inTangent.ToString());
                    Log.Info(key.outTangent.ToString());
                    Log.Info(key.tangentMode.ToString());
                    Log.Info(key.time.ToString());
                    Log.Info(key.value.ToString());
                    Log.Info("// ======================================== //");
                }
            }
        }

        private static void AlertedSetTarget(PowerFX arg1, GameObject arg2)
        {
            if (arg1 != null)
            {
                Log.Info($"Name {arg1.gameObject.name}");

                if (arg2 != null)
                {
                    Log.Info($"Name {arg2.name}");
                }
            }
        }

        private static void AlertedNewBuilderBuild(BuilderTool arg1, Constructable arg2)
        {
            if (arg2 != null)
            {
                Log.Info($"Name: {arg2.gameObject.name}");

                var powerFX = arg2.GetComponent(typeof(PowerFX));

                if (powerFX != null)
                {
                    var fx = powerFX as PowerFX;
                    Log.Info($"PowerFX: {fx.vfxPrefab.name}");
                }

                //var list = arg2.GetComponentsInChildren(typeof(Component));
                //for (int i = 0; i < list.Length; i++)
                //{
                //    Log.Info($"Component Name: {list[i].name}");
                //}
            }
        }

        private static bool FindAllowedModules()
        {
            var aIDeepDrillerBattery = TechTypeHandler.TryGetModdedTechType("AIDeepDrillerBattery", out TechType aiDeepDrillerBattery);
            if (aIDeepDrillerBattery)
            {
                DeepDrillerAllowedModules.Add(aiDeepDrillerBattery);
            }
            else
            {
                Log.Error($"Deep Driller Battery not found!");
                return false;
            }

            var aIDeepDrillerSolar = TechTypeHandler.TryGetModdedTechType("AIDeepDrillerSolar", out TechType aiDeepDrillerSolar);
            if (aIDeepDrillerSolar)
            {
                DeepDrillerAllowedModules.Add(aiDeepDrillerSolar);
            }
            else
            {
                Log.Error($"Deep Driller Solar not found!");
                return false;
            }

            return true;
        }

        private static bool LoadConfigs()
        {
            if (File.Exists(Information.GetConfigFile(Information.JetStreamTurbineName)))
            {
                string savedDataJson = File.ReadAllText(Information.GetConfigFile(Information.JetStreamTurbineName)).Trim();

                //LoadData
                JetStreamT242Config = JsonConvert.DeserializeObject<Cfg>(savedDataJson);

                Log.Info($"JetStreamT242Config: {JetStreamT242Config.MaxCapacity}");
            }
            else
            {
                Log.Error($"{Information.GetConfigFile(Information.JetStreamTurbineName)} doesn't exist");
                return false;
            }

            if (File.Exists(Information.GetConfigFile(Information.DeepDrillerName)))
            {
                string savedDataJson = File.ReadAllText(Information.GetConfigFile(Information.DeepDrillerName)).Trim();

                //LoadData
                DeepDrillConfig = JsonConvert.DeserializeObject<DeepDrillerCfg>(savedDataJson);

                Log.Info($"DeepDrillConfig Biome Ores: {DeepDrillConfig.BiomeOres.Count}");
            }
            else
            {
                Log.Error($"{Information.GetConfigFile(Information.DeepDrillerName)} doesn't exist");
                return false;
            }
            return true;
        }

        public static bool ApplyDamageMaterials(AssetBundle bundle, Transform trans)
        {
            try
            {
                Log.Info($"Apply Damage Materials");
                List<object> objects = new List<object>(bundle.LoadAllAssets(typeof(object)));
                Log.Info($"Object Count {objects.Count}");

                for (int i = 0; i < objects.Count; i++)
                {
                    Log.Info($"{objects[i]}");

                    if (objects[i] is Material)
                    {
                        var obj = objects[i] as Material;

                        Log.Info($"Material: {obj.name}");
                    }

                    if (objects[i] is GameObject)
                    {
                        var objGame = objects[i] as GameObject;
                        Log.Info($"GameObject: {objGame.transform.childCount}");

                        foreach (var child in objGame.GetComponentsInChildren<Renderer>())
                        {
                            foreach (var childMaterial in child.materials)
                            {
                                Log.Info($"Material: {childMaterial.name}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
            return true;
        }

        public static void CleanOldSaveData()
        {
            try
            {
                var turbines = FindObjectsOfType<JetStreamT242Controller>();

                var turbineIDs = GetTurbineIds(turbines);

                Log.Info($"turbineIDs Count: {turbineIDs.Count()}");

                var savesFolderFiles = GetSaveFiles(Information.ModName).ToList();

                Log.Info($"savesFolderFiles Count: {savesFolderFiles.Count()}");

                savesFolderFiles.RemoveAll(c => turbineIDs.ToList().Exists(n => c.Contains(n)));
                //savesFolderFiles.RemoveAll(c => turbineIDs.ToList().Exists(n => n.Contains(Path.GetFileNameWithoutExtension(c))));

                foreach (var file in savesFolderFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the prefabs from the asset bundle
        /// </summary>
        /// <returns></returns>
        private static bool GetPrefabs()
        {

            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "globalmaterials"));
            if (myLoadedAssetBundle != null)
            {
                GlobalBundle = myLoadedAssetBundle;
            }
            else
            {
                return false;
            }

            var assetBundle = AssetHelper.Asset(Information.ModName, Information.ModBundleName);

            if (assetBundle != null)
            {
                ASSETBUNDLE = assetBundle;
            }
            else
            {
                return false;
            }

            var aiSolutionsJetStreamPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.ModBundleRoot).FindChild(Information.JetStreamTurbineGameObjectName);

            if (aiSolutionsJetStreamPrefab != null)
            {
                JetStreamT242Prefab = aiSolutionsJetStreamPrefab;
            }
            else
            {
                return false;
            }

            var marineTurbinesMonitorPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.ModBundleRoot).FindChild(Information.MarineTurbinesMonitorGameObjectName);

            if (marineTurbinesMonitorPrefab != null)
            {
                MarineTurbinesMonitorPrefab = marineTurbinesMonitorPrefab;
            }
            else
            {
                return false;
            }

            var deepDrillerPrefab = ASSETBUNDLE.LoadAsset<GameObject>(Information.ModBundleRoot).FindChild(Information.DeepDrillerGameObjectName);

            if (deepDrillerPrefab != null)
            {
                Log.Info("Found Driller Prefab");
                DeepDrillerPrefab = deepDrillerPrefab;
            }
            else
            {
                return false;
            }

            var turbineItemPrefab = ASSETBUNDLE.LoadAsset<GameObject>("TurbineItem");

            if (turbineItemPrefab != null)
            {
                Log.Info("Found TurbineItem Prefab");
                TurbineItemPrefab = turbineItemPrefab;
            }
            else
            {
                return false;
            }

            //Resources.Load<GameObject>("Submarine/Build/PowerTransmitter");
            var xPowerConnectionPrefab = ASSETBUNDLE.LoadAsset<GameObject>("xPowerConnection");

            if (xPowerConnectionPrefab != null)
            {
                Log.Info("Found xPowerConnection Prefab");
                XPowerConnectionPrefab = xPowerConnectionPrefab;
                var lineRenderer = XPowerConnectionPrefab.AddComponent<LineRenderer>();
                lineRenderer.textureMode = LineTextureMode.Stretch;
                lineRenderer.material = MaterialHelpers.FindMaterial("FCSLineRender", ASSETBUNDLE);
                lineRenderer.startColor = new Color(0.08235294f, 1f, 1f);
                lineRenderer.endColor = new Color(0.08235294f, 1f, 1f);
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
            }
            else
            {
                return false;
            }

            return true;
        }

        public static GameObject XPowerConnectionPrefab { get; set; }

        public static AssetBundle GlobalBundle { get; set; }

        private static string[] GetSaveFiles(string modName)
        {
            return Directory.GetFiles(Information.GetSaveFileDirectory(), "*.json");
        }

        private static IEnumerable<string> GetTurbineIds(JetStreamT242Controller[] turbines)
        {
            foreach (JetStreamT242Controller jetStreamT242Controller in turbines)
            {
                yield return jetStreamT242Controller.ID;
            }
        }

        private static ModStrings LoadLanguage<T>(string mod) where T : ModStrings
        {
            //  == Load the language settings == //
            LanguageSystem.GetCurrentSystemLanguageInfo();
            var currentLang = LanguageSystem.CultureInfo.Name;

            Log.Info(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));


            var languages = LanguageSystem.LoadCurrentRegion<T>(Path.Combine(Information.LANGUAGEDIRECTORY, $"{mod}.json"));


            if (languages != null)
            {
                Log.Info(languages.Count.ToString());

                var modStrings = languages.Single(x => x.Region.Equals(currentLang));

                if (modStrings != null)
                {
                    return modStrings;
                }
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
