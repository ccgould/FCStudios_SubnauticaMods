using FCSCommon.Enums;
using SMLHelper.V2.Handlers;

namespace ARS_SeaBreezeFCS32.Helpers
{
    internal static class TechTypeHelper
    {
        internal static TechType Find(FilterTypes type)
        {
            TechType filterTechType = TechType.None;

            switch (type)
            {
                case FilterTypes.None:
                    filterTechType = TechType.None;
                    break;
                case FilterTypes.LongTermFilter:
                    TechTypeHandler.TryGetModdedTechType("LongTermFilter_ARS", out TechType longTerm);
                    filterTechType = longTerm;
                    break;
                case FilterTypes.ShortTermFilter:
                    TechTypeHandler.TryGetModdedTechType("ShortTermFilter_ARS", out TechType shortTerm);
                    filterTechType = shortTerm;
                    break;
            }

            return filterTechType;
        }
    }
}
