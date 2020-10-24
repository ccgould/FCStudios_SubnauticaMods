using System;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.HydroponicHarvester.Models;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class GrowBedManager : MonoBehaviour, IFCSGrowBed
    {
        internal PlantSlot[] Slots;
        private HydroponicHarvesterController _mono;
        private bool _initialized;
        private const float MaxPlantsHeight = 3f;

        internal void Initialize(HydroponicHarvesterController mono)
        {
            _mono = mono;

            if (FindPots())
            {
                grownPlantsRoot = GameObject.Find("Planters");
                _initialized = true;
            }
        }

        internal void AddDummy(TechType type, int amount = 1)
        {
            try
            {
                if (amount > Slots.Length)
                {
                    amount = Slots.Length;
                }

                var item = type.ToInventoryItem();

                if (item != null)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        AddItem(item);
                    }
                }
                else
                {
                    QuickLogger.Error($"To inventory time failed with techtype {type}");
                    return;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message,true);

            }

        }


        private bool FindPots()
        {
            try
            {
                var planters = gameObject.transform.Find("model").Find("Planters").transform;
                Slots = new PlantSlot[planters.childCount];

                for (int i = 0; i < planters.childCount; i++)
                {
                    Slots[i] = new PlantSlot(i, planters.GetChild(i));
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private int GetFreeSlotID()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].IsOccupied)
                {
                    return i;
                }
            }

            return 0;
        }

        private PlantSlot GetSlotByID(int slotID)
        {
            return Slots[slotID];
        }

        private void SetSlotOccupiedState(int slotID, bool state)
        {
            GetSlotByID(slotID).IsOccupied = state;
        }

        internal void RemoveItem(int slotID)
        {
            var slotByID = GetSlotByID(slotID);

            if (slotByID == null)
            {
                QuickLogger.Debug("SlotById returned null");
                return;
            }

            GameObject plantModel = slotByID.PlantModel;
            slotByID.Clear();
            Destroy(plantModel);
            SetSlotOccupiedState(slotID, false);
        }

        internal void AddItem(InventoryItem item)
        {
            Plantable component = item.item.GetComponent<Plantable>();
            item.item.SetTechTypeOverride(component.plantTechType, false);
            item.isEnabled = false;
            this.AddItem(component, this.GetFreeSlotID());
        }

        private void AddItem(Plantable plantable, int slotID)
        {
            PlantSlot slotByID = this.GetSlotByID(slotID);
            if (slotByID == null)
            {
                return;
            }

            if (slotByID.IsOccupied)
            {
                return;
            }

            this.SetSlotOccupiedState(slotID, true);
            GameObject gameObject = plantable.Spawn(slotByID.Slot, true);
            this.SetupRenderers(gameObject, true);
            gameObject.SetActive(true);
            slotByID.Plantable = plantable;
            slotByID.PlantModel = gameObject;
            slotByID.SetMaxPlantHeight(MaxPlantsHeight);
            var growingPlant = gameObject.GetComponent<GrowingPlant>();
            if (growingPlant != null)
            {
                var fcsGrowing = gameObject.AddComponent<FCSGrowingPlant>();
                fcsGrowing.Initialize(growingPlant, this);
                Destroy(growingPlant);
            }

            if (plantable.eatable != null)
            {
                plantable.eatable.SetDecomposes(false);
            }

            //slotByID.ShowPlant();
        }

        public void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer, true, -1);
        }

        public GameObject grownPlantsRoot { get; set; }

        public void ClearGrowBed()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                RemoveItem(i);
            }
        }
    }
}
