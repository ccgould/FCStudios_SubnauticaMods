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

        internal static void CreateConfiguration()
        {

        }

        internal static void AddNewSlots()
        {
            if (!_addingSlots)
            {
                foreach (string slotID in SlotIDs)
                {
                    if (slotID.StartsWith("SC"))
                    {
                        Equipment.slotMapping.Add(slotID, EquipmentType.NuclearReactor);
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
            TechType.CookedLavaBoomerang
        };
    }

    internal class Config
    {
        [JsonProperty] internal bool PlaySFX { get; set; } = true; 
        [JsonProperty] internal float AlienFecesTankCapacity { get; set; } = 250.0f;
        [JsonProperty] internal float GasTankCapacity { get; set; } = 100.0f;
        [JsonProperty] internal float CookTime { get; set; } = 5.0f;
        [JsonProperty] internal float EnergyPerSec { get; set; } = 0.102f;
        [JsonProperty] internal float UsagePerItem { get; set; } = 5.0f;
        [JsonProperty] internal int StorageWidth { get; set; } = 4;
        [JsonProperty] internal int StorageHeight { get; set; } = 4;
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }
}
