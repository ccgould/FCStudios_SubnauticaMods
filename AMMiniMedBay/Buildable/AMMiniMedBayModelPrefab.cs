using AMMiniMedBay.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace AMMiniMedBay.Buildable
{
    internal partial class AMMiniMedBayBuildable
    {
        private GameObject _Prefab;

        private static AssetBundle _assetBundle;
        public static GameObject ColorItemPrefab { get; set; }

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

            _assetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(ClassID);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                _Prefab = prefab;

                //Lets apply the material shader
                ApplyShaders(prefab);

                QuickLogger.Debug($"{this.FriendlyName} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            var colorItemPrefab = assetBundle.LoadAsset<GameObject>("ColorItem");

            //If the prefab isn't null lets add the shader to the materials
            if (colorItemPrefab != null)
            {
                ColorItemPrefab = colorItemPrefab;

                QuickLogger.Debug($"{this.FriendlyName} Color Item Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Color Item Not Found!");
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
            MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, _assetBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, _assetBundle);
            #endregion

            #region FCS_SUBMods_GlobalDecals_2
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals_2", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals_2_Norm", "FCS_SUBMods_GlobalDecals_Norm", prefab, _assetBundle);
            #endregion

            #region AMMiniMedBay_BaseColor
            MaterialHelpers.ApplySpecShader("AMMiniMedBay_BaseColor", "AMMiniMedBay_Spec", prefab, 1, 0.5f, _assetBundle);
            MaterialHelpers.ApplyNormalShader("AMMiniMedBay_BaseColor", "AMMiniMedBay_Norm", prefab, _assetBundle);
            #endregion

        }
    }
}
