using System;

namespace FCS_AlterraHub.Mono
{
    public class CraftingOperation
    {
        public string AssignedUnit { get; set; }
        public int Amount { get; set; }
        public TechType TechType { get; set; }
        public bool IsRecursive { get; set; }
        public bool IsBeingCrafted { get; set; }

        public bool IsSame(CraftingOperation operation)
        {
            return operation.IsRecursive == IsRecursive &&
                   operation.Amount == Amount &&
                   operation.AssignedUnit.Equals(AssignedUnit, StringComparison.OrdinalIgnoreCase) &&
                   operation.TechType == TechType;
        }
    }
}