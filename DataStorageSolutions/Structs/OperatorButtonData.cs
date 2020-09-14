using DataStorageSolutions.Display;
using FCSTechFabricator.Components;

namespace DataStorageSolutions.Structs
{
    internal struct OperatorButtonData
    {
        internal OperationInterfaceButton Button { get; set; }
        internal FCSConnectableDevice Connectable { get; set; }
        internal TechType TechType { get; set; }
    }
}