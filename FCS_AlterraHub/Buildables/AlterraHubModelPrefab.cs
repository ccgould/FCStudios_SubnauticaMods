using System;
using System.Collections.Generic;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Buildables
{
    public partial class AlterraHub
    {
        private static bool _initialized;
        private static Dictionary<string,Material> _v2Materials = new Dictionary<string, Material>();
        private static bool _v2MaterialsLoaded;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }

        /// <summary>
        /// Material for fcs01_BP
        /// </summary>
        public const string BasePrimaryCol = "fcs01_BP";
        /// <summary>
        /// Material for fcs01_BS
        /// </summary>
        public const string BaseSecondaryCol = "fcs01_BS";
        /// <summary>
        /// Material for fcs01_BD_Exterior
        /// </summary>
        public const string BaseDecalsExterior = "fcs01_BD_Exterior";
        /// <summary>
        /// Material for fcs01_BTD
        /// </summary>
        public const string BaseTexDecals = "fcs01_BTD";
        /// <summary>
        /// Material for base lights emissive controller
        /// </summary>
        public const string BaseLightsEmissiveController = "fcs01_BLEC";
        /// <summary>
        /// Material for base decals emissive controller
        /// </summary>
        public const string BaseDecalsEmissiveController = "fcs01_BDEC";
        /// <summary>
        /// Material for indoor for none alpha.
        /// </summary>
        public const string BaseFloor01Interior = "fcs01_Floor01_Interior";
        /// <summary>
        /// Material for outdoor for none alpha.
        /// </summary>
        public const string BaseFloor01Exterior = "fcs01_Floor01_Exterior";
        /// <summary>
        /// Material for interiors - non-alpha
        /// </summary>
        public const string BaseOpaqueInterior = "fcs01_BO_Interior";
        /// <summary>
        /// Material for exterior - non alpha
        /// </summary>
        public const string BaseOpaqueExterior = "fcs01_BO_Exterior";
        /// <summary>
        /// Material for interior decals
        /// </summary>
        public const string BaseDecalsInterior = "fcs01_BD_Interior";
        /// <summary>
        /// Material for beacons
        /// </summary>
        public const string BaseBeaconLightEmissiveController = "fcs01_BBLEC";


        public const string TBaseDetail = "fcs01_D";
        public const string TBaseNormal = "fcs01_N";
        public const string TBaseEmission = "fcs01_E";
        public const string TFloorDetail = "fcs01_Floor01_D";
        public const string TFloorNormal = "fcs01_Floor01_N";
        public const string TFloorEmission = "fcs01_Floor01_E";
        public const string TEmissionInterior = "fcs01_E_Interior";
        public const string TBaseSpec = "fcs01_s";
        
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
        internal static GameObject FcsPDAPrefab { get; set; }
        internal static GameObject FCSPDADecoPrefab { get; set; }
        internal static GameObject ColorPickerDialogPrefab { get; set; }
        internal static GameObject MissionObjectiveItemPrefab { get; set; }
        public static GameObject MissionItemPrefab { get; set; }
        public static GameObject PDAEntryPrefab { get; set; }
        public static GameObject MissionMessageBoxPrefab { get; set; }
        public static GameObject BaseOperationItemPrefab { get; set; }

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

                    if (!LoadAssetV2("fcsPDA", QPatch.GlobalBundle, out var fcsPDAPrefab)) return false;
                    FcsPDAPrefab = fcsPDAPrefab;

                    if (!LoadAsset(Mod.AlterraHubPrefabName, QPatch.GlobalBundle, out var prefabGo)) return false;
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

                    if (!LoadAssetV2("DecoFCSPDA", QPatch.GlobalBundle, out var fcsPDADecoGo)) return false;
                    FCSPDADecoPrefab = fcsPDADecoGo;

                    if (!LoadAsset("MissionObjectItem", QPatch.GlobalBundle, out var missionObjectiveItemPrefabGo, false)) return false;
                    MissionObjectiveItemPrefab = missionObjectiveItemPrefabGo;

                    if (!LoadAsset("MissionItem", QPatch.GlobalBundle, out var missionItemPrefabGo, false)) return false;
                    MissionItemPrefab = missionItemPrefabGo;                    
                    
                    if (!LoadAsset("PDAEntry", QPatch.GlobalBundle, out var pdaEntryPrefabGo, false)) return false;
                    PDAEntryPrefab = pdaEntryPrefabGo;

                    if (!LoadAsset("MissionMessageBox", QPatch.GlobalBundle, out var missionMessageBox, false)) return false;
                    MissionMessageBoxPrefab = missionMessageBox;                    
                    
                    if (!LoadAsset("BaseOperationItem", QPatch.GlobalBundle, out var baseOperationItem, false)) return false;
                    BaseOperationItemPrefab = baseOperationItem;

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

        private static bool LoadAssetV2(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ApplyShadersV2(prefab, assetBundle);
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

        public static void ApplyShadersV2(GameObject prefab, AssetBundle bundle)
        {
            #region BasePrimaryCol
            MaterialHelpers.ApplyNormalShader(BasePrimaryCol, TBaseNormal, prefab, bundle);
            #endregion

            #region BaseSecondaryCol
            MaterialHelpers.ApplyNormalShader(BaseSecondaryCol, TBaseNormal, prefab, bundle);
            #endregion
            
            #region BaseTexDecals
            MaterialHelpers.ApplyNormalShader(BaseTexDecals, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseTexDecals, prefab);
            MaterialHelpers.ApplySpecShader(BaseTexDecals,TBaseSpec,prefab,1,3,bundle);
            #endregion

            #region BaseLightsEmissiveController
            MaterialHelpers.ApplyNormalShader(BaseLightsEmissiveController, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseLightsEmissiveController, prefab);
            MaterialHelpers.ApplyEmissionShader(BaseLightsEmissiveController, TBaseEmission, prefab, bundle, Color.white);
            #endregion

            #region BaseDecalsEmissiveController

            MaterialHelpers.ApplyNormalShader(BaseDecalsEmissiveController, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseDecalsEmissiveController, prefab);
            MaterialHelpers.ApplyEmissionShader(BaseDecalsEmissiveController, TBaseEmission, prefab, bundle, Color.white);

            #endregion

            #region BaseFloor01Interior
            MaterialHelpers.ApplyNormalShader(BaseFloor01Interior, TFloorNormal, prefab, bundle);
            MaterialHelpers.ApplyEmissionShader(BaseFloor01Interior, TFloorEmission, prefab, bundle, Color.white);

            #endregion

            #region BaseFloor01Exterior
            MaterialHelpers.ApplyNormalShader(BaseFloor01Exterior, TFloorNormal, prefab, bundle);
            #endregion

            #region BaseOpaqeInterior
            MaterialHelpers.ApplyNormalShader(BaseOpaqueInterior, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyEmissionShader(BaseOpaqueInterior, TEmissionInterior, prefab, bundle, Color.white);
            MaterialHelpers.ApplySpecShader(BaseOpaqueInterior, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseOpaqeExterior
            MaterialHelpers.ApplyNormalShader(BaseOpaqueExterior, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplySpecShader(BaseOpaqueExterior, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseDecalsInterior
            MaterialHelpers.ApplyNormalShader(BaseDecalsInterior, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyEmissionShader(BaseDecalsInterior, TEmissionInterior, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(BaseDecalsInterior, prefab);
            MaterialHelpers.ApplySpecShader(BaseDecalsInterior, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseDecalsExterior
            MaterialHelpers.ApplyNormalShader(BaseDecalsExterior, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseDecalsExterior, prefab);
            MaterialHelpers.ApplySpecShader(BaseDecalsExterior, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseBeaconLightEmissiveController
            MaterialHelpers.ApplyNormalShader(BaseBeaconLightEmissiveController, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseBeaconLightEmissiveController, prefab);
            MaterialHelpers.ApplyEmissionShader(BaseBeaconLightEmissiveController, TBaseEmission, prefab, bundle, Color.red);
            #endregion
        }

        public static void LoadV2Materials()
        {
            if (_v2MaterialsLoaded) return;

            if (QPatch.GlobalBundle == null)
            {
                QPatch.GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);
            }

            if (QPatch.GlobalBundle == null)
            {
                QuickLogger.Error($"[LoadV2Materials] GlobalBundle returned null stopping process");
                return;
            }

            #region BasePrimaryCol
            Material basePrimaryCol = QPatch.GlobalBundle.LoadAsset<Material>(BasePrimaryCol);
            MaterialHelpers.CreateV2NormalMaterial(basePrimaryCol, TBaseNormal, QPatch.GlobalBundle);
            _v2Materials.Add(BasePrimaryCol, basePrimaryCol);
            #endregion

            #region BaseSecondaryCol
            Material baseSecondaryCol = QPatch.GlobalBundle.LoadAsset<Material>(BaseSecondaryCol);
            MaterialHelpers.CreateV2NormalMaterial(baseSecondaryCol, TBaseNormal, QPatch.GlobalBundle);
            _v2Materials.Add(BaseSecondaryCol, baseSecondaryCol);
            #endregion
            
            #region BaseTexDecals
            Material baseTexDecals = QPatch.GlobalBundle.LoadAsset<Material>(BaseTexDecals);
            MaterialHelpers.CreateV2NormalMaterial(baseTexDecals, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseTexDecals, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseTexDecals, TBaseSpec, 1, 3, QPatch.GlobalBundle);
            _v2Materials.Add(BaseTexDecals, baseTexDecals);
            #endregion

            #region BaseLightsEmissiveController
            Material baseLightsEmissiveController = QPatch.GlobalBundle.LoadAsset<Material>(BaseLightsEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseLightsEmissiveController, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseLightsEmissiveController, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseLightsEmissiveController, TBaseEmission, QPatch.GlobalBundle, Color.white);
            _v2Materials.Add(BaseLightsEmissiveController, baseLightsEmissiveController);
            #endregion

            #region BaseDecalsEmissiveController
            Material baseDecalsEmissiveController = QPatch.GlobalBundle.LoadAsset<Material>(BaseDecalsEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsEmissiveController, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsEmissiveController, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseDecalsEmissiveController, TBaseEmission, QPatch.GlobalBundle, Color.white);
            _v2Materials.Add(BaseDecalsEmissiveController, baseDecalsEmissiveController);
            #endregion
            
            #region BaseFloor01Interior
            Material baseFloor01Interior = QPatch.GlobalBundle.LoadAsset<Material>(BaseFloor01Interior);
            MaterialHelpers.CreateV2NormalMaterial(baseFloor01Interior, TFloorNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseFloor01Interior, TFloorEmission, QPatch.GlobalBundle, Color.white);
            _v2Materials.Add(BaseFloor01Interior, baseFloor01Interior);
            #endregion

            #region BaseFloor01Exterior
            Material baseFloor01Exterior = QPatch.GlobalBundle.LoadAsset<Material>(BaseFloor01Exterior);
            MaterialHelpers.CreateV2NormalMaterial(baseFloor01Exterior, TFloorNormal, QPatch.GlobalBundle);
            _v2Materials.Add(BaseFloor01Exterior, baseFloor01Exterior);
            #endregion

            #region BaseOpaqueInterior
            Material baseOpaqueInterior = QPatch.GlobalBundle.LoadAsset<Material>(BaseOpaqueInterior);
            MaterialHelpers.CreateV2NormalMaterial(baseOpaqueInterior, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseOpaqueInterior, TEmissionInterior, QPatch.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseOpaqueInterior, TBaseSpec, 1, 3, QPatch.GlobalBundle);
            _v2Materials.Add(BaseOpaqueInterior, baseOpaqueInterior);
            #endregion

            #region BaseOpaqueExterior
            Material baseOpaqueExterior = QPatch.GlobalBundle.LoadAsset<Material>(BaseOpaqueExterior);
            MaterialHelpers.CreateV2NormalMaterial(baseOpaqueExterior, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseOpaqueExterior, TBaseSpec, 1, 3, QPatch.GlobalBundle);
            _v2Materials.Add(BaseOpaqueExterior, baseOpaqueExterior);
            #endregion

            #region BaseDecalsInterior
            Material baseDecalsInterior = QPatch.GlobalBundle.LoadAsset<Material>(BaseDecalsInterior);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsInterior, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsInterior, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseDecalsInterior, TEmissionInterior, QPatch.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseDecalsInterior, TBaseSpec, 1, 3, QPatch.GlobalBundle);
            _v2Materials.Add(BaseDecalsInterior, baseDecalsInterior);
            #endregion

            #region BaseDecalsExterior
            Material baseDecalsExterior = QPatch.GlobalBundle.LoadAsset<Material>(BaseDecalsExterior);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsExterior, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsExterior, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseDecalsExterior, TBaseSpec, 1, 3, QPatch.GlobalBundle);
            _v2Materials.Add(BaseDecalsExterior, baseDecalsExterior);
            #endregion

            #region BaseBeaconLightEmissiveController
            Material baseBeaconLightsEmissiveController = QPatch.GlobalBundle.LoadAsset<Material>(BaseBeaconLightEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseBeaconLightsEmissiveController, TBaseNormal, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseBeaconLightsEmissiveController, QPatch.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseBeaconLightsEmissiveController, TBaseEmission, QPatch.GlobalBundle, Color.red);
            _v2Materials.Add(BaseBeaconLightEmissiveController, baseBeaconLightsEmissiveController);
            #endregion

            _v2MaterialsLoaded = true;

        }

        public static void ReplaceShadersV2(GameObject prefab,string materialName)
        {
            LoadV2Materials();
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                for (var index = 0; index < renderer.materials.Length; index++)
                {
                    Material material = renderer.materials[index];
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        renderer.materials[index] = _v2Materials[materialName];
                    }
                }
            }
        }
    }
}