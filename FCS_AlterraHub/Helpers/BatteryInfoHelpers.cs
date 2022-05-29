using System.Collections.Generic;
using System.Linq;

namespace FCS_AlterraHub.Helpers
{
    public static  class BatteryInfoHelpers
    {
        private static HashSet<TechType> batteryChargerCompatibleTech => BatteryCharger.compatibleTech;
        private static HashSet<TechType> powerCellChargerCompatibleTech => PowerCellCharger.compatibleTech;
        public static List<TechType> CompatibleTechConcat => batteryChargerCompatibleTech.Concat(powerCellChargerCompatibleTech).ToList();

        public static bool IsPowercell(TechType techType) => powerCellChargerCompatibleTech.Contains(techType);
        public static bool IsBattery(TechType techType) => batteryChargerCompatibleTech.Contains(techType);
    }
}
