
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
                QuickLogger.Debug("1");
                if (amount > Slots.Length)
                {
                    QuickLogger.Debug("2");
                    amount = Slots.Length;
                    QuickLogger.Debug("3");
                }

                QuickLogger.Debug("4");
                var item = type.ToInventoryItem();
                QuickLogger.Debug("5");
                if (item != null)
                {
                    QuickLogger.Debug("6");
                    for (int i = 0; i < amount; i++)
                    {
                        QuickLogger.Debug("7");
                        AddItem(item);
                        QuickLogger.Debug("8");
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
                QuickLogger.Error($"[GrowBedManager_AddDummy]{e.Message} : {e.StackTrace}",true);

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
                    var planter = planters.GetChild(i);
                    Slots[i] = new PlantSlot(i, planter, planter.Find("PlanterBounds"));
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[Find Pots] {e.Message}");
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
                fcsGrowing.Initialize(growingPlant, this,slotByID.SlotBounds.GetComponent<Collider>().bounds.size);
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
