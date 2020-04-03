using FCS_DeepDriller.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Buildable.MK1
{
    internal partial class FCSDeepDrillerBuildable
    {
        private GameObject _prefab;

        public static AssetBundle AssetBundle { get; private set; }

        internal static GameObject BatteryModule { get; private set; }

        public static GameObject FocusModule { get; set; }

        public static GameObject SolarModule { get; set; }

        public static GameObject ItemPrefab { get; set; }

        internal static GameObject SandPrefab { get; set; }

        private bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            AssetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

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

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject listItem = assetBundle.LoadAsset<GameObject>("ListButton");

            //If the prefab isn't null lets add the shader to the materials
            if (listItem != null)
            {
                ItemPrefab = listItem;

                QuickLogger.Debug("List item Prefab Found!");
            }
            else
            {
                QuickLogger.Error("List item Prefab Not Found!");
                return false;
            }

            #region Battery Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject batteryModule = assetBundle.LoadAsset<GameObject>("Battery_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (batteryModule != null)
            {
                ApplyShaders(batteryModule);

                BatteryModule = batteryModule;

                QuickLogger.Debug($"Battery Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Battery Module  Prefab Not Found!");
                return false;
            }
            #endregion

            #region Solar Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject solarModule = assetBundle.LoadAsset<GameObject>("Solar_Panel_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (solarModule != null)
            {
                ApplyShaders(solarModule);

                SolarModule = solarModule;

                QuickLogger.Debug($"Solar Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }
            #endregion

            #region Focus Module
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject focusModule = assetBundle.LoadAsset<GameObject>("Scanner_Screen_Attachment");

            //If the prefab isn't null lets add the shader to the materials
            if (focusModule != null)
            {
                ApplyShaders(focusModule);

                FocusModule = focusModule;

                QuickLogger.Debug($"Focus Module Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Solar Module  Prefab Not Found!");
                return false;
            }
            #endregion
            
            return true;
        }

        private bool GetOres()
        {
            GameObject sand = AssetBundle.LoadAsset<GameObject>("Sand");

            //If the prefab isn't null lets add the shader to the materials
            if (sand != null)
            {
                SandPrefab = sand;

                //Lets apply the material shader
                ApplyShaders(SandPrefab);

                QuickLogger.Debug($"Sand Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Sand Prefab Not Found!");
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
            MaterialHelpers.ApplySpecShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Spec", prefab, 1, 0.5f, AssetBundle);
            MaterialHelpers.ApplyNormalShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Norm", prefab, AssetBundle);
            MaterialHelpers.ApplyEmissionShader("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On", prefab, AssetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, AssetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyEmissionShader("Lava_Rock", "lava_rock_emission", prefab, AssetBundle, Color.white);
            MaterialHelpers.ApplyNormalShader("Lava_Rock", "lava_rock_01_normal", prefab, AssetBundle);
            #endregion
        }
    }
}
