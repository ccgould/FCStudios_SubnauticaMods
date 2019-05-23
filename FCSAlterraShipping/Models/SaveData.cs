using System.Collections.Generic;

namespace FCSAlterraShipping.Models
{
    public class SaveData
    {
        public bool HasBreakerTripped { get; set; }
        public float TimeTillDeletion { get; set; }
        public float TimeLeft { get; set; }
        public Dictionary<TechType, int> ContainerItems { get; set; } = new Dictionary<TechType, int>();
    }
}
