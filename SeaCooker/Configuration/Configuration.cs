using FCSCommon.Utilities;
using System.Collections.Generic;
using Oculus.Newtonsoft.Json;

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

    internal class Config
    {
        [JsonProperty] internal bool PlaySFX { get; set; } 
        [JsonProperty] internal float AlienFecesTankCapacity { get; set; }
        [JsonProperty] internal float GasTankCapacity { get; set; }
        [JsonProperty] internal float CookTime { get; set; }
        [JsonProperty] internal float EnergyPerSec { get; set; }
        [JsonProperty] internal float UsagePerItem { get; set; }
        [JsonProperty] internal int StorageWidth { get; set; }
        [JsonProperty] internal int StorageHeight { get; set; }
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
