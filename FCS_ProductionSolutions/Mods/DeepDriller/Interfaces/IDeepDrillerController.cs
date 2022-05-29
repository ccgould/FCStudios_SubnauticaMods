using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Interfaces
{
    internal interface IDeepDrillerController
    {
        DumpContainer OilDumpContainer { get; set; }
        bool IsOperational { get; }
        FCSDeepDrillerContainer DeepDrillerContainer { get; set; }
        bool IsConstructed { get; set; }
        bool IsInitialized { get; set; }
        bool IsBreakSet();
        bool IsPowerAvailable();
        IEnumerable<UpgradeFunction> GetUpgrades();
        bool GetIsDrilling();
        void StopSFX();
        List<PistonBobbing> GetPistons();
        void PlaySFX();
    }
}
