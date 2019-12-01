using System.Collections.Generic;

namespace AE.MiniFountainFilter.Configuration
{
    public class ModConfiguration
    {
        public bool PlaySFX;
        public float TankCapacity { get; set; }
        public float EnergyPerSec { get; set; }
        public int StorageWidth { get; set; }
        public int StorageHeight { get; set; }
        public float WaterPerSecond { get; set; }
        public bool AutoGenerateMode { get; set; }
        public string BottleTechType { get; set; }
    }
}
