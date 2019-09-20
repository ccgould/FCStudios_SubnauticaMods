using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AIMarineTurbine.Buildable
{
    internal partial class AIMarineMonitorBuildable
    {
        private GameObject _prefab;
        public static GameObject TurbineItemPrefab { get; private set; }
        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = QPatch.Bundle.LoadAsset<GameObject>("MarineTurbinesMonitor");//MarineTurbinesMonitor //AlterraTurbines_Monitor

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

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject turbineItemPrefab = QPatch.Bundle.LoadAsset<GameObject>("TurbineItem");

            //If the prefab isn't null lets add the shader to the materials
            if (turbineItemPrefab != null)
            {
                TurbineItemPrefab = turbineItemPrefab;

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
            MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region SystemLights_BaseColor
            //MaterialHelpers.ApplyEmissionShader("FCS_MarineTurbine_Tex_DefaultState", "JetStreamT242_MarineTurbineMat_Emission", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            //MaterialHelpers.ApplyMetallicShader("FCS_TurbinesMonitor", "JetStreamT242_MarineTurbineMat_MetallicSmoothness", prefab, _assetBundle, 0.2f);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, QPatch.Bundle);
            #endregion
        }
    }
}
