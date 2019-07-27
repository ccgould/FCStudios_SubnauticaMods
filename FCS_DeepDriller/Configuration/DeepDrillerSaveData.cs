using FCSCommon.Objects;
using FCSCommon.Utilities.Enums;
using System;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    [Serializable]
    public class DeepDrillerSaveDataEntry
    {
        public Vec3 DrillBitPosition { get; set; }

        public List<SlotData> Modules { get; set; }

        public float PowerAmount { get; set; }

        public float Health { get; set; }

        public string Id { get; set; }

        public FCSPowerStates PowerState { get; set; }

        public IEnumerable<KeyValuePair<TechType, int>> Items { get; set; }

        public DeepDrillerPowerData PowerData { get; set; }
        public TechType FocusOre { get; set; }
        public bool IsFocused { get; set; }
    }

    [Serializable]
    public class DeepDrillerSaveData
    {
        public List<DeepDrillerSaveDataEntry> Entries = new List<DeepDrillerSaveDataEntry>();
    }
}
