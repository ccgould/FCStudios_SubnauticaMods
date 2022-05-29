using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Model
{
    public class PowercellData
    {
        [JsonProperty] private float Charge { get; set; }
        [JsonProperty] private float Capacity { get; set; }

        public void Initialize(float charge, float capacity)
        {
            Charge = charge;
            Capacity = capacity;
        }

        public float GetCharge()
        {
            return Charge;
        }

        public void RemoveCharge(float amount)
        {
            if (Charge <= 0) return;
            Charge = Mathf.Clamp(Charge - amount, 0, Capacity);
        }

        public void AddCharge(float amount)
        {
            if (Charge >= Capacity) return;
            Charge = Mathf.Clamp(Charge + amount, 0, Capacity);
        }

        public PowercellData Save()
        {
            return this;
        }

        public bool IsFull()
        {
            return Charge >= Capacity;
        }

        public float GetCapacity()
        {
            return Capacity;
        }

        public void Flush()
        {
            Charge = 0f;
        }

        public bool IsEmpty()
        {
            return Charge <= 0;
        }
    }
}
