using FCS_AlterraHub.Model;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Configuration
{
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
                Battery.Initialize(0f, Main.Configuration.DDInternalBatteryCapacity);
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
}
