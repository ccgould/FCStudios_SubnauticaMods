using System;
using System.Collections.Generic;
using AlterraGen.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using UnityEngine;

namespace AlterraGen.Buildables
{
    internal partial class AlterraGenBuildable
    {
        private static bool _initialized;
        private static AssetBundle _assetBundle;
        private IEnumerable<Vector3> _bubbleLocations = new []
        {
            new Vector3(), 
            new Vector3(), 
            new Vector3(), 
        };
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => Mod.ModName;
        internal static string SpecTexture => $"{Mod.ModName}_S";
        internal static string LUMTexture => $"{Mod.ModName}_E";
        internal static string DecalMaterial => $"{Mod.ModName}_Decals";
        internal static string ColorIDTexture => $"{Mod.ModName}_ID";
        internal static string NormalTexture => $"{Mod.ModName}_N";
        internal static GameObject Prefab { get; private set; }
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
                    GameObject prefab = _assetBundle.LoadAsset<GameObject>(Mod.ModPrefabName);


                    //If the prefab isn't null lets add the shader to the materials
                    if (prefab != null)
                    {
                        //Lets apply the material shader
                        ApplyShaders(prefab, _assetBundle);

                        Prefab = prefab;
                        QuickLogger.Debug($"Alterra Gen Prefab Found!");
                    }
                    else
                    {
                        QuickLogger.Error($"Alterra Gen Prefab Not Found!");
                        return false;
                    }

                    GameObject itemPrefab = _assetBundle.LoadAsset<GameObject>("ItemButton");
                    if (itemPrefab != null)
                    {
                        ItemPrefab = itemPrefab;
                        QuickLogger.Debug($"Item Button Found!");
                    }
                    else
                    {
                        QuickLogger.Error($"Item Button Prefab Not Found!");
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

                   
                    _initialized = true;
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
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
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, NormalTexture,prefab,bundle);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, DefaultConfigurations.DefaultColor, Color.white, prefab, bundle); //Use color2 

            #endregion
        }
    }
}
