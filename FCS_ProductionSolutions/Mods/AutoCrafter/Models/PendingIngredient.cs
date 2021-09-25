
namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models
{
    internal class PendingIngredient : IIngredient
    {
        public TechType techType { get; }
        public int amount { get; }
    }
}
