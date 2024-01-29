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
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseModuleRack.Mono;

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
        // Remote Module.
        RemoteModuleTechType = CreateNewBaseManagerModule(PluginInfo.PLUGIN_NAME, "RemoteConnectionModule", "Remote Connection Module", "Allows the FCS PDA to see all base devices from the Base Manager remotely.",9000,StoreCategory.AlterraHub);

        // Transceiver Module.
        TranceiverModuleTechType = CreateNewBaseManagerModule(PluginInfo.PLUGIN_NAME, "TransceiverModule", "Tranceiver Module", "Supports 5 Automation Operations (See Encyclopedia) (Req Module Rack)", 9000, StoreCategory.AlterraHub);
    }

    internal static TechType CreateNewBaseManagerModule(string pluginName,string classID,string friendlyName,string description,decimal itemCost,StoreCategory storeCategory)
    {
        // DSS Integration Module.
        var assetPath = ModRegistrationService.GetModPackData(pluginName).GetAssetPath();
        var moduleInfo = PrefabInfo.WithTechType(classID, friendlyName, description).WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(assetPath, $"{classID}.png")));
        var modulePrefab = new CustomPrefab(moduleInfo);
        var cyclopsHullObj = new CloneTemplate(moduleInfo, TechType.CyclopsHullModule1);
        modulePrefab.SetGameObject(cyclopsHullObj);
        modulePrefab.SetEquipment(BaseManagerModuleRackBuildable.BaseManagerEquipmentType);
        modulePrefab.Register();
        FCSModsAPI.PublicAPI.CreateStoreEntry(moduleInfo.TechType, moduleInfo.TechType, 1, itemCost, storeCategory, true);
        BaseManagerModuleRackBuildable.allowedTech.Add(moduleInfo.TechType);
        return moduleInfo.TechType;

    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        yield break;
    }
}
