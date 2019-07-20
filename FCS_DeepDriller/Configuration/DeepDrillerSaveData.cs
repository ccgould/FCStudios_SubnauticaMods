using FCS_DeepDriller.Enumerators;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    [Serializable]
    public class DeepDrillerSaveDataEntry
    {
        public Vector3 DrillBitPosition { get; set; }

        public DeepDrillModules Module { get; set; }

        public float PowerAmount { get; set; }

        public bool HasBreakerTripped { get; set; }

        public float Health { get; set; }

        public string Id { get; set; }
    }

    [Serializable]
    public class DeepDrillerSaveData
    {
        public List<DeepDrillerSaveDataEntry> Entries = new List<DeepDrillerSaveDataEntry>();
    }
}
