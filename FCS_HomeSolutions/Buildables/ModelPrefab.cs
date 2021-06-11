using System;
using System.Collections.Generic;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Object = UnityEngine.Object;


namespace FCS_HomeSolutions.Buildables
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModPackID}_COL";
        internal static string SecondaryMaterial => $"{Mod.ModPackID}_COL_S";
        internal static string DecalMaterial => $"{Mod.ModPackID}_DECALS";
        internal static string DetailsMaterial => $"{Mod.ModPackID}_DETAILS";
        internal const string CurtainDecalMaterial = "CurtainPackTemplate_Decal";
        internal static string SpecTexture => $"{Mod.ModPackID}_S";
        internal static string LUMTexture => $"{Mod.ModPackID}_E";
        internal static string EmissionControllerMaterial => $"{Mod.ModPackID}_E_Controller";
        internal static string NormalTexture => $"{Mod.ModPackID}_N";
        internal static string DetailTexture => $"{Mod.ModPackID}_D";
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        internal static GameObject PaintToolPrefab { get; set; }
        internal static GameObject BaseOperatorPrefab { get; set; }
        public static GameObject HoverLiftPadPrefab { get; set; }
        public static GameObject SmallOutdoorPot { get; set; }
        public static GameObject MiniFountainFilterPrefab { get; set; }
        public static GameObject SeaBreezeItemPrefab { get; set; }
        public static GameObject SeaBreezePrefab { get; set; }
        public static GameObject TrashReceptaclePrefab { get; set; }
        public static GameObject TrashRecyclerPrefab { get; set; }
        public static GameObject PaintCanPrefab { get; set; }
        public static GameObject QuantumTeleporterPrefab { get; set; }
        public static GameObject NetworkItemPrefab { get; set; }
        public static GameObject TemplateItem { get; set; }
        public static GameObject TrashRecyclerItemPrefab { get; set; }
        public static GameObject CurtainPrefab { get; set; }
        public static GameObject AlienChefPrefab { get; set; }
        public static GameObject Cabinet1Prefab { get; set; }
        public static GameObject Cabinet2Prefab { get; set; }
        public static GameObject Cabinet3Prefab { get; set; }
        public static GameObject CookerItemPrefab { get; set; }
        public static GameObject CookerOrderItemPrefab { get; set; }
        public static GameObject LedLightLongPrefab { get; set; }
        public static GameObject LedLightWallPrefab { get; set; }
        public static GameObject LedLightShortPrefab { get; set; }
        public static GameObject ObservationTankPrefab { get; set; }
        public static GameObject FireExtinguisherRefuelerPrefab { get; set; }
        public static GameObject AlterraMiniBathroomPrefab { get; set; }

        internal static void Initialize()
        {
            if (_initialized) return;

            if (GlobalBundle == null)
            {
                GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            }

            if (ModBundle == null)
            {
                ModBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.ModBundleName, Mod.GetModDirectory());
            }

            //LoadFMod();

            PaintToolPrefab = GetPrefab(Mod.PaintToolPrefabName);
            SmallOutdoorPot = GetPrefab(Mod.SmartPlanterPotPrefabName);
            //BaseOperatorPrefab = GetPrefab(Mod.BaseOperatorPrefabName);
            //HoverLiftPadPrefab = GetPrefab(Mod.HoverLiftPrefabName);
            MiniFountainFilterPrefab = GetPrefab(Mod.MiniFountainFilterPrefabName);
            SeaBreezePrefab = GetPrefab(Mod.SeaBreezePrefabName);
            TrashReceptaclePrefab = GetPrefab(Mod.TrashReceptaclePrefabName);
            TrashRecyclerPrefab = GetPrefab(Mod.RecyclerPrefabName);
            PaintCanPrefab = GetPrefab(Mod.PaintCanPrefabName);
            QuantumTeleporterPrefab = GetPrefab(Mod.QuantumTeleporterPrefabName);
            AlienChefPrefab = GetPrefab(Mod.AlienChefPrefabName);
            Cabinet1Prefab = GetPrefab(Mod.Cabinet1PrefabName);
            Cabinet2Prefab = GetPrefab(Mod.Cabinet2PrefabName);
            Cabinet3Prefab = GetPrefab(Mod.Cabinet3PrefabName);
            AlterraMiniBathroomPrefab = GetPrefab(Mod.AlterraMiniBathroomPrefabName,true);
            FireExtinguisherRefuelerPrefab = GetPrefab(Mod.FireExtinguisherRefuelerPrefabName);
            ObservationTankPrefab = GetPrefab(Mod.EmptyObservationTankPrefabName);
            TrashRecyclerItemPrefab = GetPrefab("RecyclerItem");
            SeaBreezeItemPrefab = GetPrefab("ARSItem");
            NetworkItemPrefab = GetPrefab("NetworkItem");
            TemplateItem = GetPrefab("TemplateItem");
            CurtainPrefab = GetPrefab("Curtain");
            CookerItemPrefab = GetPrefab("CookerItem");
            CookerOrderItemPrefab = GetPrefab("OrderItem");
            LedLightLongPrefab = GetPrefab("FCS_LedLightStick_03");
            LedLightShortPrefab = GetPrefab("FCS_LedLightStick_01");
            LedLightWallPrefab = GetPrefab("FCS_LedLightStick_02");
            _initialized = true;
        }
        
        internal static GameObject GetPrefab(string prefabName, bool isV2 = false)
        {
            try
            {
                GameObject prefabGo;

                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (isV2)
                {
                    if (!LoadAssetV2(prefabName, ModBundle, out prefabGo)) return null;
                }
                else
                {
                    if (!LoadAsset(prefabName, ModBundle, out prefabGo)) return null;
                }
                
                return prefabGo;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
        
        //internal static void LoadFMod()
        //{
        //    //RuntimeManager
        //    var strings = ModBundle.LoadAsset<TextAsset>("FCS Master Bank.strings");
        //    var master = ModBundle.LoadAsset<TextAsset>("FCS Master Bank");
            
        //    if (master == null)
        //    {
        //        QuickLogger.Debug("FCS Master Not Found");
        //        return;
        //    }
        //    QuickLogger.Debug($"Master: {master}");
        //    RuntimeManager.LoadBank(master);




        //    QuickLogger.Debug($"Any Bank Loading: {RuntimeManager.AnyBankLoading()}");
        //    if (RuntimeManager.HasBankLoaded("FCS Master Bank"))
        //    {
        //        QuickLogger.Debug("Bank Loaded");

        //        foreach (KeyValuePair<string, RuntimeManager.LoadedBank> bank in RuntimeManager.Instance.loadedBanks)
        //        {
        //            QuickLogger.Debug(bank.Key);
        //            if (bank.Key.Equals("FCS Master Bank"))
        //            {
        //                QuickLogger.Debug($"Bank Count: {bank.Value.RefCount}");
        //                bank.Value.Bank.getEventList(out EventDescription[] array);
        //                for (int i = 0; i < array.Length; i++)
        //                {
        //                    array[i].getPath(out string path);
        //                    QuickLogger.Debug($"Bank Count: {path}");
        //                }

        //                break;
        //            }
                    
        //        }

        //        //ShowerLoop = RuntimeManager.CreateInstance("event:/AlterraMiniBathroom/Shower");
        //    }
        //    else
        //    {
        //        QuickLogger.Debug("Bank Not Loaded");
        //    }
            
        //}

        public static EventInstance ShowerLoop { get; set; }


        private static bool LoadAsset(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, assetBundle);
                    QuickLogger.Debug($"Applied shaders to prefab {prefabName}");
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
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BasePrimaryCol);
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BaseSecondaryCol);
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BaseDecalsExterior);
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BaseTexDecals);
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BaseLightsEmissiveController);
                    AlterraHub.ReplaceShadersV2(prefab,AlterraHub.BaseDecalsEmissiveController);
                    QuickLogger.Debug($"Applied shaderes to prefab {prefabName}");
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
            MaterialHelpers.ApplyEmissionShader(EmissionControllerMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(DetailsMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(CurtainDecalMaterial, prefab);
            #endregion
        }

        public static Texture2D GetImageFromPrefab(string imageName)
        {
            var prefab = ModBundle.LoadAsset<Texture2D>(imageName);
            return prefab != null ? prefab : null;
        }
    }
}
