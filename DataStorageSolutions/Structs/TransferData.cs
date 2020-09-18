using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;

namespace DataStorageSolutions.Structs
{
    internal struct TransferData
    {
        public ButtonType ButtonType { get; set; }
        public BaseManager Manager { get; set; }
        public Vehicle Vehicle { get; set; }
        public ItemsContainer Container { get; set; }

    }
}
