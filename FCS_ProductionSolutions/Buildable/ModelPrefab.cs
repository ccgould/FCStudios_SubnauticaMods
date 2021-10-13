using System;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Buildable
{
    internal static class ModelPrefab
    {
        public static GameObject DSSCrafterCratePrefab;
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        public static GameObject HydroponicHarvesterPrefab { get; set; }
        public static GameObject HydroponicDNASamplePrefab { get; set; }
        public static GameObject HydroponicScreenItemPrefab { get; set; }
        public static GameObject MatterAnalyzerPrefab { get; set; }
        public static GameObject DeepDrillerItemPrefab { get; set; }
        public static GameObject DeepDrillerPrefab { get; set; }
        internal static GameObject DeepDrillerSandPrefab { get; set; }
        public static GameObject DeepDrillerOreBTNPrefab { get; set; }
        public static GameObject DeepDrillerOverrideItemPrefab { get; set; }
        public static GameObject DeepDrillerFunctionOptionItemPrefab { get; set; }
        public static GameObject ReplicatorPrefab { get; set; }

        internal static void Initialize()
        {
            if (GlobalBundle == null)
            {
                GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            }

            if (ModBundle == null)
            {
                ModBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.ModBundleName, Mod.GetModDirectory());
            }

            HydroponicHarvesterPrefab = GetPrefab(Mod.HydroponicHarvesterModPrefabName);
            HydroponicScreenItemPrefab = GetPrefab("HarvesterScreenItem");
            HydroponicDNASamplePrefab = GetPrefab("DNASampleEntry");
            MatterAnalyzerPrefab = GetPrefab(Mod.MatterAnalyzerPrefabName);
            DeepDrillerItemPrefab = GetPrefab("InventoryItemBTN");
            DeepDrillerOreBTNPrefab = GetPrefab("OreBTN");
            DeepDrillerPrefab = GetPrefab(Mod.DeepDrillerMk3PrefabName);
            DeepDrillerSandPrefab = GetPrefab("DD_SandOre");
            //DeepDrillerListItemPrefab = GetPrefab("DeepDrillerTransferToggleButton");
            //DeepDrillerProgrammingItemPrefab = GetPrefab("DeepDrillerProgrammingItem");
            DeepDrillerOverrideItemPrefab = GetPrefab("OverrideItem");
            DeepDrillerFunctionOptionItemPrefab = GetPrefab("FunctionOptionItem");
            ReplicatorPrefab = GetPrefab(Mod.ReplicatorPrefabName);
            DSSCrafterCratePrefab = GetPrefab("CrafterCrate");
        }

        internal static GameObject GetPrefab(string prefabName)
        {
            try
            {
                GameObject prefabGo;

                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (!AlterraHub.LoadPrefabAssetV2(prefabName, ModBundle, out prefabGo)) return null;
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

        internal static Sprite GetSprite(string spriteName)
        {
            return ModBundle.LoadAsset<Sprite>(spriteName);
        }
    }
}
