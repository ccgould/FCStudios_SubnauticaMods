using System.Collections.Generic;
using FCSCommon.Extensions;
using Oculus.Newtonsoft.Json;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class CraftingItem
    {
        private readonly Dictionary<TechType, TechType> _techTypeFixes = new Dictionary<TechType, TechType>()
        {
            {"FCSGlass".ToTechType(),TechType.Glass }
        };

        public TechType TechType { get; set; }
        public float StartTime { get; set; }
        public int Amount { get; set; }
        public int AmountCompleted { get; set; }
        [JsonIgnore] public bool IsBeingCrafted { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsComplete => AmountCompleted >= Amount;
        public CraftingItem(TechType techType)
        {
            TechType = techType;
        }

        internal TechType FixCustomTechType()
        {
            return _techTypeFixes.ContainsKey(TechType) ? _techTypeFixes[TechType] : TechType;
        }
    }
}