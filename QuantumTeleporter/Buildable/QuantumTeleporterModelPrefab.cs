using System;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using QuantumTeleporter.Configuration;
using UnityEngine;

namespace QuantumTeleporter.Buildable
{
    internal partial class QuantumTeleporterBuildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ClassID}_M_COL";
        internal static string DetailMaterial => $"{Mod.ClassID}_D_COL";
        internal static string LightsMaterial => $"{Mod.ClassID}Lights_D_COL";
        internal static GameObject ItemPrefab { get; set; }
        public bool GetPrefabs()
        {

            try
            {
                QuickLogger.Debug($"AssetBundle Set");

                QuickLogger.Debug("GetPrefabs");
                AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

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

                GameObject colorItem = QPatch.GlobalBundle.LoadAsset<GameObject>("ColorItem");

                if (colorItem != null)
                {
                    ColorItemPrefab = colorItem;
                }
                else
                {
                    QuickLogger.Error($"Color Item Not Found!");
                    return false;
                }

                GameObject itemPrefab = assetBundle.LoadAsset<GameObject>("BaseTeleportItem");
                if (itemPrefab != null)
                {
                    ItemPrefab = itemPrefab;

                    QuickLogger.Debug($"Teleport Item Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Teleport Item Prefab Not Found!");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error<QuantumTeleporterBuildable>(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region BaseColor
            MaterialHelpers.ApplyNormalShader(BodyMaterial, $"{Mod.ClassID}_M_COL_NORM", prefab, bundle);
            MaterialHelpers.ApplySpecShader(BodyMaterial, $"{Mod.ClassID}_M_COL_SPEC", prefab, 1, 3f, bundle);
            #endregion

            MaterialHelpers.ApplyEmissionShader(DetailMaterial, $"{Mod.ClassID}_D_COL_LUM", prefab, bundle, Color.white);
            MaterialHelpers.ApplySpecShader(DetailMaterial, $"{Mod.ClassID}_D_COL_SPEC", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyAlphaShader(DetailMaterial, prefab);

            MaterialHelpers.ApplyEmissionShader(LightsMaterial, $"{LightsMaterial}_D_COL_LUM", prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(LightsMaterial, prefab);
        }
    }
}
