using FCS_ProductionSolutions.Mods.DeepDriller.Configuration;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerThermalController : MonoBehaviour
    {
        private DeepDrillerPowerData _powerData;
        private const float MaxEnergyPerSec = 3.80f;
        private const float QueryTempInterval = 10f;
        private const float AddPowerInterval = 2f;

        public float Temperature { get; private set; }

        internal void Initialize(DeepDrillerPowerData powerData)
        {
            _powerData = powerData;
            InvokeRepeating("QueryTemperature", UnityEngine.Random.value, QueryTempInterval);
			InvokeRepeating("AddPower", UnityEngine.Random.value, 1f);
        }

		private void QueryTemperature()
		{
			WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
			if (main)
			{
				Temperature = Mathf.Max(Temperature, main.GetTemperature(transform.position));
            }
		}

		public void AddPower()
		{
			if (this.Temperature > 25f)
			{
				float addPowerInterval = AddPowerInterval * DayNightCycle.main.dayNightSpeed;
				float amount = MaxEnergyPerSec * addPowerInterval * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, this.Temperature));
                _powerData.Thermal.AddCharge(amount);
            }
		}
    }
}
