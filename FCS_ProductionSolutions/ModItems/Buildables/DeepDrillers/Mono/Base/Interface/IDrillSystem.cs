using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base.Interface;

public interface IDrillSystem
{
    bool IsBreakSet();
    bool IsOperational();
    int GetOresPerDayCountInt();
    IEnumerable<UpgradeFunction> GetUpgrades();
    string UnitID { get; }
    GameObject GameObject { get; }

    string GetPrefabID();
    string GetOilLevel();
    void AddItemToContainer(TechType techType);
    void RemoveItemFromContainer(TechType techType);
}
