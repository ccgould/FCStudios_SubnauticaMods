using FCSAlterraShipping.Enums;
using System.Collections.Generic;

namespace FCSAlterraShipping.Models
{
    public class SaveData
    {
        public bool HasBreakerTripped { get; set; }
        public float TimeTillDeletion { get; set; }
        public float TimeLeft { get; set; }
        public Dictionary<TechType, int> ContainerItems { get; set; } = new Dictionary<TechType, int>();
        public string Target { get; set; }
        public int CurrentPage { get; set; }
        public bool CurrentDoorState { get; set; }
        public ShippingContainerStates ContainertMode { get; set; }
        public string ContainerName { get; set; }
    }
}
