using AE.HabitatSystemsPanel.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.HabitatSystemsPanel.Buildable
{
    internal partial class HSPBuildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ClassID}_MainBaseColor";
        internal static string DetailMaterial => $"{Mod.ClassID}_DetailBaseColor";

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

            return true;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region BaseColor
            MaterialHelpers.ApplyNormalShader(BodyMaterial, $"{Mod.ClassID}_MainBase_Normal", prefab, bundle);
            MaterialHelpers.ApplySpecShader(BodyMaterial, $"{Mod.ClassID}_MainBase_Spec", prefab, 1, 6f, bundle);
            #endregion

            MaterialHelpers.ApplyEmissionShader(DetailMaterial, $"{Mod.ClassID}_Detail_Emissive", prefab, bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplySpecShader(DetailMaterial, $"{Mod.ClassID}_DetailBase_Spec", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyAlphaShader(DetailMaterial, prefab);
        }
    }
}
