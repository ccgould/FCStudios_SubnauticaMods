using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class SolarAttachmentBuildable
    {
        private GameObject _prefab;

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Bundle.LoadAsset<GameObject>("solar_panel_module");

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
            #region DeepDriller_BaseColor_BaseColor
            MaterialHelpers.ApplyAlphaShader("DeepDriller_BaseColor_BaseColor", prefab);
            MaterialHelpers.ApplySpecShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Spec", prefab, 1, 0.5f, QPatch.Bundle);
            MaterialHelpers.ApplyNormalShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplyEmissionShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }
    }
}
