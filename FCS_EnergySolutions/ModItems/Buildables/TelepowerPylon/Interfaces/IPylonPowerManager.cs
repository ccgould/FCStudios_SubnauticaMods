using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Interfaces;
internal interface IPylonPowerManager
{
    void AddConnection(BaseTelepowerPylonManager controller, bool toggleSelf = false);
}
