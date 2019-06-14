using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable
    {

        private AssetBundle _assetBundle;
        private GameObject _Prefab;

        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset($"FCS{ClassID}", "aipowercellsocketbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            _assetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("AIPowerCellSocket");

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

            #region PowercellSocket
            MaterialHelpers.ApplySpecShader("PowercellSocket", "PowercellSocket_Specular", prefab, 1, 6f, _assetBundle);
            MaterialHelpers.ApplyNormalShader("PowercellSocket", "PowercellSocket_Normal", prefab, _assetBundle);
            #endregion

            #region FCS_SUBMods_GlobalDecals_2
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals_2", prefab);
            #endregion

            #region Power Cell
            MaterialHelpers.ApplyEmissionShader("power_cell_01", "power_cell_01_illum", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("power_cell_01", "power_cell_01_normal", prefab, _assetBundle);
            #endregion

        }
    }
}
