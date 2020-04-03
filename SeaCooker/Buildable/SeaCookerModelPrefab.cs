using AE.SeaCooker.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.SeaCooker.Buildable
{
    internal partial class SeaCookerBuildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab;
        internal static GameObject SeaBreezeItemPrefab { get; set; }

        public static string BodyMaterial => "SeaCooker_MainBaseColor";

        public bool GetPrefabs()
        {

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = QPatch.AssetBundle.LoadAsset<GameObject>(Mod.ClassID);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                _prefab = prefab;
                //Lets apply the material shader
                ApplyShaders(prefab, QPatch.AssetBundle);

                QuickLogger.Debug($"{this.FriendlyName} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Prefab Not Found!");
                return false;
            }

            GameObject seaBreezeItem = QPatch.AssetBundle.LoadAsset<GameObject>("SC_SeaBreezeItem");

            //If the prefab isn't null lets add the shader to the materials
            if (seaBreezeItem != null)
            {
                SeaBreezeItemPrefab = seaBreezeItem;

                QuickLogger.Debug($"SC_SeaBreezeItem Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"SC_SeaBreezeItem Prefab Not Found!");
                return false;
            }

            #region Color Item
            //We have found the asset bundle and now we are going to continue by looking for the model.
            ColorItemPrefab = QPatch.GlobalBundle.LoadAsset<GameObject>("ColorItem");

            //If the prefab isn't null lets add the shader to the materials
            if (ColorItemPrefab != null)
            {
                QuickLogger.Debug($"ColorItem Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region SystemLights_BaseColor
            MaterialHelpers.ApplyEmissionShader("SeaCooker_BaseColor", "SeaCooker_Emissive", prefab, bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SeaCooker_BaseColor", "SeaCooker_Normal", prefab, bundle);
            MaterialHelpers.ApplySpecShader("SeaCooker_BaseColor", "SeaCooker_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyAlphaShader("SeaCooker_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplySpecShader(BodyMaterial, "SeaCooker_MainBaseColor_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, "SeaCooker_MainBaseColor_Normal", prefab, bundle);
            #endregion
        }
    }
}
