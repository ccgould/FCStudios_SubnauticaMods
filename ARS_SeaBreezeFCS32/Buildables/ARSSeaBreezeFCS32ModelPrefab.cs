using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Buildables
{
    internal partial class ARSSeaBreezeFCS32Buildable
    {
        private GameObject _Prefab;
        public static GameObject ItemPrefab { get; set; }

        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");

            //If the result is null return false.
            if (QPatch.Bundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = QPatch.Bundle.LoadAsset<GameObject>("SeaBreezeFCS32");

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

            GameObject itemPrefab = QPatch.Bundle.LoadAsset<GameObject>("ARSItem");

            if (itemPrefab != null)
            {
                ItemPrefab = itemPrefab;

                QuickLogger.Debug($"{this.FriendlyName} ItemPrefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} ItemPrefab Not Found!");
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
            MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, QPatch.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, QPatch.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, QPatch.GlobalBundle);
            #endregion

            MaterialHelpers.ApplyNormalShader("SeaBreezeFCS32_BaseColor", "SeaBreezeFCS32_Norm", prefab, QPatch.GlobalBundle);

        }
    }
}
