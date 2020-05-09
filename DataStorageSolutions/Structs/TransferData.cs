using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;

namespace DataStorageSolutions.Structs
{
    internal struct TransferData
    {
        public ButtonType ButtonType { get; set; }
        public BaseManager Manager { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
