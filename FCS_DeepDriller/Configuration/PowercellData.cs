using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal class PowercellData
    {
        [JsonProperty] internal float Charge { get; set; }
        [JsonProperty] internal float Capacity { get; set; }

        internal bool IsFull()
        {
            return Charge >= Capacity;
        }

        public void TakePower(float amount)
        {
            Charge = Mathf.Clamp(Charge - amount, 0,Capacity);
        }

        public void ChargeBattery(float amount)
        {
            Charge = Mathf.Clamp(Charge + amount, 0, Capacity);
        }
    }
}
