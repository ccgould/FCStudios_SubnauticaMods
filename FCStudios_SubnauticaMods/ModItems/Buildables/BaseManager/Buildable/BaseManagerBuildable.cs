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
using Nautilus.Utility;
using System.IO;
using System.Reflection;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
internal class BaseManagerBuildable : FCSBuildableModBase
{
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
        var assetPath = ModRegistrationService.GetModPackData(PluginInfo.PLUGIN_NAME).GetAssetPath();

        // Remote Module.
        var remoteConnectionModuleInfo = PrefabInfo.WithTechType("RemoteConnectionModule", "Remote Connection Module", "Allows the FCS PDA to see all base devices from the Base Manager remotely.").WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(assetPath, $"RemoteConnectionModule.png")));
        var remoteConnectionModulePrefab = new CustomPrefab(remoteConnectionModuleInfo);
        var cyclopsHullObj = new CloneTemplate(remoteConnectionModuleInfo, TechType.CyclopsHullModule1);
        remoteConnectionModulePrefab.SetGameObject(cyclopsHullObj);
        remoteConnectionModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        remoteConnectionModulePrefab.Register();
        RemoteModuleTechType = remoteConnectionModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(RemoteModuleTechType, RemoteModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub,true);

        // Transceiver Module.
        var transceiverModuleInfo = PrefabInfo.WithTechType("TransceiverModule", "Tranceiver Module", "Supports 5 Automation Operations (See Encyclopedia) (Req Module Rack)").WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(assetPath, $"TransceiverModule.png")));
        var transceiverModulePrefab = new CustomPrefab(transceiverModuleInfo);
        transceiverModulePrefab.SetGameObject(cyclopsHullObj);
        transceiverModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        transceiverModulePrefab.Register();
        TranceiverModuleTechType = transceiverModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(TranceiverModuleTechType, TranceiverModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub,true);

        //// DSS Integration Module.
        //var assetPath = ModRegistrationService.GetModPackData(PluginInfo.PLUGIN_NAME).GetAssetPath();
        //var dssIntegrationModuleInfo = PrefabInfo.WithTechType("DSSIntegrationModule", "DSS Integration", "N/A").WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(assetPath, "DSSIntegrationModule.png")));
        //var dssIntegerationModulePrefab = new CustomPrefab(dssIntegrationModuleInfo);
        //dssIntegerationModulePrefab.SetGameObject(cyclopsHullObj);
        //dssIntegerationModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        //dssIntegerationModulePrefab.Register();
        //DSSIntegrationModuleTechType = dssIntegrationModuleInfo.TechType;
        //FCSModsAPI.PublicAPI.CreateStoreEntry(DSSIntegrationModuleTechType, DSSIntegrationModuleTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub,true);



    }

    internal static void CreateNewBaseManagerModule(string pluginName,string classID,string friendlyName,string description,decimal itemCost,StoreCategory storeCategory)
    {
        // DSS Integration Module.
        var assetPath = ModRegistrationService.GetModPackData(pluginName).GetAssetPath();
        var dssIntegrationModuleInfo = PrefabInfo.WithTechType(classID, friendlyName, description).WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(assetPath, $"{classID}.png")));
        var dssIntegerationModulePrefab = new CustomPrefab(dssIntegrationModuleInfo);
        var cyclopsHullObj = new CloneTemplate(dssIntegrationModuleInfo, TechType.CyclopsHullModule1);
        dssIntegerationModulePrefab.SetGameObject(cyclopsHullObj);
        dssIntegerationModulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        dssIntegerationModulePrefab.Register();
        DSSIntegrationModuleTechType = dssIntegrationModuleInfo.TechType;
        FCSModsAPI.PublicAPI.CreateStoreEntry(DSSIntegrationModuleTechType, DSSIntegrationModuleTechType, 1, itemCost, storeCategory, true);
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        yield break;
    }
}
