using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal partial class FCSDemoBuidable : Buildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ClassID}_M_COL";
        internal static string DetailMaterial => $"{Mod.ClassID}_D_COL";
        internal static string LightsMaterial => $"{Mod.ClassID}Lights_D_COL";
        public bool GetPrefabs()
        {

            try
            {
                QuickLogger.Debug($"AssetBundle Set");

                QuickLogger.Debug("GetPrefabs");
                AssetBundle assetBundle = AssetHelper.Asset(Mod.ModName, Mod.BundleName);

                //We have found the asset bundle and now we are going to continue by looking for the model.
                GameObject prefab = assetBundle.LoadAsset<GameObject>(Mod.PrefabName);

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

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDemoBuidable>(e.Message);
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
            MaterialHelpers.ApplySpecShader(BodyMaterial, $"{Mod.ClassID}_M_COL_SPEC", prefab, 1, 3f, bundle);
            #endregion

            MaterialHelpers.ApplyEmissionShader(DetailMaterial, $"{Mod.ClassID}_D_COL_LUM", prefab, bundle, Color.white);
            MaterialHelpers.ApplySpecShader(DetailMaterial, $"{Mod.ClassID}_D_COL_SPEC", prefab, 1, 6f, bundle);
            MaterialHelpers.ApplyAlphaShader(DetailMaterial, prefab);
        }
    }
}
