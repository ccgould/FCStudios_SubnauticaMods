using FCS_AlterraHub.Helpers;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_HomeSolutions.TrashRecycler.Model
{
    public class Waste
    {
        public TechType TechType { get; set; }
        public double TimeAdded;
        public int IngredientCount { get; set; }

        public Waste()
        {
            
        }

        public Waste(InventoryItem inventoryItem, double timeAdded)
        {
            TechType = inventoryItem.item.GetTechType();
            IngredientCount = TechDataHelpers.GetIngredientCount(inventoryItem.item);
            TimeAdded = timeAdded;
        }
    }
}