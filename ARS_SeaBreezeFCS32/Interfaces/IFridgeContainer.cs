using FCSCommon.Objects;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Interfaces
{
    internal interface IFridgeContainer
    {
        bool IsFull { get; }

        int NumberOfItems { get; }

        void OpenStorage();

        void AttemptToTakeItem(TechType techType);

        List<EatableEntities> FridgeItems { get; }

        Dictionary<TechType, int> TrackedItems { get; }
        int GetTechTypeAmount(TechType techType);
    }
}
