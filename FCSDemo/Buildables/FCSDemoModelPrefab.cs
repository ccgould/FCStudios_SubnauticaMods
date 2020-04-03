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
        internal static string BodyMaterial => $"{Mod.ClassID}_COL";
        internal static string SpecTexture => $"{Mod.ClassID}_COL_SPEC";
        internal static string LUMTexture => $"{Mod.ClassID}_COL_LUM";
        internal static string ColorIDTexture => $"{Mod.ClassID}_COL_ID";
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
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, Color.white, Color.white, prefab, bundle); //Use color2 
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyGlassShaderTemplate(prefab.FindChild("model").FindChild("HydroponicHarvesterGlass"),Mod.ClassID);
            #endregion
        }
    }
}
