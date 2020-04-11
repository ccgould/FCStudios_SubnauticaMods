using FCSCommon.Helpers;
using FCSCommon.Utilities;
using MAC.OxStation.Config;
using UnityEngine;

namespace MAC.OxStation.Buildables
{
    internal static class OxStationModelPrefab
    {
        internal static GameObject OxstationPrefab;
        internal static GameObject OxstationScreenPrefab;
        private static bool _initialized;
        internal static GameObject ItemPrefab;

        /// <summary>
        /// Gets the prefabs from the bundle
        /// </summary>
        /// <returns></returns>
        internal static bool GetPrefabs()
        {
            if (!_initialized)
            {
                QuickLogger.Debug("GetPrefabs");
                AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

                //If the result is null return false.
                if (assetBundle == null)
                {
                    QuickLogger.Error($"AssetBundle is Null!");
                    return false;
                }

                QuickLogger.Debug($"AssetBundle Set");

                //We have found the asset bundle and now we are going to continue by looking for the model.
                GameObject prefab = assetBundle.LoadAsset<GameObject>(Mod.ModName);
                GameObject screenPrefab = assetBundle.LoadAsset<GameObject>(Mod.ModScreenName);

                //If the prefab isn't null lets add the shader to the materials
                if (prefab != null)
                {
                    ApplyShaders(prefab, assetBundle);
                    OxstationPrefab = prefab;

                    QuickLogger.Debug($"Oxstation Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Oxstation Prefab Not Found!");
                    return false;
                }

                //If the prefab isn't null lets add the shader to the materials
                if (screenPrefab != null)
                {
                    ApplyShaders(screenPrefab, assetBundle);
                    OxstationScreenPrefab = screenPrefab;

                    QuickLogger.Debug($"Oxstation Screen Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Oxstation Screen Prefab Not Found!");
                    return false;
                }

                GameObject item = assetBundle.LoadAsset<GameObject>("OxStation_Item");

                if (item != null)
                {
                    ItemPrefab = item;
                }
                else
                {
                    QuickLogger.Error($"OxStation_Item Not Found!");
                    return false;
                }

                _initialized = true;
            }
            
            return true;
        }

        /// <summary>
        /// Applies the shader needed for the materials to a prefab
        /// </summary>
        /// <param name="prefab">The prefab to apply the shaders to.</param>
        /// <param name="bundle">The bundle to search in for the needed textures</param>
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region Oxstation
            MaterialHelpers.ApplyAlphaShader("OxStation_BaseColor", prefab);
            MaterialHelpers.ApplySpecShader("OxStation_BaseColor", "OxStation_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyNormalShader("OxStation_BaseColor", "OxStation_Normal", prefab, bundle);
            MaterialHelpers.ApplyEmissionShader("OxStation_BaseColor", "OxStation_Emissive", prefab, bundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }
    }
}
