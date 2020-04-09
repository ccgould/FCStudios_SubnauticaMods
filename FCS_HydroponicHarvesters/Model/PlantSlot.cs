using FCSTechFabricator.Components;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Model
{
    internal class PlantSlot
    {
        public PlantSlot(int id, Transform slot)
        {
            Id = id;
            Slot = slot;
            //PlantModel = slot.GetChild(0).gameObject;
        }

        public void Clear()
        {
            Plantable = null;
            PlantModel = null;
        }
        
        public readonly int Id;

        public readonly Transform Slot;

        public bool IsOccupied;

        public FCSDNA Plantable;

        public GameObject PlantModel;
	}
}
