using System;
using CyclopsUpgradeConsole.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace CyclopsUpgradeConsole.Buildables
{
    internal partial class CUCBuildable
    {
        private static bool _initialized;
        private static AssetBundle _assetBundle;
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => Mod.ModName;
        internal static string SpecTexture => $"{Mod.ModName}_S";
        internal static string LUMTexture => $"{Mod.ModName}_E";
        internal static string DecalMaterial => $"{Mod.ModName}_Decals";
        internal static string NormalTexture => $"{Mod.ModName}_N";
        internal static GameObject Prefab { get; private set; }
        internal static AssetBundle Bundle { get; set; }
        public static bool GetPrefabs()
        {
            try
            {
                if (!_initialized)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs");
                    _assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

                    Bundle = _assetBundle;

                    //We have found the asset bundle and now we are going to continue by looking for the model.
                    GameObject prefab = _assetBundle.LoadAsset<GameObject>(Mod.ModPrefabName);


                    //If the prefab isn't null lets add the shader to the materials
                    if (prefab != null)
                    {
                        //Lets apply the material shader
                        ApplyShaders(prefab, _assetBundle);

                        Prefab = prefab;
                        QuickLogger.Debug($"{Mod.ModFriendlyName} Prefab Found!");
                    }
                    else
                    {
                        QuickLogger.Error($"{Mod.ModFriendlyName} Gen Prefab Not Found!");
                        return false;
                    }

                   
                    _initialized = true;
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle = null)
        {
            if (bundle == null)
            {
                bundle = _assetBundle;
            }

            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, NormalTexture,prefab,bundle);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplySpecShader("engine_console_key", "engine_console_key_01_spec", prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader("engine_console_key", "engine_console_key_01_illum", prefab, bundle, Color.white);
            MaterialHelpers.ApplyNormalShader("engine_console_key", "engine_console_key_01_normal_N", prefab, bundle);

            #endregion
        }
    }
}
