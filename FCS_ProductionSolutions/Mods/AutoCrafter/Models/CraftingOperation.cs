using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;

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
        public bool IsRecursive { get; set; }
        public bool IsBeingCrafted { get; set; }
        public bool IsOperational { get; set; }
        public HashSet<string> Devices { get; set; } = new HashSet<string>();
        [JsonIgnore] public HashSet<FcsDevice> MountedBy { get; set; } = new HashSet<FcsDevice>();
        public int AmountCompleted { get; set; }
        public TechType TechType { get; set; }
        public bool IsComplete => GetIsComplete();
        public int ReturnAmount { get; set; }
        public Action<CraftingOperation> OnOperationDeleted { get; set; }
        public string ParentMachineUnitID { get; set; }

        private bool GetIsComplete()
        {
            if (IsRecursive) return false;
            return AmountCompleted >= Amount;
        }

        public CraftingOperation()
        {
            
        }

        public CraftingOperation(string unitID, TechType techType, int amount, bool isRecursive)
        {
            ParentMachineUnitID = unitID;
            TechType = techType;
            Amount = amount;
#if SUBNAUTICA
            var returnAmount = CraftDataHandler.Main.GetTechData(techType)?.craftAmount ?? 1;
#else
            var returnAmount = CraftDataHandler.Main.GetRecipeData(techType)?.craftAmount ?? 1;
#endif
            ReturnAmount = returnAmount == 0 ? 1 : returnAmount;
            IsRecursive = isRecursive;
        }

        private void AddDevice(string unitID)
        {
            Devices.Add(unitID);
            if (!IsRecursive)
            {

            }
        }

        private void RemoveDevice(string unitID)
        {
            Devices.Remove(unitID);
        }

        public bool IsSame(CraftingOperation operation)
        {
            return operation.IsRecursive == IsRecursive &&
                   operation.Amount == Amount &&
                   //operation.AssignedUnit.Equals(AssignedUnit, StringComparison.OrdinalIgnoreCase) &&
                   operation.TechType == TechType;
        }

        public bool CanCraft()
        {
            if (IsRecursive && IsBeingCrafted) return false;
            return AmountCompleted < Amount && Devices.Count < Amount;
        }

        public void NotifyIfComplete()
        {
            AmountCompleted++;
        }

        private readonly Dictionary<TechType, TechType> _techTypeFixes = new Dictionary<TechType, TechType>()
        {
            {"FCSGlass".ToTechType(),TechType.Glass }
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
    }
}