using System.Collections.Generic;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
#endif

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class Shipment
    {
        public string PortPrefabID { get; set; }
        private AlterraDronePortController _port;
        internal AlterraDronePortController Port
        {
            get
            {
                if (_port == null && !string.IsNullOrWhiteSpace(PortPrefabID))
                {
                    _port = BaseManager.FindPort(PortPrefabID);
                }

                return _port;
            }
            set => _port = value;
        }

        public List<CartItemSaveData> CartItems { get; set; }
        public string OrderNumber { get; set; }
    }
}