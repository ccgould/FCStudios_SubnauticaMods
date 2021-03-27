using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCSCommon.Extensions;

namespace FCS_AlterraHub.Mono
{
    public class CraftingOperation
    {
        public int Amount { get; set; }
        public bool IsRecursive { get; set; }
        public bool IsBeingCrafted { get; set; }
        public bool IsOperational { get; set; }
        public HashSet<string> Devices { get; set; } = new HashSet<string>();
        public int AmountCompleted { get; set; }
        public List<IIngredient> Ingredients { get; set; } = new List<IIngredient>();
        public TechType TechType { get; set; }
        public bool IsComplete => Amount >= AmountCompleted;

        public CraftingOperation()
        {
            
        }

        public CraftingOperation(TechType techType, int amount, bool isRecursive)
        {
            TechType = techType;
            Amount = amount;
            IsRecursive = isRecursive;
            UpdateIngredients();
        }

        public void UpdateIngredients()
        {
            if(TechType == TechType.None) return;

            if (Ingredients.Any())
            {
                Ingredients.Clear();
            }

            Ingredients = TechDataHelpers.GetIngredients(TechType);
        }
        
        public void AddDevice(string unitID)
        {
            Devices.Add(unitID);
            if (!IsRecursive)
            {

            }
        }

        public void RemoveDevice(string unitID)
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
            return AmountCompleted < Amount && Devices.Count <= Amount;
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
    }
}