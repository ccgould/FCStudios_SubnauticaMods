using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Items;
internal class PaintCanSpawnable : FCSSpawnableModBase
{
    //public override string AssetsFolder => ModRegistrationService.GetModPackData(PluginInfo.PLUGIN_NAME)?.GetAssetPath();

    public static TechType PatchedTechType { get; private set; }

    public PaintCanSpawnable() : base(PluginInfo.PLUGIN_NAME, "FCS_PaintCan", FileSystemHelper.ModDirLocation, "PaintCan", "Paint Can")
    {
        OnFinishRegister += () =>
        {
            PatchedTechType = TechType;
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield break;
    }
}
