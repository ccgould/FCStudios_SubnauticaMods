using System;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Buildable
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        internal static string BodyMaterial { get; } = $"{Mod.ModPackID}_COL";
        internal static string SecondaryMaterial { get; } = $"{Mod.ModPackID}_COL_S";
        internal static string DecalMaterial { get; } = $"{Mod.ModPackID}_DECALS";
        internal static string DetailsMaterial { get; } = $"{Mod.ModPackID}_DETAILS";
        internal static string SpecTexture { get; } = $"{Mod.ModPackID}_S";
        internal static string LUMTexture { get; } = $"{Mod.ModPackID}_E";
        internal static string NormalTexture { get; } = $"{Mod.ModPackID}_N";
        internal static string DetailTexture => $"{Mod.ModPackID}_D";
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        public static string EmissiveBControllerMaterial { get;} = $"{Mod.ModPackID}_B_Controller";
        public static string EmissiveControllerMaterial { get; } = $"{Mod.ModPackID}_E_Controller";
        public static GameObject EnergyPillVendingMachinePrefab { get; set; }
        public static GameObject RedEnergyPillPrefab { get; set; }

        public static GameObject BlueEnergyPillPrefab { get; set; }

        public static GameObject GreenEnergyPillPrefab { get; set; }
        public static GameObject PillHudPrefab { get; set; }
        public static GameObject MiniMedBayPrefab { get; set; }
        public static GameObject BaseUtilityUnitPrefab { get; set; }
        public static GameObject BaseOxygenTankPrefab { get; set; }


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

            EnergyPillVendingMachinePrefab = GetPrefab(Mod.EnergyPillVendingMachinePrefabName);
            BaseOxygenTankPrefab = GetPrefab(Mod.BaseOxygenTankPrefabName);
            RedEnergyPillPrefab = GetPrefab("RedPill");
            GreenEnergyPillPrefab = GetPrefab("GreenPill");
            BlueEnergyPillPrefab = GetPrefab("BluePill");
            PillHudPrefab = GetPrefab("PillHUD");
            MiniMedBayPrefab = GetPrefab(Mod.MiniMedBayPrefabName);
            BaseUtilityUnitPrefab = GetPrefab(Mod.BaseUtilityUnityPrefabName);
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
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    AlterraHub.ReplaceShadersV2(prefab);
                }

                go = prefab;

                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");
            go = null;
            return false;
        }
    }
}
