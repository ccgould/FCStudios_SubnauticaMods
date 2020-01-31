namespace AE.MiniFountainFilter.Configuration
{
    public class ModConfiguration
    {
        public bool PlaySFX { get; set; } = true;
        public float TankCapacity { get; set; } = 100f;
        public float EnergyPerSec { get; set; } = 0.425f;
        public int StorageWidth { get; set; } = 3;
        public int StorageHeight { get; set; } = 2;
        public float WaterPerSecond { get; set; } = 1f;
        public bool AutoGenerateMode { get; set; } = false;
        public string BottleTechType { get; set; } = "FilteredWater";
    }
}
