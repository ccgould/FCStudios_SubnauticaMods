using System;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal static class FCSDemoModel
    {
        private static bool _init;
        private static AssetBundle _assetBundle;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SecondaryMaterial => $"{Mod.ModName}_COL_S";
        internal static string DecalMaterial => $"{Mod.ModName}_DECALS";
        internal static string DetailsMaterial => $"{Mod.ModName}_DETAILS";
        internal static string SpecTexture => $"{Mod.ModName}_S";
        internal static string LUMTexture => $"{Mod.ModName}_E";
        internal static string NormalTexture => $"{Mod.ModName}_N";
        internal static string DetailTexture => $"{Mod.ModName}_D";
        public static string EmissiveBControllerMaterial { get; } = $"{Mod.ModName}_B_Controller";
        public static string EmissiveControllerMaterial { get; } = $"{Mod.ModName}_E_Controller";

        public static GameObject GetPrefabs(string prefabName)
        {
            try
            {
                QuickLogger.Info($"Trying to find prefab ID: {prefabName}");
                if (!_init)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs"); 
                    _assetBundle = AssetHelper.Asset(Mod.BundleName);
                    _init = true;
                }
                
                
                //We have found the asset bundle and now we are going to continue by looking for the model.
                GameObject prefab = _assetBundle?.LoadAsset<GameObject>(prefabName);



                //If the prefab isn't null lets add the shader to the materials
                if (prefab != null)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, _assetBundle);
                    QuickLogger.Info($"{prefabName} Found!");
                    return prefab;

                }
                else
                {
                    QuickLogger.Error($"{prefabName} Not Found!");
                    return null;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDemoBuidable>(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private static void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DetailsMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(EmissiveControllerMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion
        }
    }
}
