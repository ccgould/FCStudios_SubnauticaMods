using System;
using FCS_AlterraHub.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Buildables
{
    internal partial class AlterraHub
    {
        private static bool _initialized;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => $"fcs{Mod.ModName}_COL";
        internal static string DecalMaterial => $"fcs{Mod.ModName}_DECALS";
        internal static string DetailsMaterial => $"fcs{Mod.ModName}_DETAILS";
        internal static string SpecTexture => $"fcs{Mod.ModName}_S";
        internal static string LUMTexture => $"fcs{Mod.ModName}_E";
        internal static string NormalTexture => $"fcs{Mod.ModName}_N";
        internal static string DetailTexture => $"fcs{Mod.ModName}_D";
        internal static GameObject OilPrefab { get; set; }
        internal static GameObject OreConsumerPrefab { get; set; }
        internal static GameObject AlterraHubPrefab { get; private set; }
        internal static GameObject DebitCardPrefab { get; set; }
        internal static GameObject BioFuelPrefab { get; set; }
        internal static GameObject KitPrefab { get; set; }
        internal static GameObject CartItemPrefab { get; set; }

        public static GameObject ColorPickerDialogPrefab { get; set; }

        public static bool GetPrefabs()
        {
            try
            {
                if (!_initialized)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs");

                    if (!LoadAsset("TestItem", QPatch.GlobalBundle, out var colorPickerCanvasGo)) return false;
                    ColorPickerDialogPrefab = colorPickerCanvasGo;

                    if (!LoadAsset(Mod.ModPrefabName, QPatch.GlobalBundle, out var prefabGo)) return false;
                    AlterraHubPrefab = prefabGo;

                    if (!LoadAsset(Mod.CardPrefabName, QPatch.GlobalBundle,out var cardPrefabGo)) return false;
                    DebitCardPrefab = cardPrefabGo;

                    if (!LoadAsset(Mod.BioFuelPrefabName, QPatch.GlobalBundle, out var bioFuelPrefabGo)) return false;
                    BioFuelPrefab = bioFuelPrefabGo;

                    if (!LoadAsset(Mod.KitPrefabName, QPatch.GlobalBundle, out var kitPrefabGo)) return false; 
                    KitPrefab = kitPrefabGo;

                    if (!LoadAsset(Mod.OreConsumerPrefabName, QPatch.GlobalBundle, out var oreConsumerPrefabGo)) return false;
                    OreConsumerPrefab = oreConsumerPrefabGo;
                    
                    if (!LoadAsset("CartItem", QPatch.GlobalBundle, out var cartItemPrefabGo)) return false;
                    CartItemPrefab = cartItemPrefabGo;

                    if (!LoadAsset("StoreItem", QPatch.GlobalBundle, out var itemPrefabGo)) return false;
                    ItemPrefab = itemPrefabGo;

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
        
        private static bool LoadAsset(string prefabName,AssetBundle assetBundle,out GameObject go,bool applyShaders = true)
        {
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            
            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, assetBundle);
                }

                go = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle = null)
        {
            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DetailsMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(DetailsMaterial, prefab);
            #endregion
        }
    }
}
