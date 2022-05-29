using System;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Elevator.Buildable;
using FCSCommon.Utilities;
using FMOD.Studio;
using UnityEngine;


namespace FCS_HomeSolutions.Buildables
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        internal static GameObject ElevatorUIPrefab;
        public static GameObject TVVideoBTNPrefab;
        internal static GameObject PlatformFloorFrame { get; set; }
        internal static GameObject ElevatorFloorItemPrefab { get; set; }
        internal static GameObject ElevatorPrefab { get; set; }
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
        public static GameObject SeaBreezeItemPrefab { get; set; }
        public static GameObject NetworkItemPrefab { get; set; }
        public static GameObject TemplateItem { get; set; }
        public static GameObject TrashRecyclerItemPrefab { get; set; }
        public static GameObject CurtainPrefab { get; set; }

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

            ElevatorFloorItemPrefab = GetPrefab("NewFloorItem",false);
            TVVideoBTNPrefab = GetPrefab("TVVideoBTN", false);
            PlatformFloorFrame = GetPrefab("PlatformFloorFrame", true);
            ElevatorPrefab = GetPrefab(ElevatorBuildable.ElevatorPrefabName, true);
            ElevatorUIPrefab = GetPrefab("ElevatorUI", false);
            TrashRecyclerItemPrefab = GetPrefab("RecyclerItem");
            SeaBreezeItemPrefab = GetPrefabFromGlobal("ARSItem",false);
            NetworkItemPrefab = GetPrefab("NetworkItem");
            TemplateItem = GetPrefab("TemplateItem");
            CurtainPrefab = GetPrefab("Curtain");
            _initialized = true;
        }
        
        internal static GameObject GetPrefab(string prefabName, bool applyShaders = true)
        {
            try
            {
                GameObject prefabGo;

                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (!LoadAssetV2(prefabName, ModBundle, out prefabGo, applyShaders)) return null;
                return prefabGo;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        internal static GameObject GetPrefabFromGlobal(string prefabName)
        {
            return FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName,
                FCSAssetBundlesService.PublicAPI.GlobalBundleName);
        }
        internal static GameObject GetPrefabFromGlobal(string prefabName, bool applyShaders = true)
        {
            try
            {
                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                return !LoadAssetV2(prefabName, FCSAssetBundlesService.PublicAPI.GetAssetBundleByName("fcsalterrahubbundle"), out GameObject prefabGo,applyShaders) ? null : prefabGo;
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
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    AlterraHub.ReplaceShadersV2(prefab);
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
