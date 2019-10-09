using System.Collections.Generic;

namespace AE.MiniFountainFilter.Configuration
{
    public class Config
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

    public class ModConfiguration
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AssemblyName { get; set; }
        public string EntryMethod { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> LoadAfter { get; set; }
        public Config Config { get; set; }
        public bool Enable { get; set; }
    }
}
