using FCS_DeepDriller.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Buildable
{
    internal partial class FCSDeepDrillerBuildable
    {
        private GameObject _prefab;

        private static AssetBundle _assetBundle;
        internal static GameObject BatteryModule { get; private set; }

        private bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModName, Mod.BundleName);

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            _assetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject batteryModule = assetBundle.LoadAsset<GameObject>("Battery_Module_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (batteryModule != null)
            {
                BatteryModule = batteryModule;

                //Lets apply the material shader
                ApplyShaders(batteryModule);

                QuickLogger.Debug($"Battery Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Battery Module  Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("AlterraDeepDriller");

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

            return true;
        }



        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab)
        {
            #region AMMiniMedBay_BaseColor
            MaterialHelpers.ApplyAlphaShader("DeepDriller_BaseColor_BaseColor", prefab);
            MaterialHelpers.ApplySpecShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Spec", prefab, 1, 0.5f, QPatch.GlobalBundle);
            MaterialHelpers.ApplyNormalShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Norm", prefab, QPatch.GlobalBundle);
            MaterialHelpers.ApplyEmissionShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive", prefab, QPatch.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, QPatch.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            #endregion
        }
    }
}
