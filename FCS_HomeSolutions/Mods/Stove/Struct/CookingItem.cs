namespace FCS_HomeSolutions.Mods.Stove.Struct
{
    internal struct CookingItem
    {
        public TechType TechType { get; set; }
        public TechType CookedItem { get; set; }
        public TechType CuredItem { get; set; }
        public TechType CustomItem { get; set; }
        public TechType ReturnItem { get; set; }
    }
}
