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
            MaterialHelpers.ApplyEmissionShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }
    }
}
