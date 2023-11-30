using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;

internal class DSSServerSpawnable : FCSSpawnableModBase
{
    //public override string AssetsFolder => ModRegistrationService.GetModPackData(PluginInfo.PLUGIN_NAME)?.GetAssetPath();

    public static TechType PatchedTechType { get; private set; }

    public DSSServerSpawnable() : base(PluginInfo.PLUGIN_NAME, "DSS_ServerDataDisc", FileSystemHelper.ModDirLocation, "DSSServer", "Data Disk")
    {
        OnStartRegister += () =>
        {
            PatchedTechType = TechType;
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, TechType, 1, _settings.ItemCost, StoreCategory.Storage,true);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        //prefab.AddComponent<DSSServerController>();

        yield break;
    }
}
