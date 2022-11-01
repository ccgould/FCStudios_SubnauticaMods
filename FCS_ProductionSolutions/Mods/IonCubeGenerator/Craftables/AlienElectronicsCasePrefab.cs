﻿using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Craftables
{
    // using Logger = QModManager.Utility.Logger;

    internal partial class AlienEletronicsCase
    {
        private AssetBundle _assetBundle;
        private GameObject _alienElectronicsCasePrefab;

        internal bool GetPrefabs(AssetBundle assetBundle)
        {

            _alienElectronicsCasePrefab = ModelPrefab.GetPrefab("Precursor_WireKit");
            _assetBundle = ModelPrefab.ModBundle;
            if (_alienElectronicsCasePrefab == null)
            {
                QuickLogger.Error("Precursor_Ingot is Null!", false);
                return false;
            }
            ApplyShaders(this._alienElectronicsCasePrefab);
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

            #region BaseCol1
            MaterialHelpers.ApplyMetallicShader("BaseCol1", "BaseCol1_Metallic", prefab, _assetBundle, 0.2f);
            MaterialHelpers.ApplyNormalShader("BaseCol1", "BaseCol1_Norm", prefab, _assetBundle);
            #endregion

            #region BaseCol1_Dark
            MaterialHelpers.ApplyMetallicShader("BaseCol1_Dark", "BaseCol1_Metallic", prefab, _assetBundle, 0.2f);
            MaterialHelpers.ApplyNormalShader("BaseCol1_Dark", "BaseCol1_Norm", prefab, _assetBundle);
            #endregion
        }
    }
}
