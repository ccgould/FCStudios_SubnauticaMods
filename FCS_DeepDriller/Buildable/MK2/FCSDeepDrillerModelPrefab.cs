using FCS_DeepDriller.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using UnityEngine;

namespace FCS_DeepDriller.Buildable.MK1
{
    internal partial class FCSDeepDrillerBuildable
    {
        private GameObject _prefab;
        internal static string BodyMaterial => $"{Mod.MaterialBaseName}_COL";
        internal static string DecalMaterial => $"{Mod.MaterialBaseName}_Decals";
        internal static string SpecTexture => $"{Mod.MaterialBaseName}_S";
        internal static string NormTexture => $"{Mod.MaterialBaseName}_N";
        internal static string LUMTexture => $"{Mod.MaterialBaseName}_E";
        internal static string ColorIDTexture => $"{Mod.MaterialBaseName}_ID";

        public static AssetBundle AssetBundle { get; private set; }

        public static GameObject ItemPrefab { get; set; }

        internal static GameObject SandPrefab { get; set; }
        public static GameObject ListItemPrefab { get; set; }
        public static GameObject ColorItemPrefab { get; set; }
        public static GameObject ProgrammingItemPrefab { get; set; }

        private bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset(Mod.ModFolderName, Mod.BundleName);

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                return false;
            }

            AssetBundle = assetBundle;

            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>("AlterraDeepDriller");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                //Lets apply the material shader
                ApplyShaders(prefab);

                _prefab = prefab;
                
                QuickLogger.Debug($"{this.FriendlyName} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName} Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject itemButton = assetBundle.LoadAsset<GameObject>("ItemButton");

            //If the prefab isn't null lets add the shader to the materials
            if (itemButton != null)
            {
                ItemPrefab = itemButton;

                QuickLogger.Debug("Item Button Prefab Found!");
            }
            else
            {
                QuickLogger.Error("Item Button Prefab Not Found!");
                return false;
            }

            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject programmingItemButton = assetBundle.LoadAsset<GameObject>("ProgrammingItem");

            //If the prefab isn't null lets add the shader to the materials
            if (programmingItemButton != null)
            {
                ProgrammingItemPrefab = programmingItemButton;

                QuickLogger.Debug("Programming Item Button Prefab Found!");
            }
            else
            {
                QuickLogger.Error("Programming Item Button Prefab Not Found!");
                return false;
            }

            GameObject listItemButton = assetBundle.LoadAsset<GameObject>("TransferToggleButton");

            if (listItemButton != null)
            {
                ListItemPrefab = listItemButton;

                QuickLogger.Debug("List Item Button Prefab Found!");
            }
            else
            {
                QuickLogger.Error("Item Button Prefab Not Found!");
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

        private bool GetOres()
        {
            GameObject sand = AssetBundle.LoadAsset<GameObject>("Sand");

            //If the prefab isn't null lets add the shader to the materials
            if (sand != null)
            {
                SandPrefab = sand;

                //Lets apply the material shader
                ApplyShaders(SandPrefab);

                QuickLogger.Debug($"Sand Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"Sand Prefab Not Found!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab)
        {

            MaterialHelpers.ApplyColorMaskShader(BodyMaterial, ColorIDTexture, Color.white, DefaultConfigurations.DefaultColor, Color.white, prefab, AssetBundle); //Use color2 
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, AssetBundle);
            MaterialHelpers.ApplyEmissionShader(BodyMaterial, LUMTexture, prefab, AssetBundle, Color.white);
            MaterialHelpers.ApplyNormalShader(BodyMaterial, NormTexture, prefab, AssetBundle);


            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplySpecShader(DecalMaterial, SpecTexture, prefab, 1, 3f, AssetBundle);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, AssetBundle, Color.white);
            MaterialHelpers.ApplyNormalShader(DecalMaterial, NormTexture, prefab, AssetBundle);





            //MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", prefab, AssetBundle, Color.white);
            //MaterialHelpers.ApplyEmissionShader("Lava_Rock", "lava_rock_emission", prefab, AssetBundle, Color.white);
            //MaterialHelpers.ApplyNormalShader("Lava_Rock", "lava_rock_01_normal", prefab, AssetBundle);
        }
    }
}
