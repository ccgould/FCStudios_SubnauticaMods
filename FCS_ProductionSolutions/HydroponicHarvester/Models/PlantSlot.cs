using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Models
{
    internal class PlantSlot
    {
        public PlantSlot(int id, Transform slot)
        {
            Id = id;
            Slot = slot;
            //PlantModel = slot.GetChild(0).gameObject;
        }

        public Plantable Plantable { get; set; }

        public void Clear()
        {
            PlantModel = null;
            Plantable = null;
        }

        public readonly int Id;

        public readonly Transform Slot;

        public bool IsOccupied;

        public GameObject PlantModel;

        public void ShowPlant()
        {
            var model = GameObject.Instantiate(PlantModel);
            model.transform.SetParent(Slot,false);
            model.transform.localPosition = Vector3.zero;
        }

        public void SetMaxPlantHeight(float height)
        {
            if (height <= 0f || Plantable == null)
            {
                return;
            }
            Plantable.SetMaxPlantHeight(height - Slot.localPosition.y);
        }
    }
}
