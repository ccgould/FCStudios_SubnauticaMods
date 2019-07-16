using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTesting.Buildables
{
    internal partial class CyclopsBioReactorBuildable
    {
        private GameObject _prefab;
        private static AssetBundle _assetBundle;

        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset("FCSTesting", "cyclopsbioreactormodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            _assetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("CyclopsBioreactor");

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
        private void ApplyShaders(GameObject prefab)
        {

            #region SystemLights_BaseColor
            MaterialHelpers.ApplyNormalShader("Bioreactor_Base", "Bioreactor_Bioreactor_Normal", prefab, _assetBundle);
            MaterialHelpers.ApplyEmissionShader("Bioreactor_Base", "Bioreactor_Bioreactor_Emissive", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyAlphaShader("Bioreactor_Base", prefab);
            MaterialHelpers.ApplySpecShader("Bioreactor_Base", "Bioreactor_Bioreactor_SpecularGloss", prefab, 1, 6f, _assetBundle);
            #endregion
        }
    }
}
