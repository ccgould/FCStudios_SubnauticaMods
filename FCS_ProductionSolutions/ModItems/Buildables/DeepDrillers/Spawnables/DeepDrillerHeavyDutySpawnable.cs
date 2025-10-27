using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Spawnables;
internal class DeepDrillerHeavyDutySpawnable : FCSSpawnableModBase
{
    public static TechType PatchedTechType { get; set; }

    public DeepDrillerHeavyDutySpawnable() : base(PluginInfo.PLUGIN_NAME, "DeepDrillerHeavyDuty2", FileSystemHelper.ModDirLocation, "DeepDrillerHeavyDuty", "Deep Driller Heavy Duty")
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
