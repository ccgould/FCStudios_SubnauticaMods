using System;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using FCSTechFabricator.Extensions;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal static class FCSDemoModel
    {
        private static bool _init;
        private static AssetBundle _assetBundle;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SpecTexture => $"{Mod.ModName}_COL_SPEC";
        internal static string LUMTexture => $"{Mod.ModName}_COL_LUM";
        internal static string ColorIDTexture => $"{Mod.ModName}_COL_ID";
        internal static string DecalMaterial => $"{Mod.ModName}_COL_Decals";
        public static GameObject GetPrefabs(string prefabName)
        {
            try
            {
                QuickLogger.Info($"Trying to find prefab ID: {prefabName}");
                if (!_init)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs"); 
                    _assetBundle = AssetHelper.Asset(Mod.ModName, Mod.BundleName);
                    _init = true;
                }
                
                
                //We have found the asset bundle and now we are going to continue by looking for the model.
                GameObject prefab = _assetBundle?.LoadAsset<GameObject>(prefabName);



                //If the prefab isn't null lets add the shader to the materials
                if (prefab != null)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, _assetBundle);
                    QuickLogger.Debug($"{prefabName} Found!");
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
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, QPatch.Configuration.Config.BodyColor.Vector4ToColor(), Color.white, prefab, bundle); //Use color2 
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion
        }
    }
}
