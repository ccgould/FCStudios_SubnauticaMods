using FCS_AlterraHub.Core.Components;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
internal class DeepDrillerPowerData
{
    public PowercellData SolarPanel { get; set; }
    public PowercellData Battery { get; set; }
    public PowercellData Thermal { get; set; }

    public DeepDrillerPowerData()
    {
        if (Battery == null)
        {
            Battery = new PowercellData();
            Battery.Initialize(0f, Plugin.Configuration.DDInternalBatteryCapacity);
        }

        if (SolarPanel == null)
        {
            SolarPanel = new PowercellData();
            SolarPanel.Initialize(0f, 75 * 12);
        }

        if (Thermal == null)
        {
            Thermal = new PowercellData();
            Thermal.Initialize(0f, 250);
        }
    }
}
