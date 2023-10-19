using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.TestObject.Buildable;

internal class TestBuildable : FCSBuildableModBase
{
    public TestBuildable() : base(PluginInfo.PLUGIN_NAME, "MaterialsShowcaseDemo", FileSystemHelper.ModDirLocation, "MaterialsShowcaseDemo", "Materials Showcase Demo")
    {

    }


    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        //prefab.AddComponent<TestController>();
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        yield break;
    }
}
