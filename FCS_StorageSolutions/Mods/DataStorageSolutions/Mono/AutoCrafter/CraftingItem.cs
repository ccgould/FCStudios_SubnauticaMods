using Oculus.Newtonsoft.Json;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class CraftingItem
    {
        public TechType TechType { get; set; }
        public float StartTime { get; set; }
        public int Amount { get; set; }
        public int AmountCompleted { get; set; }
        [JsonIgnore] public bool IsBeingCrafted { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsComplete => AmountCompleted >= Amount;
        public CraftingItem(TechType techType)
        {
            TechType = techType;
        }
    }
}