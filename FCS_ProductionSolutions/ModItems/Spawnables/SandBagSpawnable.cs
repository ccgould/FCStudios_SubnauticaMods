using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Spawnables;
internal class SandBagSpawnable : FCSSpawnableModBase
{
    public static TechType PatchedTechType { get; set; }

    public SandBagSpawnable() : base(PluginInfo.PLUGIN_NAME, "DD_SandOre", FileSystemHelper.ModDirLocation, "SandBag", "Sand Bag")
    {
        OnFinishRegister += () =>
        {
            PatchedTechType = TechType;
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield return null;
    }
}
