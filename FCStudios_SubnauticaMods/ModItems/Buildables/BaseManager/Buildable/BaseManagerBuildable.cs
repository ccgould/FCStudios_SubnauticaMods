using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
internal class BaseManagerBuildable : FCSBuildableModBase
{
    private TechType _kitTechType;
    public static TechType PatchedTechType { get; private set; }
    public BaseManagerBuildable() : base(PluginInfo.PLUGIN_NAME, "fcsBaseManagerScreen", FileSystemHelper.ModDirLocation, "BaseManager", "Base Manager Display")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(ClassID, FriendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            PatchedTechType = TechType;
            //FCSPDAController.AddAdditionalPage<uGUI_IonCube>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_Ioncube", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        yield break;
    }
}
