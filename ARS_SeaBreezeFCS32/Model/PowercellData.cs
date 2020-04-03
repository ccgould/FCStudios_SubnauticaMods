using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class PowercellData
    {
        [JsonProperty] private float Charge { get; set; }
        [JsonProperty] private float Capacity { get; set; }

        internal void Initialize(float charge, float capacity)
        {
            Charge = charge;
            Capacity = capacity;
        }

        internal float GetCharge()
        {
            return Charge;
        }

        internal void RemoveCharge(float amount)
        {
            if (Charge <= 0) return;
            Charge = Mathf.Clamp(Charge - amount, 0, Capacity);
        }

        internal void AddCharge(float amount)
        {
            if (Charge >= Capacity) return;
            Charge = Mathf.Clamp(Charge + amount, 0, Capacity);
        }

        internal PowercellData Save()
        {
            return this;
        }

        internal bool IsFull()
        {
            return Charge >= Capacity;
        }

        public float GetCapacity()
        {
            return Capacity;
        }
    }
}
