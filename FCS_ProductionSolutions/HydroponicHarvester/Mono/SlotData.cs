namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal struct SlotData
    {
        public TechType TechType { get; set; }
        public int SlotId { get; set; }

        public SlotData(TechType techType, int slotId)
        {
            TechType = techType;
            SlotId = slotId;
        }
    }
}