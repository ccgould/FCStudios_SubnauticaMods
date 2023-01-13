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
        /// Material for fcs_BaseColor
        /// </summary>
        public const string BasePrimaryCol = "fcs_BaseColor";
        /// <summary>
        /// Material for fcs_LIGHTS
        /// </summary>
        public const string BaseSecondaryCol = "fcs_LIGHTS";




        public const string TBaseDetail = "fcsCoreBaseTexture_D";
        public const string TBaseNormal = "fcsCoreBaseTexture_N";
        public const string TBaseSpec = "fcsCoreBaseTexture_S";
        public const string TBaseEmission = "fcsCoreBaseTexture_LM";
        public const string TBaseID = "fcsCoreBaseTexture_ID";

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


        public static void ApplyShadersV2(GameObject prefab, AssetBundle bundle)
        {
            #region BasePrimaryCol
            MaterialHelpers.ApplyNormalShader(BasePrimaryCol, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplySpecShader(BasePrimaryCol, TBaseSpec, prefab, 1, 3, bundle);

            #endregion

            #region BaseSecondaryCol
            MaterialHelpers.ApplyNormalShader(BaseSecondaryCol, TBaseNormal, prefab, bundle);
            MaterialHelpers.ApplySpecShader(BaseSecondaryCol, TBaseSpec, prefab, 1, 3, bundle);

            #endregion
        }

        public static void ApplyShadersV2(GameObject prefab)
        {
            ApplyShadersV2(prefab,Main.GlobalBundle);
        }

        public static void LoadV2Materials()
        {
            return;
            QuickLogger.Info($"[LoadV2Materials] processing");

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
            MaterialHelpers.CreateV2NormalMaterial(basePrimaryCol, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(basePrimaryCol, TBaseSpec, 1, 3, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(basePrimaryCol, TBaseEmission, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2ColorMaskShader(basePrimaryCol,TBaseID,Color.white,Color.white,Color.white,Main.GlobalBundle);
            _v2Materials.Add(BasePrimaryCol, basePrimaryCol);
            #endregion

            #region BaseSecondaryCol
            Material baseSecondaryCol = Main.GlobalBundle.LoadAsset<Material>(BaseSecondaryCol);
            MaterialHelpers.CreateV2NormalMaterial(baseSecondaryCol, TBaseNormal, Main.GlobalBundle);
            MaterialHelpers.CreateV2Specular(baseSecondaryCol, TBaseSpec, 1, 3, Main.GlobalBundle);
            MaterialHelpers.CreateV2EmissionMaterial(baseSecondaryCol, TBaseEmission, Main.GlobalBundle, Color.white);
            MaterialHelpers.CreateV2ColorMaskShader(baseSecondaryCol, TBaseID, Color.white, Color.white, Color.white, Main.GlobalBundle);
            _v2Materials.Add(BaseSecondaryCol, baseSecondaryCol);
            #endregion

            _v2MaterialsLoaded = true;

        }

        public static void ReplaceShadersV2(GameObject prefab)
        {
            ReplaceShadersV2(prefab, BasePrimaryCol);
            ReplaceShadersV2(prefab, BaseSecondaryCol);
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