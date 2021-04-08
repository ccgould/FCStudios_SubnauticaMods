using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCSCommon.Extensions;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;

namespace FCS_AlterraHub.Mono
{
    public class CraftingOperation
    {
        public int Amount { get; set; }
        public bool IsRecursive { get; set; }
        public bool IsBeingCrafted { get; set; }
        public bool IsOperational { get; set; }
        public HashSet<string> Devices { get; set; } = new HashSet<string>();
        [JsonIgnore]public HashSet<FcsDevice> MountedBy { get; set; } = new HashSet<FcsDevice>();
        public int AmountCompleted { get; set; }
        public TechType TechType { get; set; }
        public bool IsComplete => GetIsComplete();
        public int ReturnAmount { get; set; }

        private bool GetIsComplete()
        {
            if (IsRecursive) return false;
            return AmountCompleted >= Amount;
        }

        public CraftingOperation()
        {
            
        }

        public CraftingOperation(TechType techType, int amount, bool isRecursive)
        {
            TechType = techType;
            Amount = amount;
            ReturnAmount = CraftDataHandler.Main.GetTechData(techType)?.craftAmount ?? 1;
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