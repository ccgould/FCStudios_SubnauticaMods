namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class CraftingItem
    {
        public TechType TechType { get; set; }
        public float StartTime { get; set; }
        public int Amount { get; set; }

        public CraftingItem(TechType techType)
        {
            TechType = techType;
            StartTime = DayNightCycle.main.timePassedAsFloat;
        }
    }
}