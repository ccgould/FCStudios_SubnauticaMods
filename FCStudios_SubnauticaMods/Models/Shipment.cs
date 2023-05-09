using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using System.Collections.Generic;

namespace FCS_AlterraHub.Models;

internal class Shipment
{
    public ShipmentInfo Info { get; set; }

    public List<CartItemSaveData> CartItems { get; set; }
}
