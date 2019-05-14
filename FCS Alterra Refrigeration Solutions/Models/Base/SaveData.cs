using System.Collections.Generic;
using FCS_Alterra_Refrigeration_Solutions.Enums;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    public class SaveData
    {
        public bool IsEnabled { get; set; }
        public bool RemovedDecompition { get; set; }
        public bool HasBreakerTripped { get; set; }
        public bool ContainerHasBeenOpened { get; set; }
        public bool ContainerHasBeenClosed { get; set; }
        public bool Decomposing { get; set; }
        public bool PowerAvaliable { get; set; }
        public bool NoPowerAvaliablePrev { get; set; }
        public bool OldFilter { get; set; }
        public bool IsUsingFilter { get; set; }
        public bool PreviousDecomposingState { get; set; }
        public bool AllowTick { get; set; }
        public float MaxTime { get; set; }
        public TechType ItemID { get; set; }
        public PowerState PowerState { get; set; }
        public Dictionary<string, EatableData> SeaBreezeTracking { get; set; } = new Dictionary<string, EatableData>();
    }
}
