using System;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Configuration
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        public static GameObject DSSServerPrefab;
        public static GameObject DSSAvaliableVehiclesItemPrefab;
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        public static GameObject AlterraStoragePrefab { get; set; }
        public static GameObject DSSItemDisplayPrefab { get; set; }
        public static GameObject DSSAntennaPrefab { get; set; }
        public static GameObject DSSWallServerRackPrefab { get; set; }
        public static GameObject DSSFloorServerRackPrefab { get; set; }
        public static GameObject DSSTerminalPrefab { get; set; }
        public static GameObject DSSInventoryItemPrefab { get; set; }
        public static GameObject DSSTransceiverPrefab { get; set; }


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

            AlterraStoragePrefab = GetPrefab(Mod.AlterraStoragePrefabName);
            DSSServerPrefab = GetPrefab(Mod.DSSServerPrefabName);
            DSSTerminalPrefab = GetPrefab(Mod.DSSTerminalPrefabName);
            DSSTransceiverPrefab = GetPrefab(Mod.TransceiverPrefabName);
            DSSWallServerRackPrefab = GetPrefab(Mod.DSSWallServerRackPrefabName);
            DSSFloorServerRackPrefab = GetPrefab(Mod.DSSFloorServerRackPrefabName);
            DSSAntennaPrefab = GetPrefab(Mod.DSSAntennaPrefabName);
            DSSItemDisplayPrefab = GetPrefab(Mod.DSSItemDisplayPrefabName);
            DSSInventoryItemPrefab = GetPrefab("DSSInventoryItem");
            DSSAvaliableVehiclesItemPrefab = GetPrefab("AvaliableVehiclesItem");
        }
        
        internal static GameObject GetPrefab(string prefabName)
        {
            try
            {
                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (!AlterraHub.LoadPrefabAssetV2(prefabName, ModBundle, out var prefabGo)) return null;
                return prefabGo;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
    }
}
