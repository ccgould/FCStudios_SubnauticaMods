using FCSCommon.Helpers;
using UnityEngine;

namespace FCSTechFabricator.Models
{
    internal static class Shaders
    {
        internal static void ApplyDeepDriller(GameObject prefab)
        {
            #region AMMiniMedBay_BaseColor
            MaterialHelpers.ApplyAlphaShader("DeepDriller_BaseColor_BaseColor", prefab);
            MaterialHelpers.ApplySpecShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Spec", prefab, 1, 0.5f, QPatch.Bundle);
            MaterialHelpers.ApplyNormalShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplyEmissionShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("Lava_Rock", "lava_rock_emission", prefab, QPatch.Bundle, Color.white);
            MaterialHelpers.ApplyNormalShader("Lava_Rock", "lava_rock_01_normal", prefab, QPatch.Bundle);
            #endregion
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyKitShaders(GameObject prefab)
        {
            MaterialHelpers.ApplyEmissionShader("UnitContainerKit_BaseColor", "UnitContainerKit_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyAlphaShader("UnitContainerKit_BaseColor", prefab);
            MaterialHelpers.ApplyNormalShader("UnitContainerKit_BaseColor", "UnitContainerKit_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplySpecShader("UnitContainerKit_BaseColor", "UnitContainerKit_Spec", prefab, 1f, 6f, QPatch.Bundle);
        }

        internal static void ApplySeaTankShaders(GameObject prefab)
        {
            #region SystemLights_BaseColor
            MaterialHelpers.ApplyEmissionShader("SeaCooker_BaseColor", "SeaCooker_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SeaCooker_BaseColor", "SeaCooker_Normal", prefab, QPatch.Bundle);
            MaterialHelpers.ApplySpecShader("SeaCooker_BaseColor", "SeaCooker_Spec", prefab, 1, 6f, QPatch.Bundle);
            MaterialHelpers.ApplyAlphaShader("SeaCooker_BaseColor", prefab);
            #endregion
        }

        internal static void ApplyFreonShaders(GameObject prefab)
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

            MaterialHelpers.ApplyNormalShader("Freon_Bottle", "Freon_Freon_Bottle_Normal", prefab, QPatch.Bundle);
            MaterialHelpers.ApplySpecShader("Freon_Bottle", "Freon_Freon_Bottle_Specular", prefab, 1f, 4f, QPatch.Bundle);
        }
    }
}
