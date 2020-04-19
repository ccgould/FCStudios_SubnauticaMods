using System;
using System.Collections.Generic;
using System.Linq;
using FCS_HydroponicHarvesters.Enumerators;
using FCS_HydroponicHarvesters.Model;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvGrowBed : MonoBehaviour, IFCSStorage
    {

        private bool _initialized;
        private FCSEnvironment _currentEnvironment;
        private bool _fromLoad;
        private HydroHarvController _mono;
        private readonly Dictionary<TechType, StoredDNAData> _dna = new Dictionary<TechType, StoredDNAData>();
        private readonly List<TechType> _invalidAdjustTechTypes = new List<TechType>
        {
            TechType.CreepvineSeedCluster,
            TechType.Creepvine,
            TechType.CreepvinePiece,
            TechType.BloodOil,
            TechType.BloodGrass,
            TechType.BloodRoot,
            TechType.BloodVine
        };


        public int GetContainerFreeSpace => Slots.Count(x => !x.IsOccupied);
        public bool IsFull => GetContainerFreeSpace <= 0;
        internal PlantSlot[] Slots;


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

        private bool TryPlant(FCSDNA plantable, InventoryItem item)
        {
            bool result = false;
            if (IsCorrectEnvironment(plantable))
            {
                if (GetContainerFreeSpace >= 1)
                {
                    var freeslot = GetFreeSlotID();
                    AddItem(plantable, freeslot, item.item.GetTechType());
                    result = true;
                }
            }

            if (result)
            {
                Destroy(item.item);
            }

            return result;
        }

        private void AddItem(FCSDNA plantable, int slotID, TechType dnaTechType)
        {
            var slotByID = this.GetSlotByID(slotID);
            if (slotByID == null)
            {
                return;
            }
            if (slotByID.IsOccupied)
            {
                return;
            }
            QuickLogger.Debug($"Adding DNa with TechType {plantable.TechType}", true);

            SetSlotOccupiedState(slotID, true);
            slotByID.Plantable = plantable;
            slotByID.PlantModel = Spawn(slotByID.Slot, plantable.TechType, plantable.Model);

            if (_dna.ContainsKey(plantable.GiveItem))
            {
                _dna[plantable.GiveItem].Amount += 1;
            }
            else
            {
                _dna.Add(plantable.GiveItem, new StoredDNAData { Amount = 1, TechType = dnaTechType });
            }

            if (!_fromLoad)
            {
                if (!_mono.HydroHarvContainer.Items.ContainsKey(plantable.GiveItem))
                {
                    _mono.HydroHarvContainer.AddItemToContainer(plantable.GiveItem, true);
                }
            }
            _mono.DisplayManager.UpdateDnaCounter();
        }

        private Vector3 GetModelScale(TechType techType)
        {
            return new Vector3(0.3f, 0.3f, 0.3f);
        }

        private void SetActiveAllChildren(Transform transform, bool value)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(value);

                SetActiveAllChildren(child, value);
            }
        }

        private void SetLocalZeroAllChildren(Transform transform)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.transform.localPosition = Vector3.zero;
                SetLocalZeroAllChildren(child);
            }
        }

        private void SetSlotOccupiedState(int slotID, bool state)
        {
            GetSlotByID(slotID).IsOccupied = state;
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

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;

            if (FindPots())
            {
                _initialized = true;
            }
        }

        public bool CanBeStored(int amount)
        {
            throw new NotImplementedException();
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var dna = item.item.gameObject.GetComponentInChildren<FCSDNA>();
            return TryPlant(dna, item);
        }

        internal HydroHarvSize GetHydroHarvSize()
        {
            switch (Slots.Length)
            {
                case 1:
                    return HydroHarvSize.Small;
                case 2:
                    return HydroHarvSize.Medium;
                case 4:
                    return HydroHarvSize.Large;
            }

            return HydroHarvSize.Unknown;
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

            dna.GetDnaData();

            if (_currentEnvironment != dna.Environment)
            {
                return false;
            }

            return true;
        }
        
        public GameObject Spawn(Transform parent, TechType techType, GameObject model)
        {
            if (!_initialized) return null;

            var gameObject = model != null ? Instantiate(model) : Instantiate(CraftData.GetPrefabForTechType(techType));

            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localScale = GetModelScale(techType);
            gameObject.transform.localPosition = Vector3.zero;

            var growingPlant = gameObject.GetComponent<GrowingPlant>();
            if (growingPlant != null)
            {
                growingPlant.EnableIndoorState();
                growingPlant.enabled = false;
                growingPlant.SetProgress(1f);
                Transform transform = model.transform;
                growingPlant.SetScale(transform, 1f);
                growingPlant.SetPosition(transform);
            }
            else
            {
                Destroy(gameObject.GetComponent<Pickupable>());
                gameObject.AddComponent<TechTag>().type = techType;
                Destroy(gameObject.GetComponent<UniqueIdentifier>());
                gameObject.AddComponent<GrownPlant>().seed = null;
            }

            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<WorldForces>());

            SetActiveAllChildren(parent.transform, true);

            if (!_invalidAdjustTechTypes.Contains(techType))
            {
                SetLocalZeroAllChildren(parent.transform);
            }

            return gameObject;
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
            _mono.DisplayManager.UpdateDnaCounter();
        }
        
        internal void SetBedType(FCSEnvironment environment)
        {
            _currentEnvironment = environment;
        }

        internal FCSEnvironment GetBedType()
        {
            return _currentEnvironment;
        }
        
        internal Dictionary<TechType, StoredDNAData> GetDNASamples()
        {
            return _dna;
        }

        internal bool HasItems()
        {
            return _dna.Count > 0;
        }
        
        internal void RemoveDNA(TechType item)
        {
            if (_dna == null)
            {
                QuickLogger.Error("DNA is Null");
                return;
            }

            if (Slots == null)
            {
                QuickLogger.Error("Slots is Null");
                return;
            }

            if (_dna.ContainsKey(item))
            {

                QuickLogger.Debug($"Checking for: {item} || Amount Stored: {_dna[item].Amount}");

                if (_dna[item].Amount > 1)
                {
                    _dna[item].Amount -= 1;
                }
                else
                {
                    _dna.Remove(item);
                }
            }

            foreach (PlantSlot slot in Slots)
            {
                if (slot?.Plantable?.GiveItem == item)
                {
                    RemoveItem(slot.Id);
                    break;
                }
            }

            QuickLogger.Debug("Remove Model");
        }
        
        internal void Load(Dictionary<TechType, StoredDNAData> savedDataDnaSamples)
        {
            if (savedDataDnaSamples == null) return;
            _fromLoad = true;
            foreach (KeyValuePair<TechType, StoredDNAData> dnaSample in savedDataDnaSamples)
            {
                for (int i = 0; i < dnaSample.Value.Amount; i++)
                {
                    var dna = dnaSample.Value.TechType.ToInventoryItem();
                    var fcsDna = dna.item.gameObject.GetComponentInChildren<FCSDNA>();
                    fcsDna.GetDnaData();
                    AddItemToContainer(dna);
                }
            }

            _fromLoad = false;
        }

        public int GetDnaCount(TechType item)
        {
            try
            {
                return _dna.SingleOrDefault(x => x.Key == item).Value.Amount;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return 0;
        }

        public int GetDNASamplesTotal()
        {
            int amount = 0;
            foreach (KeyValuePair<TechType, StoredDNAData> dnaData in _dna)
            {
                amount += dnaData.Value.Amount;
            }

            return amount;
        }
    }
}
