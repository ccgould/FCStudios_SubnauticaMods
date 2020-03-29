using FCSCommon.Objects;
using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace FCS_AIMarineTurbine.Configuration
{
    internal class SaveData
    {
        public List<EatableEntities> FridgeContainer { get; set; }
        public List<EatableEntities> RottenContainer { get; set; }
        public bool HasBreakerTripped { get; set; }
    }
}
