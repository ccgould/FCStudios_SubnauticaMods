using FCSCommon.Objects;
using System.Collections.Generic;

namespace FCS_AIMarineTurbine.Configuration
{
    internal class SaveData
    {
        public float FilterHealth { get; set; }

        public List<EatableEntities> FridgeContainer { get; set; }

        public bool HasBreakerTripped { get; set; }
    }
}
