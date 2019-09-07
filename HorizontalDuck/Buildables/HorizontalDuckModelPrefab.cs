using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace HorizontalDuck.Buildables
{
    internal partial class HorizontalDuckBuildable
    {
        private GameObject _prefab;

        private static AssetBundle _assetBundle;

        private bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset("HorizontalDuck", "horizontalduckmodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            _assetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("HorizontalDuck");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                _prefab = prefab;

                //Lets apply the material shader
                ApplyShaders(prefab);

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
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab)
        {
            #region ExStorageDepotUnit_BaseColor
            MaterialHelpers.ApplyGlassShaderTemplate("Blank", prefab, 8);
            //MaterialHelpers.ApplyAlphaShader("ExStorageDepotUnit_BaseColor", prefab);
            //MaterialHelpers.ApplySpecShader("ExStorageDepotUnit_BaseColor", "ExStorageDepotUnit_Spec", prefab, 1, 0.5f, _assetBundle);
            //MaterialHelpers.ApplyNormalShader("ExStorageDepotUnit_BaseColor", "ExStorageDepotUnit_Norm", prefab, _assetBundle);
            //MaterialHelpers.ApplyEmissionShader("ExStorageDepotUnit_BaseColor", "DeepDriller_Emissive_On", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }

    }
}
