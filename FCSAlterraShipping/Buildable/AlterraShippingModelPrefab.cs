﻿using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSAlterraShipping.Buildable
{
    internal partial class AlterraShippingBuildable
    {

        private AssetBundle _assetBundle;
        private GameObject _Prefab;
        public static GameObject ItemPrefab { get; private set; }
        public bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(this.ClassID, "alterrashippingmodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            _assetBundle = assetBundle;
            QuickLogger.Debug($"AssetBundle Set");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("AlterraShippingMod");

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

            var itemPrefab = assetBundle.LoadAsset<GameObject>("Item");

            //If the prefab isn't null lets add the shader to the materials
            if (itemPrefab != null)
            {
                ItemPrefab = itemPrefab;

                QuickLogger.Debug($"{this.FriendlyName} Item Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Item Prefab Not Found!");
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
            MaterialHelpers.ApplyEmissionShader("SystemLights_BaseColor", "SystemLights_OnMode_Emissive", prefab, _assetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_BaseColor", "SystemLights_Norm", prefab, _assetBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_BaseColor", prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", prefab);
            MaterialHelpers.ApplyNormalShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Norm", prefab, _assetBundle);
            #endregion

            #region AlterraShipping
            MaterialHelpers.ApplyMetallicShader("AlterraShipping", "AlterraShipping_Metallic", prefab, _assetBundle, 0.2f);
            MaterialHelpers.ApplyNormalShader("AlterraShipping", "AlterraShipping_Norm", prefab, _assetBundle);
            #endregion

        }
    }
}