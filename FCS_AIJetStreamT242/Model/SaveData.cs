using FCSCommon.Objects;

namespace FCS_AIJetStreamT242.Model
{
    public class SaveData
    {
        public TargetRotation TurbineRot { get; set; }

        public bool HasBreakerTripped { get; set; }

        public float Health { get; set; }

        public float Charge { get; set; }

        public float DegPerSec { get; set; }

        public string Biome { get; set; }

        public float CurrentSpeed { get; set; }

        public float PassedTime { get; set; }
    }


}
