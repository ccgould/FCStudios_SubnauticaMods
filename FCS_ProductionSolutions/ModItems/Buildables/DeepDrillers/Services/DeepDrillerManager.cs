using FCS_AlterraHub.Core.Services;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Services;
internal class DeepDrillerManager : MonoBehaviour
{
    public static DeepDrillerManager main { get; private set; }
    private Dictionary<string, DeepDrillerHeavyDutyController> drills = new();

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            main = this;
        }
    }

    internal void RegisterDrill(DeepDrillerHeavyDutyController drill)
    {
        if(!drills.ContainsKey(drill.GetPrefabID()))
        {
            drills.Add(drill.GetPrefabID(), drill);
        }
    }

    internal void UnRegisterDrill(DeepDrillerHeavyDutyController drill)
    {
        drills.Remove(drill.GetPrefabID());
    }

    internal List<ConnectedDrillData> Save()
    {
        return null;
    }
}
