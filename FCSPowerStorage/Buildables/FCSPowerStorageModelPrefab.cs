using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using UnityEngine;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable
    {
        private GameObject _prefab;
        internal static AssetBundle AssetBundle;
        public static GameObject ColorItemPefab { get; set; }

        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset($"{Information.ModFolderName}", "fcspowerstorage-mod");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            AssetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(Information.PrefrabName);

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
            GameObject colorItem = assetBundle.LoadAsset<GameObject>("ColorItem");

            //If the prefab isn't null lets add the shader to the materials
            if (colorItem != null)
            {
                ColorItemPefab = colorItem;

                QuickLogger.Debug($"ColorItem Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"ColorItem Prefab Not Found!");
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
            //MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            //MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, _assetBundle);
            //MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            //MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            //MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, _assetBundle);
            #endregion

            MaterialHelpers.ApplySpecShader("Power_Storage_StorageBaseColor_Albedo", "Power_Storage_StorageBaseColor_MetallicSmoothness", prefab, 1, 8, AssetBundle);
            MaterialHelpers.ApplySpecShader("Power_Storage_Details_Albedo", "Power_Storage_Details_MetallicSmoothness", prefab, 1, 6, AssetBundle);

        }
    }
}
