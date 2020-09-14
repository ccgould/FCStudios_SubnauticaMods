using FCSTechFabricator.Abstract;
using FCSTechFabricator.Enums;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStoragePowerManager : FCSPowerManager
    {
        public override FCSPowerStates GetPowerState()
        {
            return FCSPowerStates.Powered;
        }
    }
}
