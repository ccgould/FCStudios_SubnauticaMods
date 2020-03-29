using System;
using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace FCSTechFabricator.Interfaces
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

        Action<int, int> OnContainerUpdate { get; set; }
    }
}
