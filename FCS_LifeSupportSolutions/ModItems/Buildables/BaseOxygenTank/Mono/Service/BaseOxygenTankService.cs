using System.Collections.Generic;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono.Service;
internal class BaseOxygenTankService : MonoBehaviour
{
    public static BaseOxygenTankService main;

    private Dictionary<string,BaseOxygenTankManager> managers = new();

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(this);
        }
        else
        {
            main = this;
        }
    }

    internal void RegisterBaseOxygenTankManager(string prefabID,BaseOxygenTankManager manager)
    {
        if (managers.ContainsKey(prefabID)) return;
        managers.Add(prefabID, manager);
    }

    internal void UnRegisterBaseOxygenTankManager(string prefabID)
    {
        managers.Remove(prefabID);
    }

    internal BaseOxygenTankManager GetBaseOxygenTankManager(string prefabID)
    {
        if(managers.ContainsKey(prefabID))
        {
            return managers[prefabID];
        }
        return null;
    }
}
