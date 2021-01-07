using System;
using FCS_AlterraHub.API;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FCS_StorageSolutions.Mods.AlterraStorage.Buildable
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        public static GameObject DSSServerPrefab;
        public static GameObject DSSAvaliableVehiclesItemPrefab;
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
        public static GameObject AlterraStoragePrefab { get; set; }
        public static GameObject DSSItemDisplayPrefab { get; set; }
        public static GameObject DSSAntennaPrefab { get; set; }
        public static GameObject DSSWallServerRackPrefab { get; set; }
        public static GameObject DSSFloorServerRackPrefab { get; set; }
        public static GameObject DSSAutoCrafterPrefab { get; set; }
        public static GameObject DSSTerminalPrefab { get; set; }
        public static GameObject DSSFormattingStationPrefab { get; set; }
        public static GameObject DSSInventoryItemPrefab { get; set; }
        public static GameObject FilterItemPrefab { get; set; }
        public static GameObject DSSCrafterCratePrefab { get; set; }


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

            AlterraStoragePrefab = GetPrefab(Mod.AlterraStoragePrefabName);
            DSSServerPrefab = GetPrefab(Mod.DSSServerPrefabName);
            DSSFormattingStationPrefab = GetPrefab(Mod.DSSFormattingStationPrefabName);
            DSSTerminalPrefab = GetPrefab(Mod.DSSTerminalPrefabName);
            DSSAutoCrafterPrefab = GetPrefab(Mod.DSSAutoCrafterPrefabName);
            DSSWallServerRackPrefab = GetPrefab(Mod.DSSWallServerRackPrefabName);
            DSSFloorServerRackPrefab = GetPrefab(Mod.DSSFloorServerRackPrefabName);
            DSSAntennaPrefab = GetPrefab(Mod.DSSAntennaPrefabName);
            DSSItemDisplayPrefab = GetPrefab(Mod.DSSItemDisplayPrefabName);
            DSSInventoryItemPrefab = GetPrefab("DSSInventoryItem");
            DSSAvaliableVehiclesItemPrefab = GetPrefab("AvaliableVehiclesItem");
            DSSCrafterCratePrefab = GetPrefab("DSSCrafterCrate");
            FilterItemPrefab = GetPrefab("FilterItem");
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
            if (bundle == null)
            {
                bundle = ModBundle;
            }

            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DetailsMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(EmissionControllerMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion
        }
    }
}
