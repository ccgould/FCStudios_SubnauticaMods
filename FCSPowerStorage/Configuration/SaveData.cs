using FCSPowerStorage.Utilities.Enums;
using System;
using FCSCommon.Objects;

namespace FCSPowerStorage.Configuration
{
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// The charge of the object
        /// </summary>
        public float Charge { get; set; }
        public float StoredPower { get; set; }
        public float BatteryStatus1Bar { get; set; }
        public string BatteryStatus1Percentage { get; set; }
        public float BatteryStatus2Bar { get; set; }
        public string BatteryStatus2Percentage { get; set; }
        public float BatteryStatus3Bar { get; set; }
        public string BatteryStatus3Percentage { get; set; }
        public float BatteryStatus4Bar { get; set; }
        public string BatteryStatus4Percentage { get; set; }
        public float BatteryStatus5Bar { get; set; }
        public string BatteryStatus5Percentage { get; set; }
        public float BatteryStatus6Bar { get; set; }
        public string BatteryStatus6Percentage { get; set; }
        public bool HasBreakerTripped { get; set; }
        public PowerToggleStates ChargeMode { get; set; }
        public ColorVec4 BodyColor { get; set; }
    }
}
