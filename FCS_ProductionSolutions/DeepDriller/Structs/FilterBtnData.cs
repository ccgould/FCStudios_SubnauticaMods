using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.DeepDriller.Mono;

namespace FCS_ProductionSolutions.DeepDriller.Structs
{
    internal struct FilterBtnData
    {
        public TechType TechType { get; set; }
        public FCSToggleButton Toggle { get; set; }
    }
}