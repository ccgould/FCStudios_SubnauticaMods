using System;
using System.Collections.Generic;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Buildables
{
    public partial class AlterraHub
    {
        private static bool _initialized;
        private static Dictionary<string,Material> _v2Materials = new();
        private static bool _v2MaterialsLoaded;
        public static GameObject PDAScreenPrefab;
        public static GameObject ImageSelectorHUDPrefab;
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
        /// Material for outdoor for none alpha.
        /// </summary>
        public const string BaseFloor02Exterior = "fcs01_Floor02_Exterior";
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
        public const string TBaseSpec = "fcs01_s";        
        
        public const string TBaseColorDetail = "fcs01_BaseColor_D";
        public const string TBaseColorNormal = "fcs01_BaseColor_N";
        public const string TBaseColorSpec = "fcs01_BaseColor_S";

        public const string TBaseEmission = "fcs01_E";
        public const string TFloorDetail = "fcs01_Floor01_D";
        public const string TFloorNormal = "fcs01_Floor01_N";
        public const string TFloor2Normal = "fcs01_Floor02_N";
        public const string TFloor2Spec = "fcs01_Floor02_S";
        public const string TFloorEmission = "fcs01_Floor01_E";
        public const string TEmissionInterior = "fcs01_E_Interior";


        internal static string BodyMaterial => $"fcs{Mod.ModPackID}_COL";
        internal static string DecalMaterial => $"fcs{Mod.ModPackID}_DECALS";
        internal static string DetailsMaterial => $"fcs{Mod.ModPackID}_DETAILS";
        internal static string SpecTexture => $"fcs{Mod.ModPackID}_S";
        internal static string LUMTexture => $"fcs{Mod.ModPackID}_E";
        internal static string NormalTexture => $"fcs{Mod.ModPackID}_N";
        internal static string DetailTexture => $"fcs{Mod.ModPackID}_D";
        internal static GameObject DebitCardPrefab { get; set; }
        internal static GameObject KitPrefab { get; set; }
        internal static GameObject CartItemPrefab { get; set; }
        internal static GameObject FcsPDAPrefab { get; set; }
        internal static GameObject PDARadialMenuEntryPrefab { get; set; }
        internal static GameObject PDAShipmentItemPrefab { get; set; }
        internal static GameObject PDAShipmentItemNodePrefab { get; set; }
        internal static GameObject PDATeleportItemPrefab { get; set; }
        public static GameObject MissionMessageBoxPrefab { get; set; }
        public static GameObject AlterraHubDepotPrefab { get; set; }
        public static GameObject AlterraHubDepotItemPrefab { get; set; }
        public static GameObject EncyclopediaEntryPrefab { get; set; }
        public static GameObject AlterraHubFabricatorPrefab { get; set; }
        public static Sprite UpArrow { get; set; }
        public static Sprite DownArrow { get; set; }
        public static Sprite EncyclopediaEntryBackgroundNormal { get; set; }
        public static Sprite EncyclopediaEntryBackgroundSelected { get; set; }
        public static Sprite EncyclopediaEntryBackgroundHover { get; set; }
        public static GameObject DronePortPadHubNewPrefab { get; set; }

        public static GameObject AlterraHubTransportDronePrefab { get; set; }

        public static GameObject DronePortFragmentsPrefab { get; set; }
        public static GameObject DataBoxPrefab { get; set; }
        public static GameObject BluePrintDataDiscPrefab { get; set; }
        public static GameObject ImageSelectorItemPrefab { get; set; }

        public static bool GetPrefabs()
        {
            try
            {
                if (!_initialized)
                {
                    QuickLogger.Debug($"AssetBundle Set");

                    QuickLogger.Debug("GetPrefabs");
                    
                    if (!LoadAssetV2("fcsPDA", Main.GlobalBundle, out var fcsPDAPrefab)) return false;
                    FcsPDAPrefab = fcsPDAPrefab;                    
                    
                    if (!LoadAssetV2("RadialMenuEntry", Main.GlobalBundle, out var pdaRadialMenuEntryPrefab)) return false;
                    PDARadialMenuEntryPrefab = pdaRadialMenuEntryPrefab;

                    if (!LoadAsset(Mod.CardPrefabName, Main.GlobalBundle,out var cardPrefabGo)) return false;
                    DebitCardPrefab = cardPrefabGo;
                    
                    if (!LoadAsset(Mod.KitPrefabName, Main.GlobalBundle, out var kitPrefabGo)) return false; 
                    KitPrefab = kitPrefabGo;


                    if (!LoadAsset("CartItem", Main.GlobalBundle, out var cartItemPrefabGo)) return false;
                    CartItemPrefab = cartItemPrefabGo;


                    if (!LoadAssetV2("AlterraHubDepot", Main.GlobalBundle, out var alterraHubDepotPrefab)) return false;
                    AlterraHubDepotPrefab = alterraHubDepotPrefab;
                    
                    if (!LoadAsset("StoreItem", Main.GlobalBundle, out var itemPrefabGo)) return false;
                    ItemPrefab = itemPrefabGo;

                    if (!LoadAsset("MissionMessageBox", Main.GlobalBundle, out var missionMessageBox, false)) return false;
                    MissionMessageBoxPrefab = missionMessageBox;

                    if (!LoadSpriteAsset("arrowUp", Main.GlobalBundle, out var arrowUp)) return false;
                    UpArrow = arrowUp;

                    if (!LoadSpriteAsset("arrowDown", Main.GlobalBundle, out var arrowDown)) return false;
                    DownArrow = arrowDown;

                    if (!LoadAsset("EncyclopediaEntry", Main.GlobalBundle, out var encyclopediaEntryPrefab)) return false;
                    AddComponentsToEncyclopediaEntry(encyclopediaEntryPrefab);
                    EncyclopediaEntryPrefab = encyclopediaEntryPrefab;                    
                    
                    if (!LoadAsset(Mod.AlterraHubStationPrefabName, Main.GlobalBundle, out var alterraHubFabricatorPrefab)) return false;
                    AlterraHubFabricatorPrefab = alterraHubFabricatorPrefab;

                    if (!LoadAsset("PDAShipmentItem", Main.GlobalBundle, out var pdaShipmentItemPrefab,false)) return false;
                     PDAShipmentItemPrefab = pdaShipmentItemPrefab;


                     if (!LoadAsset("PDAShipmentItemNode", Main.GlobalBundle, out var pdaShipmentItemNodePrefab, false)) return false;
                     PDAShipmentItemNodePrefab = pdaShipmentItemNodePrefab;

                    if (!LoadAsset("PDATeleportItem", Main.GlobalBundle, out var pdaTeleportItemPrefab, false)) return false;
                     PDATeleportItemPrefab = pdaTeleportItemPrefab;


                    if (!LoadAsset("AlterraHubDepotItem", Main.GlobalBundle, out var alterraHubDepotItemPrefab)) return false;
                    AlterraHubDepotItemPrefab = alterraHubDepotItemPrefab;                    
                    
                    if (!LoadAsset("uGUI_PDAScreen", Main.GlobalBundle, out var pdaCanvas)) return false;
                    PDAScreenPrefab = pdaCanvas;

                    if (!LoadSpriteAsset("EncyclopediaEntryBackgroundHover", Main.GlobalBundle, out var hovered)) return false;
                    EncyclopediaEntryBackgroundHover = hovered;
                    if (!LoadSpriteAsset("EncyclopediaEntryBackgroundSelected", Main.GlobalBundle, out var selected)) return false;
                    EncyclopediaEntryBackgroundSelected = selected;
                    if (!LoadSpriteAsset("EncyclopediaEntryBackgroundNormal", Main.GlobalBundle, out var normal)) return false;
                    EncyclopediaEntryBackgroundNormal = normal;

                    if (!LoadAssetV2("DronePortPad_HubNew", Main.GlobalBundle, out var dronePortPadHubNewPrefab)) return false;
                    DronePortPadHubNewPrefab = dronePortPadHubNewPrefab;

                    if (!LoadAssetV2("AlterraHubTransportDrone", Main.GlobalBundle, out var alterraHubTransportDronePrefab)) return false;
                    AlterraHubTransportDronePrefab = alterraHubTransportDronePrefab;

                    if (!LoadAssetV2("BLUEPRINT_DATA_DISC", Main.GlobalBundle, out var blueprintDataDiscPrefab)) return false;
                    BluePrintDataDiscPrefab = blueprintDataDiscPrefab;                    
                    
                    if (!LoadAsset("ImageSelectorItem", Main.GlobalBundle, out var imageSelectorItemPrefab,false)) return false;
                    ImageSelectorItemPrefab = imageSelectorItemPrefab;

                    if (!LoadAsset("ImageSelectorUI", Main.GlobalBundle, out var imageSelectorPrefab, false)) return false;
                    ImageSelectorHUDPrefab = imageSelectorPrefab;

                    if (!LoadAssetV2("fcs_BlueprintBox", Main.GlobalBundle, out var fcsDataBox)) return false;
                    DataBoxPrefab = fcsDataBox;
                    
                    _initialized = true;
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                return false;
            }
        }



        private static void AddComponentsToEncyclopediaEntry(GameObject encyclopediaEntryPrefab)
        {
            encyclopediaEntryPrefab.SetActive(false);
            var listEntry = encyclopediaEntryPrefab.AddComponent<uGUI_ListEntry>();
            listEntry.layoutElement = encyclopediaEntryPrefab.FindChild("ImageContainer").GetComponentInChildren<LayoutElement>();

            //TODO V2 Find Fix
            //#if SUBNAUTICA
//            listEntry.text = encyclopediaEntryPrefab.GetComponentInChildren<Text>();
//#endif
            listEntry.icon = encyclopediaEntryPrefab.FindChild("ImageContainer").FindChild("Arrow").GetComponent<Image>();
            listEntry.background = encyclopediaEntryPrefab.GetComponent<Image>();
            encyclopediaEntryPrefab.SetActive(true);
        }

        internal static bool LoadAsset(string prefabName,AssetBundle assetBundle,out GameObject go,bool applyShaders = true)
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

        internal static bool LoadSpriteAsset(string prefabName, AssetBundle assetBundle, out Sprite image)
        {
            //We have found the asset bundle and now we are going to continue by looking for the model.
            Sprite prefab = assetBundle.LoadAsset<Sprite>(prefabName);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                image = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            image = null;
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
                    ReplaceShadersV2(prefab);
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
            MaterialHelpers.ApplyNormalShader(BasePrimaryCol, TBaseColorNormal, prefab, bundle);
            MaterialHelpers.ApplySpecShader(BasePrimaryCol, TBaseColorSpec, prefab, 1, 3, bundle);

            #endregion

            #region BaseSecondaryCol
            MaterialHelpers.ApplyNormalShader(BaseSecondaryCol, TBaseColorNormal, prefab, bundle);
            MaterialHelpers.ApplySpecShader(BaseSecondaryCol, TBaseColorSpec, prefab, 1, 3, bundle);

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
            MaterialHelpers.ApplySpecShader(BaseLightsEmissiveController, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseDecalsEmissiveController

            MaterialHelpers.ApplyNormalShader(BaseDecalsEmissiveController, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseDecalsEmissiveController, prefab);
            MaterialHelpers.ApplyEmissionShader(BaseDecalsEmissiveController, TBaseEmission, prefab, bundle, Color.white);
            MaterialHelpers.ApplySpecShader(BaseDecalsEmissiveController, TBaseSpec, prefab, 1, 3, bundle);
            #endregion

            #region BaseFloor01Interior
            MaterialHelpers.ApplyNormalShader(BaseFloor01Interior, TFloorNormal, prefab, bundle);
            MaterialHelpers.ApplyEmissionShader(BaseFloor01Interior, TFloorEmission, prefab, bundle, Color.white);

            #endregion

            #region BaseFloor01Exterior
            MaterialHelpers.ApplyNormalShader(BaseFloor01Exterior, TFloorNormal, prefab, bundle);
            #endregion

            #region BaseFloor02Exterior
            MaterialHelpers.ApplyNormalShader(BaseFloor02Exterior, TFloor2Normal, prefab, bundle);
            MaterialHelpers.ApplyAlphaShader(BaseFloor02Exterior, prefab);
            MaterialHelpers.ApplySpecShader(BaseFloor02Exterior, TFloor2Spec, prefab, 1, 3, bundle);
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

        public static void ApplyShadersV2(GameObject prefab)
        {
            ApplyShadersV2(prefab,Main.GlobalBundle);
        }

        public static void LoadV2Materials()
        {
            if (_v2MaterialsLoaded) return;

            if (Main.GlobalBundle == null)
            {
                Main.GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);
            }

            if (Main.GlobalBundle == null)
            {
                QuickLogger.Error($"[LoadV2Materials] GlobalBundle returned null stopping process");
                return;
            }

            #region BasePrimaryCol
            Material basePrimaryCol = Main.GlobalBundle.LoadAsset<Material>(BasePrimaryCol);
            MaterialHelpers.CreateV2NormalMaterial(basePrimaryCol, TBaseColorNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(basePrimaryCol, TBaseColorSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BasePrimaryCol, basePrimaryCol);
            #endregion

            #region BaseSecondaryCol
            Material baseSecondaryCol = Main.GlobalBundle.LoadAsset<Material>(BaseSecondaryCol);
            MaterialHelpers.CreateV2NormalMaterial(baseSecondaryCol, TBaseColorNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseSecondaryCol, TBaseColorSpec, 1, 3, Main.GlobalBundle);

            _v2Materials.Add(BaseSecondaryCol, baseSecondaryCol);
            #endregion

            #region BaseTexDecals
            Material baseTexDecals = Main.GlobalBundle.LoadAsset<Material>(BaseTexDecals);
            MaterialHelpers.CreateV2NormalMaterial(baseTexDecals, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseTexDecals, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseTexDecals, TBaseSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseTexDecals, baseTexDecals);
            #endregion

            #region BaseLightsEmissiveController
            Material baseLightsEmissiveController = Main.GlobalBundle.LoadAsset<Material>(BaseLightsEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseLightsEmissiveController, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseLightsEmissiveController, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseLightsEmissiveController, TBaseEmission, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseLightsEmissiveController, TBaseColorSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseLightsEmissiveController, baseLightsEmissiveController);
            #endregion

            #region BaseDecalsEmissiveController
            Material baseDecalsEmissiveController = Main.GlobalBundle.LoadAsset<Material>(BaseDecalsEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsEmissiveController, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsEmissiveController, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseDecalsEmissiveController, TBaseEmission, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseDecalsEmissiveController, TBaseColorSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseDecalsEmissiveController, baseDecalsEmissiveController);
            #endregion

            #region BaseFloor01Interior
            Material baseFloor01Interior = Main.GlobalBundle.LoadAsset<Material>(BaseFloor01Interior);
            MaterialHelpers.CreateV2NormalMaterial(baseFloor01Interior, TFloorNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseFloor01Interior, TFloorEmission, Main.GlobalBundle, Color.white);
            _v2Materials.Add(BaseFloor01Interior, baseFloor01Interior);
            #endregion

            #region BaseFloor01Exterior
            Material baseFloor01Exterior = Main.GlobalBundle.LoadAsset<Material>(BaseFloor01Exterior);
            MaterialHelpers.CreateV2NormalMaterial(baseFloor01Exterior, TFloorNormal, Main.GlobalBundle);
            _v2Materials.Add(BaseFloor01Exterior, baseFloor01Exterior);
            #endregion

            #region BaseFloor02Exterior
            Material baseFloor02Exterior = Main.GlobalBundle.LoadAsset<Material>(BaseFloor02Exterior);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseFloor02Exterior, Main.GlobalBundle);
            MaterialHelpers.CreateV2NormalMaterial(baseFloor02Exterior, TFloor2Normal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseFloor02Exterior, TFloor2Spec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseFloor02Exterior, baseFloor02Exterior);
            #endregion

            #region BaseOpaqueInterior
            Material baseOpaqueInterior = Main.GlobalBundle.LoadAsset<Material>(BaseOpaqueInterior);
            MaterialHelpers.CreateV2NormalMaterial(baseOpaqueInterior, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseOpaqueInterior, TEmissionInterior, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseOpaqueInterior, TBaseSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseOpaqueInterior, baseOpaqueInterior);
            #endregion

            #region BaseOpaqueExterior
            Material baseOpaqueExterior = Main.GlobalBundle.LoadAsset<Material>(BaseOpaqueExterior);
            MaterialHelpers.CreateV2NormalMaterial(baseOpaqueExterior, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseOpaqueExterior, TBaseSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseOpaqueExterior, baseOpaqueExterior);
            #endregion

            #region BaseDecalsInterior
            Material baseDecalsInterior = Main.GlobalBundle.LoadAsset<Material>(BaseDecalsInterior);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsInterior, Main.GlobalBundle);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsInterior, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseDecalsInterior, TEmissionInterior, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2Specular(baseDecalsInterior, TBaseSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseDecalsInterior, baseDecalsInterior);
            #endregion

            #region BaseDecalsExterior
            Material baseDecalsExterior = Main.GlobalBundle.LoadAsset<Material>(BaseDecalsExterior);
            MaterialHelpers.CreateV2NormalMaterial(baseDecalsExterior, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseDecalsExterior, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseDecalsExterior, TBaseSpec, 1, 3, Main.GlobalBundle);
            _v2Materials.Add(BaseDecalsExterior, baseDecalsExterior);
            #endregion

            #region BaseBeaconLightEmissiveController
            Material baseBeaconLightsEmissiveController = Main.GlobalBundle.LoadAsset<Material>(BaseBeaconLightEmissiveController);
            MaterialHelpers.CreateV2NormalMaterial(baseBeaconLightsEmissiveController, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2ApplyAlphaMaterial(baseBeaconLightsEmissiveController, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseBeaconLightsEmissiveController, TBaseEmission, Main.GlobalBundle, Color.red);
            _v2Materials.Add(BaseBeaconLightEmissiveController, baseBeaconLightsEmissiveController);
            #endregion

            _v2MaterialsLoaded = true;

        }

        public static void ReplaceShadersV2(GameObject prefab)
        {
            ReplaceShadersV2(prefab, BasePrimaryCol);
            ReplaceShadersV2(prefab, BaseSecondaryCol);
            ReplaceShadersV2(prefab, BaseTexDecals);
            ReplaceShadersV2(prefab, BaseLightsEmissiveController);
            ReplaceShadersV2(prefab, BaseDecalsEmissiveController);
            ReplaceShadersV2(prefab, BaseFloor01Interior);
            ReplaceShadersV2(prefab, BaseFloor01Exterior);
            ReplaceShadersV2(prefab, BaseFloor02Exterior);
            ReplaceShadersV2(prefab, BaseOpaqueInterior);
            ReplaceShadersV2(prefab, BaseOpaqueExterior);
            ReplaceShadersV2(prefab, BaseDecalsInterior);
            ReplaceShadersV2(prefab, BaseDecalsExterior);
        }

        private static void ReplaceShadersV2(GameObject prefab,string materialName)
        {

            if (prefab is null)
            {
                QuickLogger.Error("Prefab was null when trying to replace shaders V2");
                return;
            }

            LoadV2Materials();

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            
            //QuickLogger.Debug($"Replacing Shaders on Object: {prefab.gameObject.name} | Render Count: {renderers.Length}");

            foreach (Renderer renderer in renderers)
            {
                if(renderer != null) continue;
                //QuickLogger.Debug($"Processing: {renderer.name} Materials Count: {renderer.materials.Length}");

                for (var index = 0; index < renderer.materials.Length; index++)
                {
                    Material material = renderer.materials[index];

                    ///QuickLogger.Debug($"Trying to Replacing Material: {material.name} {material.shader.name}");
                    
                    if (material.name.RemoveInstance().Equals(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        //QuickLogger.Debug($"Replacing Material: {material.name} {material.shader.name}");

                        renderer.materials[index] = _v2Materials[materialName];

                        //QuickLogger.Debug($"Done Replacing Material: {renderer.materials[index].name} {renderer.materials[index].shader.name} {_v2Materials[materialName].shader.name}");
                    }
                }
            }
        }

        public static bool LoadPrefabAssetV2(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ReplaceShadersV2(prefab);
                }

                go = prefab;

                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");
            go = null;
            return false;
        }

        public static GameObject GetPrefab(string prefabName)
        {
            LoadAssetV2(prefabName, Main.GlobalBundle, out var result);
            return result;
        }
    }
}