using FCSCommon.Utilities;
using System.Collections.Generic;

namespace AE.SeaCooker.Configuration
{
    internal static class Configuration
    {
        internal static readonly string[] SlotIDs = new string[]
            {
                "SCGasTank1"
            };

        private static bool _addingSlots;

        internal static void AddNewSlots()
        {
            if (!_addingSlots)
            {
                foreach (string slotID in SlotIDs)
                {
                    if (slotID.StartsWith("SC"))
                    {
                        Equipment.slotMapping.Add(slotID, EquipmentType.Tank);
                        QuickLogger.Debug($"Adding slot {slotID}");
                    }
                }

                _addingSlots = true;
            }
        }

        internal static readonly List<TechType> AllowedRawItems = new List<TechType>()
        {
            TechType.Peeper,
            TechType.HoleFish,
            TechType.GarryFish,
            TechType.Reginald,
            TechType.Bladderfish,
            TechType.Hoverfish,
            TechType.Spadefish,
            TechType.Boomerang,
            TechType.Eyeye,
            TechType.Oculus,
            TechType.Hoopfish,
            TechType.Spinefish,
            TechType.LavaEyeye,
            TechType.LavaBoomerang,
        };

        internal static readonly List<TechType> AllowedCookedItems = new List<TechType>()
        {
            TechType.CookedPeeper,
            TechType.CookedHoleFish,
            TechType.CookedGarryFish,
            TechType.CookedReginald,
            TechType.CookedBladderfish,
            TechType.CookedHoverfish,
            TechType.CookedSpadefish,
            TechType.CookedBoomerang,
            TechType.CookedEyeye,
            TechType.CookedOculus,
            TechType.CookedHoopfish,
            TechType.CookedSpinefish,
            TechType.CookedLavaEyeye,
            TechType.CookedLavaBoomerang,
        };
    }

    public class Config
    {
        public bool PlaySFX;
        public float AlienFecesTankCapacity { get; set; }
        public float GasTankCapacity { get; set; }
        public float CookTime { get; set; }
        public float EnergyPerSec { get; set; }
        public float UsagePerItem { get; set; }
    }

    public class ModConfiguration
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AssemblyName { get; set; }
        public string EntryMethod { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> LoadAfter { get; set; }
        public Config Config { get; set; }
        public bool Enable { get; set; }
    }
}
