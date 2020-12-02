using FCS_ProductionSolutions.DeepDriller.Mono;
using FCSCommon.Components;

namespace FCS_ProductionSolutions.DeepDriller.Structs
{
    internal struct FilterBtnData
    {
        public TechType TechType { get; set; }
        public FCSToggleButton Toggle { get; set; }
    }
}