using AE.SeaCooker.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.SeaCooker.Buildable
{
    internal partial class SeaCookerBuildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab { get; set; }
        public static string BodyMaterial => "SeaCooker_MainBaseColor";

        public bool GetPrefabs()
        {

            QuickLogger.Debug($"AssetBundle Set");

            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModName, Mod.BundleName);

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(Mod.ClassID);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                _prefab = prefab;
                //Lets apply the material shader
                ApplyShaders(prefab, assetBundle);

                QuickLogger.Debug($"{this.FriendlyName} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Prefab Not Found!");
                return false;
            }

            GameObject color = assetBundle.LoadAsset<GameObject>("ColorItem");

            //If the prefab isn't null lets add the shader to the materials
            if (color != null)
            {
                ColorItemPrefab = color;

                QuickLogger.Debug($"ColorItem Prefab Found!");
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
        private void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region SystemLights_BaseColor
            MaterialHelpers.ApplyEmissionShader("SeaCooker_BaseColor", "SeaCooker_Emissive", prefab, QPatch.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SeaCooker_BaseColor", "SeaCooker_Normal", prefab, QPatch.GlobalBundle);
            MaterialHelpers.ApplySpecShader("SeaCooker_BaseColor", "SeaCooker_Spec", prefab, 1, 6f, QPatch.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SeaCooker_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplySpecShader(BodyMaterial, "SeaCooker_MainBaseColor_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, "SeaCooker_MainBaseColor_Normal", prefab, bundle);
            #endregion
        }
    }
}
