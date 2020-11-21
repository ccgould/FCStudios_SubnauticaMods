using System;
using FCS_AlterraHub.API;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FCS_ProductionSolutions.Buildable
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SecondaryMaterial => $"{Mod.ModName}_COL_S";
        internal static string DecalMaterial => $"{Mod.ModName}_DECALS";
        internal static string DetailsMaterial => $"{Mod.ModName}_DETAILS";
        internal static string EmissionControllerMaterial => $"{Mod.ModName}_E_Controller";
        internal static string SpecTexture => $"{Mod.ModName}_S";
        internal static string LUMTexture => $"{Mod.ModName}_E";
        internal static string NormalTexture => $"{Mod.ModName}_N";
        internal static string DetailTexture => $"{Mod.ModName}_D";
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        public static GameObject HydroponicHarvesterPrefab { get; set; }
        public static GameObject MatterAnalyzerPrefab { get; set; }
        public static GameObject DeepDrillerItemPrefab { get; set; }
        public static GameObject DeepDrillerPrefab { get; set; }
        internal static GameObject DeepDrillerSandPrefab { get; set; }
        public static GameObject DeepDrillerListItemPrefab { get; set; }
        public static GameObject DeepDrillerProgrammingItemPrefab { get; set; }

        internal static void Initialize()
        {
            if (GlobalBundle == null)
            {
                GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            }

            if (ModBundle == null)
            {
                ModBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.ModBundleName, Mod.GetModDirectory());
            }

            HydroponicHarvesterPrefab = GetPrefab(Mod.HydroponicHarvesterModPrefabName);
            MatterAnalyzerPrefab = GetPrefab(Mod.MatterAnalyzerPrefabName);
            DeepDrillerItemPrefab = GetPrefab("DeepDrillerItemButton");
            DeepDrillerPrefab = GetPrefab(Mod.DeepDrillerMk2PrefabName);
            DeepDrillerSandPrefab = GetPrefab("SandOreBag");
            DeepDrillerListItemPrefab = GetPrefab("DeepDrillerTransferToggleButton");
            DeepDrillerProgrammingItemPrefab = GetPrefab("DeepDrillerProgrammingItem");
        }

        internal static GameObject GetPrefab(string prefabName)
        {
            try
            {
                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (!LoadAsset(prefabName, ModBundle, out var prefabGo)) return null;
                return prefabGo;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        private static bool LoadAsset(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, assetBundle);
                    QuickLogger.Debug($"Applied shaderes to prefab {prefabName}");
                }

                go = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle = null)
        {
            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DetailsMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(EmissionControllerMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(DetailsMaterial, prefab);
            #endregion
        }
    }
}
