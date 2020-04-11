using System;
using FCS_HydroponicHarvesters.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Buildables
{
    internal static class HydroponicHarvestersModelPrefab
    {
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SpecTexture => $"{Mod.ModName}_COL_SPEC";
        internal static string LUMTexture => $"{Mod.ModName}_COL_LUM";
        internal static string ColorIDTexture => $"{Mod.ModName}_COL_ID";
        internal static GameObject SmallPrefab { get; private set; }
        internal static GameObject MediumPrefab { get; private set; }
        internal static GameObject LargePrefab { get; private set; }
        public static AssetBundle Bundle { get; set; }

        public static bool GetPrefabs()
        {
            try
            {
                QuickLogger.Debug($"AssetBundle Set");

                QuickLogger.Debug("GetPrefabs");
                AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

                Bundle = assetBundle;

                //We have found the asset bundle and now we are going to continue by looking for the model.
                GameObject lPrefab = assetBundle.LoadAsset<GameObject>(Mod.LargePrefabName);
                GameObject mPrefab = assetBundle.LoadAsset<GameObject>(Mod.MediumPrefabName);
                GameObject sPrefab = assetBundle.LoadAsset<GameObject>(Mod.SmallPrefabName);

                //If the prefab isn't null lets add the shader to the materials
                if (lPrefab != null)
                {
                    //Lets apply the material shader
                    ApplyShaders(lPrefab, assetBundle);

                    LargePrefab = lPrefab;
                    QuickLogger.Debug($"Large Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Large Prefab Not Found!");
                    return false;
                }

                if (mPrefab != null)
                {
                    //Lets apply the material shader
                    ApplyShaders(mPrefab, assetBundle);

                    MediumPrefab = mPrefab;

                    QuickLogger.Debug($"Medium Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Medium Prefab Not Found!");
                    return false;
                }

                if (sPrefab != null)
                {
                    //Lets apply the material shader
                    ApplyShaders(sPrefab, assetBundle);
                    
                    SmallPrefab = sPrefab;


                    QuickLogger.Debug($"Small Prefab Found!");
                }
                else
                {
                    QuickLogger.Error($"Small Prefab Not Found!");
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

                GameObject item = assetBundle.LoadAsset<GameObject>("HydroHarvItem");

                if (item != null)
                {
                    ItemPrefab = item;
                }
                else
                {
                    QuickLogger.Error($"HydroHarvItem Not Found!");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error<HydroponicHarvestersBuildable>(e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private static void ApplyShaders(GameObject prefab, AssetBundle bundle)
        {
            #region BaseColor
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, "HydroponicHarvester_COL_ID", Color.white, Color.red, Color.white, prefab, bundle); //Use color2 
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
            MaterialHelpers.ApplyAlphaShader(BodyMaterial, prefab);
            #endregion
        }
    }
}
