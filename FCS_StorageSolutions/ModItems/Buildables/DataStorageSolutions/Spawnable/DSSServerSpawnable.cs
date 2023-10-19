using FCS_AlterraHub.Models.Abstract;
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
        OnFinishRegister += () =>
        {
            PatchedTechType = TechType;
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        //prefab.AddComponent<DSSServerController>();

        yield break;
    }
}
