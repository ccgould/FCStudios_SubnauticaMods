using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets;
using System.Collections;
using UnityEngine;
using Nautilus.Assets.Gadgets;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
internal class BaseManagerBuildable : FCSBuildableModBase
{
    private TechType _kitTechType;
    public static TechType PatchedTechType { get; private set; }
    public static TechType RemoteModuleTechType { get; private set; }
    public static TechType TranceiverModuleTechType { get; private set; }
    public static TechType DSSIntegrationModuleTechType { get; private set; }

    public BaseManagerBuildable() : base(PluginInfo.PLUGIN_NAME, "fcsBaseManagerScreen", FileSystemHelper.ModDirLocation, "BaseManager", "Base Manager Display")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(ClassID, FriendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;
            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);
            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_BaseManager>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_BaseManager", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);

            PatchModules();
        };
    }

    private void PatchModules()
    {
        // Remote Module.
        var remoteConnectionModuleInfo = PrefabInfo.WithTechType("RemoteConnectionModule", "Remote Connection", "Allows the FCS PDA to see all base deviced from the Base Manager remotely.").WithIcon(SpriteManager.Get(TechType.MapRoomUpgradeScanRange));
        var remoteConnectionModulePrefab = new CustomPrefab(remoteConnectionModuleInfo);
        var cyclopsHullObj = new CloneTemplate(remoteConnectionModuleInfo, TechType.CyclopsHullModule1);
        remoteConnectionModulePrefab.SetGameObject(cyclopsHullObj);
        remoteConnectionModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        remoteConnectionModulePrefab.Register();
        RemoteModuleTechType = remoteConnectionModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(RemoteModuleTechType, RemoteModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);

        // Transceiver Module.
        var transceiverModuleInfo = PrefabInfo.WithTechType("TransceiverModule", "Tranceiver", "N/A").WithIcon(SpriteManager.Get(TechType.MapRoomUpgradeScanRange));
        var transceiverModulePrefab = new CustomPrefab(transceiverModuleInfo);
        transceiverModulePrefab.SetGameObject(cyclopsHullObj);
        transceiverModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        transceiverModulePrefab.Register();
        TranceiverModuleTechType = transceiverModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(TranceiverModuleTechType, TranceiverModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);

        // DSS Integration Module.
        var dssIntegrationModuleInfo = PrefabInfo.WithTechType("DSSIntegrationModule", "DSS Integration", "N/A").WithIcon(SpriteManager.Get(TechType.MapRoomUpgradeScanRange));
        var dssIntegerationModulePrefab = new CustomPrefab(dssIntegrationModuleInfo);
        dssIntegerationModulePrefab.SetGameObject(cyclopsHullObj);
        dssIntegerationModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        dssIntegerationModulePrefab.Register();
        DSSIntegrationModuleTechType = dssIntegrationModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(DSSIntegrationModuleTechType, DSSIntegrationModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);



    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        yield break;
    }
}
