using System.Collections.Generic;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Managers;

#if SUBNAUTICA_STABLE

#else
#endif

namespace FCS_AlterraHub.Mods.Common.DroneSystem.Models
{
    internal class Shipment
    {
        public ShipmentInfo Info { get; set; }

        public List<CartItemSaveData> CartItems { get; set; }
    }
}