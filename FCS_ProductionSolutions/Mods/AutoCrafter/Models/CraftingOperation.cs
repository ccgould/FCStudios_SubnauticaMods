using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using SMLHelper.V2.Handlers;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models
{
    public class CraftingOperation
    {
        public int Amount { get; set; }
        public HashSet<string> Devices { get; set; } = new();
        public List<TechType> LinkedItems { get; set; } = new();
        [JsonIgnore] public HashSet<FcsDevice> MountedBy { get; set; } = new();
        public int AmountCompleted { get; set; }
        public TechType TechType { get; set; }
        public bool IsComplete => GetIsComplete();
        public int ReturnAmount { get; set; }
        public int LinkedItemCount { get; }
        public string ParentMachineUnitID { get; set; }
        internal AutoCrafterController Crafter => GetMachine();
        private AutoCrafterController _device;


        private AutoCrafterController GetMachine()
        {
            if (_device == null)
            {
                var fcsDevice = FCSAlterraHubService.PublicAPI.FindDevice(ParentMachineUnitID).Value;
                
                if (fcsDevice != null)
                {
                    _device = (AutoCrafterController)fcsDevice;
                }
            }
            
            return _device;

        }

        private bool GetIsComplete()
        {
            if (Crafter?.GetIsRecursive() ?? false) return false;
            return AmountCompleted >= Amount;
        }

        public CraftingOperation()
        {
        }

        public CraftingOperation(string unitID, TechType techType, int amount)
        {
            ParentMachineUnitID = unitID;
            TechType = techType;
            Amount = amount;
            var data = CraftDataHandler.Main.GetTechData(techType);
            var returnAmount = data?.craftAmount ?? 1;
            var linkedItems = data?.LinkedItems;
            if (linkedItems != null)
            {
                LinkedItems = linkedItems;
            }

            LinkedItemCount = linkedItems?.Count ?? 0;
            ReturnAmount = returnAmount == 0 ? 1 : returnAmount;
        }


        private void AddDevice(string unitID)
        {
            Devices.Add(unitID);
        }

        private void RemoveDevice(string unitID)
        {
            Devices.Remove(unitID);
        }

        private readonly Dictionary<TechType, TechType> _techTypeFixes = new()
        {
            {"FCSGlass".ToTechType(), TechType.Glass}
        };

        private readonly List<TechType> _techTypeBypasses = new()
        {
            {"FCSGlass".ToTechType()}
        };
        
        public TechType FixCustomTechType()
        {
            return _techTypeFixes.ContainsKey(TechType) ? _techTypeFixes[TechType] : TechType;
        }

        public bool IsMounted()
        {
            return MountedBy.Any();
        }

        public void Mount(FcsDevice device)
        {
            MountedBy.Add(device);
            AddDevice(device.UnitID);
        }

        public void UnMount(FcsDevice device)
        {
            MountedBy.Remove(device);
            RemoveDevice(device.UnitID);
        }

        public void AppendCompletion()
        {
            if (AmountCompleted < Amount)
            {
                AmountCompleted += 1;
            }
        }

        public bool OriginalBypass()
        {
            return _techTypeBypasses.Contains(TechType);
        }
    }
}