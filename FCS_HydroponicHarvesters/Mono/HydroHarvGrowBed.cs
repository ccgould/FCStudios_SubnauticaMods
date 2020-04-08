using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FCS_HydroponicHarvesters.Model;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvGrowBed : MonoBehaviour, IFCSStorage
    {
        public PlantSlot[] Slots;
        private bool _initialized;
        private FCSEnvironment _currentEnvironment;
        private Vector3 _modelScale = new Vector3(0.3f,0.3f,0.3f);
        private HydroHarvController _mono;
        private List<TechType>  _dna = new List<TechType>();

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;

            if (FindPots())
            {
                _initialized = true;
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

        public int GetContainerFreeSpace => Slots.Count(x => !x.isOccupied);
        public bool IsFull => GetContainerFreeSpace <= 0;

        public bool CanBeStored(int amount)
        {
            throw new NotImplementedException();
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var dna = item.item.gameObject.GetComponentInChildren<FCSDNA>();
            dna.GetData();
            return TryPlant(dna,item);
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var dna = pickupable.GetComponent<FCSDNA>();

            if (dna == null)
            {
                return false;
            }

            if (IsFull)
            {
                return false;
            }
            
            return true;
        }

        private bool TryPlant(FCSDNA plantable, InventoryItem item)
        {
            bool result = false;
            if (IsCorrectEnvironment(plantable))
            {
                if (GetContainerFreeSpace >= 1)
                {
                    var freeslot = GetFreeSlotID();
                    AddItem(plantable, freeslot);
                    result =  true;
                }
            }

            if (result)
            {
                Destroy(item.item);
            }
            
            return result;
        }

        private void AddItem(FCSDNA plantable, int slotID)
        {
            var slotByID = this.GetSlotByID(slotID);
            if (slotByID == null)
            {
                return;
            }
            if (slotByID.isOccupied)
            {
                return;
            }

            slotByID.isOccupied = true;
            slotByID.plantable = plantable;
            Spawn(slotByID.slot, plantable.TechType, plantable.Model);
            _dna.Add(plantable.TechType);
            _mono.HydroHarvContainer.AddItemToContainer(plantable.TechType, true);
        }

        public GameObject Spawn(Transform parent, TechType techType,GameObject model)
        {
            var gameObject = model != null ? Instantiate(model) : Instantiate(CraftData.GetPrefabForTechType(techType));

            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localScale = _modelScale;
            gameObject.transform.localPosition = Vector3.zero;


            var growingPlant = gameObject.GetComponent<GrowingPlant>();
            if (growingPlant != null)
            {
                Destroy(gameObject.GetComponent<GrowingPlant>());
            }

            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<WorldForces>());
            Destroy(gameObject.GetComponent<Pickupable>());
            Destroy(gameObject.GetComponent<UniqueIdentifier>());

            foreach (Transform render in gameObject.transform)
            {
                QuickLogger.Debug(render.gameObject.name);
                render.gameObject.SetActive(true);
            }



            return gameObject;
        }
        
        private int GetFreeSlotID()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].isOccupied)
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

        private bool IsCorrectEnvironment(FCSDNA plantable)
        {
            if (_currentEnvironment == FCSEnvironment.Water && plantable.Environment == FCSEnvironment.Water)
            {
                return true;
            }

            if (_currentEnvironment == FCSEnvironment.Air && plantable.Environment == FCSEnvironment.Air)
            {
                return true;
            }

            return false;
        }

        internal void SetBedType(FCSEnvironment environment)
        {
            _currentEnvironment = environment;
        }
        internal FCSEnvironment GetBedType()
        {
            return _currentEnvironment;
        }

        public List<TechType> GetDNASamples()
        {
            return _dna;
        }

        public bool HasItems()
        {
            return _dna.Count > 0;
        }

        public void RemoveDNA(TechType item)
        {
            _dna.Remove(item);
            QuickLogger.Debug("Remove Model");
        }
    }
}
