using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;

namespace DataStorageSolutions.Structs
{
    internal struct TransferData
    {
        public bool IsHomeBase { get; set; }
        public BaseManager Manager { get; set; }
    }
}
