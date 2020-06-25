using System;
using FCS_HydroponicHarvesters.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Buildables
{
    internal static class HydroponicHarvestersModelPrefab
    {
        private static bool _initialized;
        private static AssetBundle _assetBundle;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string DecalMaterial => $"{Mod.ModName}_COL_Decals";
        internal static string SpecTexture => $"{Mod.ModName}_COL_SPEC";
        internal static string LUMTexture => $"{Mod.ModName}_COL_LUM";
        internal static string ColorIDTexture => $"{Mod.ModName}_COL_ID";
        internal static GameObject SmallPrefab { get; private set; }
        internal static GameObject MediumPrefab { get; private set; }
        internal static GameObject LargePrefab { get; private set; }
        internal static AssetBundle Bundle { get; set; }
        internal static GameObject BottlePrefab { get; set; }
        public static bool GetPrefabs()
        {
            try
            {
                if (!_initialized)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs");
                    _assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

                    Bundle = _assetBundle;

                    //We have found the asset bundle and now we are going to continue by looking for the model.
                    GameObject lPrefab = _assetBundle.LoadAsset<GameObject>(Mod.LargePrefabName);
                    GameObject mPrefab = _assetBundle.LoadAsset<GameObject>(Mod.MediumPrefabName);
                    GameObject sPrefab = _assetBundle.LoadAsset<GameObject>(Mod.SmallPrefabName);

                    //If the prefab isn't null lets add the shader to the materials
                    if (lPrefab != null)
                    {
                        //Lets apply the material shader
                        ApplyShaders(lPrefab, _assetBundle);

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
                        ApplyShaders(mPrefab, _assetBundle);

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
                        ApplyShaders(sPrefab, _assetBundle);

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

                    GameObject bottleModel = _assetBundle.LoadAsset<GameObject>("HydroponicHarvesterBottleModel");

                    if (bottleModel != null)
                    {
                        ApplyShaders(bottleModel, _assetBundle);
                        BottlePrefab = bottleModel;
                    }
                    else
                    {
                        QuickLogger.Error($"HydroponicHarvesterBottleModel Not Found!");
                        return false;
                    }

                    GameObject item = _assetBundle.LoadAsset<GameObject>("HydroHarvItem");

                    if (item != null)
                    {
                        ItemPrefab = item;
                    }
                    else
                    {
                        QuickLogger.Error($"HydroHarvItem Not Found!");
                        return false;
                    }

                    _initialized = true;
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
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle = null)
        {
            if (bundle == null)
            {
                bundle = _assetBundle;
            }

            #region BaseColor
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, DefaultConfigurations.DefaultColor, Color.white, prefab, bundle); //Use color2 
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            #endregion
        }
    }
}
