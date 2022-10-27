﻿using System.Collections.Generic;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
#if SUBNAUTICA_STABLE

#else
#endif

namespace FCS_AlterraHub.Mods.Common.DroneSystem.Models
{
    internal class Shipment
    {
        public string PortPrefabID { get; set; }
        private IDroneDestination _port;
        internal IDroneDestination Port
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