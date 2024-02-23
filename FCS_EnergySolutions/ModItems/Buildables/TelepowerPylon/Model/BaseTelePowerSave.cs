using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using System.Collections.Generic;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;

internal class BaseTelePowerSave
{
    public int Id { get; set; }
    public TelepowerPylonMode Mode { get; set; } = TelepowerPylonMode.PUSH;
    public List<string> Connections { get; set; } = new();
    public TelepowerPylonUpgrade Upgrade { get; set; } = TelepowerPylonUpgrade.MK1;
}
