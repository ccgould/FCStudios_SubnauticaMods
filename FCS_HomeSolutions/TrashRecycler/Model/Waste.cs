﻿using FCS_AlterraHub.Helpers;
using Oculus.Newtonsoft.Json;

namespace FCS_HomeSolutions.TrashRecycler.Model
{
    public class Waste
    {
        [JsonIgnore]public InventoryItem InventoryItem;
        public TechType TechType { get; set; }
        public double TimeAdded;
        public int IngredientCount { get; set; }

        public Waste()
        {
            
        }

        public Waste(InventoryItem inventoryItem, double timeAdded)
        {
            InventoryItem = inventoryItem;
            TechType = inventoryItem.item.GetTechType();
            IngredientCount = TechDataHelpers.GetIngredientCount(inventoryItem.item);
            TimeAdded = timeAdded;
        }
    }
}