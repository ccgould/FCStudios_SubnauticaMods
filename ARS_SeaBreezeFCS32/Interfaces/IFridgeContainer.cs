using FCSCommon.Objects;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Interfaces
{
    internal interface IFridgeContainer
    {
        bool IsFull { get; }

        int NumberOfItems { get; }

        List<EatableEntities> FridgeItems { get; }

        Dictionary<TechType, int> TrackedItems { get; }

        void OpenStorage();

        int GetTechTypeAmount(TechType techType);
    }
}
