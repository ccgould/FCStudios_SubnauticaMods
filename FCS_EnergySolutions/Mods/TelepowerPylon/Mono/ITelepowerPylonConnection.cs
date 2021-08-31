using System;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal interface ITelepowerPylonConnection
    {
        TelepowerPylonMode GetCurrentMode();
        string UnitID { get; set; }
        Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }
        void AddItemToPullGrid(ITelepowerPylonConnection telepowerPylonConnection,bool isChecked = false);
        void AddItemToPushGrid(ITelepowerPylonConnection telepowerPylonConnection,bool isChecked = false);
        void AddPylonToPushGrid(ITelepowerPylonConnection telepowerPylonConnection);
        IPowerInterface GetPowerRelay();
        FcsDevice GetDevice();
        void DeleteFrequencyItemAndDisconnectRelay(string targetControllerUnitId);
        bool HasConnection(string unitKey);
        bool HasPowerRelayConnection(IPowerInterface getPowerRelay);
        IPylonPowerManager GetPowerManager();
        bool CanAddNewPylon();
    }
}