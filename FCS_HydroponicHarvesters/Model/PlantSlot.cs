using FCSTechFabricator.Components;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Model
{
    internal class PlantSlot
    {
        public PlantSlot(int id, Transform slot)
        {
            this.id = id;
            this.slot = slot;
        }

        public void Clear()
        {
            this.plantable = null;
            this.plantModel = null;
        }
        
        public readonly int id;

        public readonly Transform slot;

        public bool isOccupied;

        public FCSDNA plantable;

        public GameObject plantModel;
	}
}
