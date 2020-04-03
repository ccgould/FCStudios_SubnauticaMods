using System;
using System.Collections.Generic;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace ARS_SeaBreezeFCS32.Configuration
{
    [Serializable]
    internal class LegacySaveData
    {
        public float RemainingTime { get; set; }

        public int FreonCount { get; set; }

        public IEnumerable<EatableEntities> FridgeContainer { get; set; }

        public bool HasBreakerTripped { get; set; }

        public string UnitName { get; set; }
        public ColorVec4 BodyColor { get; set; }
    }
}
