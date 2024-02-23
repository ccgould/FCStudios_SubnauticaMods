using FCS_AlterraHub.Models.Abstract;
using System.Collections.Generic;
using System;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Interfaces;
internal interface ITelepowerPylonConnection
{
    TelepowerPylonMode GetCurrentMode();
    string UnitID { get; set; }
    Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }
    IPowerInterface GetPowerRelay();
    FCSDevice GetDevice();
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
