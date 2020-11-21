using System.CodeDom;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Models
{
    internal class PlantSlot
    {
        internal GameObject SlotBounds { get; set; }
        internal Plantable Plantable { get; set; }

        public TechType ReturnTechType
        {
            get => _returnTechType;
            set
            {
                _returnTechType = value;
                if (!_iconSet && value != TechType.None)
                {
                    SetIcon();
                }
            }
        }

        private void SetIcon()
        {
            _slotTab.SetIcon(ReturnTechType,_mono.OnItemButtonClicked);
            _iconSet = true;
        }

        
        public PlantSlot(GrowBedManager mono,int id, Transform slot, Transform slotBounds, SlotItemTab slotButton)
        {
            _mono = mono;
            Id = id;
            Slot = slot;
            SlotBounds = slotBounds.gameObject;
            PlantModel = slot.GetChild(0).gameObject;
            _slotTab = slotButton;

        }


        internal void ShowDisplay()
        {
            _slotTab.SetVisibility(true);
        }


        internal void HideDisplay()
        {
            _slotTab.SetVisibility(false);
        }

        public void Clear()
        {
            Object.Destroy(PlantModel);

            if (Plantable.linkedGrownPlant)
            {
                Object.Destroy(Plantable.linkedGrownPlant.gameObject);
            }
            _slotTab.Clear();
            PlantModel = null;
            Plantable = null;
            _iconSet = false;
        }

        public readonly int Id;

        public readonly Transform Slot;

        public bool IsOccupied;

        public GameObject PlantModel;
        private SlotItemTab _slotTab;
        private TechType _returnTechType;
        private bool _iconSet;
        private GrowBedManager _mono;

        public void ShowPlant()
        {
            var model = GameObject.Instantiate(PlantModel);
            model.transform.SetParent(Slot,false);
            model.transform.localPosition = Vector3.zero;
        }

        public TechType GetPlantSeedTechType()
        {
            if (Plantable != null)
            {
                return Plantable.pickupable.GetTechType();
            }

            return TechType.None;
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
