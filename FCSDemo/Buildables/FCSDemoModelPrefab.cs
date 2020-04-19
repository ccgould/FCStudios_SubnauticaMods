using System;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using FCSTechFabricator.Extensions;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal partial class FCSDemoBuidable : Buildable
    {
        private static GameObject _prefab;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SpecTexture => $"{Mod.ModName}_COL_SPEC";
        internal static string LUMTexture => $"{Mod.ModName}_COL_LUM";
        internal static string ColorIDTexture => $"{Mod.ModName}_COL_ID";
        internal static string DecalMaterial => $"{Mod.ModName}_COL_Decals";
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
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, QPatch.Configuration.Config.BodyColor.Vector4ToColor(), Color.white, prefab, bundle); //Use color2 
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            //MaterialHelpers.ApplyGlassShaderTemplate(prefab.FindChild("model").FindChild("HydroponicHarvesterGlass"),Mod.ClassID);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion
        }
    }
}
