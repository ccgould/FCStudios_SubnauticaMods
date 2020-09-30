using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStorageSolutions.Interfaces
{
    internal interface ISlot
    {
        void ConnectServer(Pickupable server);
        ISlottedDevice GetConnectedDevice();
    }
}
