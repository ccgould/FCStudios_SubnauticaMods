using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Interfaces
{
    internal interface ITelepowerPylonConnection
    {
        TelepowerPylonMode GetCurrentMode();
        string UnitID { get; set; }
        Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }
        IPowerInterface GetPowerRelay();
        FcsDevice GetDevice();
        void DeleteFrequencyItemAndDisconnectRelay(string targetControllerUnitId);
        bool HasConnection(string unitKey);
        bool HasPowerRelayConnection(IPowerInterface getPowerRelay);
        BaseTelepowerPylonManager GetPowerManager();
        bool CanAddNewPylon();
        void ActivateItemOnPushGrid(ITelepowerPylonConnection controller);
        void ActivateItemOnPullGrid(ITelepowerPylonConnection controller);
        bool CheckIsPullingFrom(ITelepowerPylonConnection controller);
        List<string> GetBasePylons();
    }
}