using AE.BaseTeleporter.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.BaseTeleporter.Buildables
{
    internal partial class BaseTeleporterBuildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab => FCSTechFabricator.QPatch.ColorItem;
        internal static GameObject ItemPrefab { get; set; }

        public static string BodyMaterial => $"{Mod.ModName}_M_COL";
        private static string DecalMaterial => $"{Mod.ModName}_D_COL";
        public bool GetPrefabs()
        {

            QuickLogger.Debug($"AssetBundle Set");

            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModDirectoryName, Mod.BundleName);

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(Mod.ModName);

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

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region SystemLights_BaseColor
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, $"{Mod.ModName}_D_COL_LUM", prefab, bundle, new Color(1, 1f, 1f));
            MaterialHelpers.ApplySpecShader(DecalMaterial, $"{Mod.ModName}_D_COL_SPEC", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion

            #region FCS_SUBMods_GlobalDecals
            MaterialHelpers.ApplySpecShader(BodyMaterial, $"{Mod.ModName}_M_COL_SPEC", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, $"{Mod.ModName}_M_COL_NORM", prefab, bundle);
            #endregion
        }
    }
}
