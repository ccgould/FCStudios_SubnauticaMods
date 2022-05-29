namespace FCS_ProductionSolutions.Structs
{
    internal struct FCSDNASampleData
    {
        public TechType TechType { get; set; }
        public TechType PickType { get; set; }
        public bool IsLandPlant { get; set; }

        public bool IsSame(TechType techType)
        {
            return TechType == techType || PickType == techType;
        }
    }
}