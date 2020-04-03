using FCSCommon.Helpers;
using FCSCommon.Utilities;
using MAC.OxStation.Config;
using UnityEngine;

namespace MAC.OxStation.Buildables
{
    internal partial class OxStationBuildable
    {
        private GameObject _prefab;

        /// <summary>
        /// Gets the prefabs from the bundle
        /// </summary>
        /// <returns></returns>
        private bool GetPrefabs()
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

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                ApplyShaders(prefab, assetBundle);
                _prefab = prefab;

                QuickLogger.Debug($"{this.FriendlyName} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Prefab Not Found!");
                return false;
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
            #region fire_extinguisher_01_holder
            MaterialHelpers.ApplyAlphaShader("OxStation_BaseColor", prefab);
            MaterialHelpers.ApplySpecShader("OxStation_BaseColor", "OxStation_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyNormalShader("OxStation_BaseColor", "OxStation_Normal", prefab, bundle);
            MaterialHelpers.ApplyEmissionShader("OxStation_BaseColor", "OxStation_Emissive", prefab, bundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }
    }
}
