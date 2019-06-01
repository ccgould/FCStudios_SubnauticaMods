using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechWorkBench.Mono
{
    internal partial class ShortTermFilterBuildable
    {
        private GameObject _prefab;

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Bundle.LoadAsset<GameObject>("ShortTerm_Filter");

            //If the prefab isn't null lets add the shader to the materials
            if (_prefab != null)
            {

                //Lets apply the material shader
                ApplyShaders(_prefab);

                QuickLogger.Debug($"{this.FriendlyName_I} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName_I} Prefab Not Found!");
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
            MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, QPatch.Bundle);
            #endregion
        }

    }
}