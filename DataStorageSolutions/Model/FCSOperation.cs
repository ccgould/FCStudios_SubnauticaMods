using FCSTechFabricator.Components;

namespace DataStorageSolutions.Model
{
    internal class FCSOperation
    {
        public FCSConnectableDevice FromDevice { get; set; }
        public FCSConnectableDevice ToDevice { get; set; }
        public TechType TechType { get; set; }
        public BaseManager Manager { get; set; }
        public bool IsCraftable { get; set; }
    }
}