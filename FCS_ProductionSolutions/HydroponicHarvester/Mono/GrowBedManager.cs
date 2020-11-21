
using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Models;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class GrowBedManager : MonoBehaviour, IFCSGrowBed, IFCSStorage
    {
        private HydroponicHarvesterController _mono;
        private bool _initialized;
        private const float MaxPlantsHeight = 3f;
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private List<TechType> _activeClones;
        private Dictionary<TechType, int> _storage;
        public bool NotAllowToGenerate => PauseUpdates || !_mono.IsConstructed || CurrentSpeedMode == SpeedModes.Off;
        internal SpeedModes CurrentSpeedMode
        {
            get => _currentMode;
            set
            {
                SpeedModes previousMode = _currentMode;
                _currentMode = value;

                if (_currentMode != SpeedModes.Off)
                {
                    //PowerManager.UpdateEnergyPerSecond(_currentMode);
                    if (previousMode == SpeedModes.Off)
                        TryStartingNextClone();
                }
                else // Off State
                {
                    //PowerManager.UpdateEnergyPerSecond(_currentMode);
                }
            }
        }

        public bool PauseUpdates { get; set; }

        internal PlantSlot[] Slots;
        private TechType _currentItemTech;
        private SpeedModes _currentMode;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        internal const float CooldownComplete = 19f;
        internal const float StartUpComplete = 4f;
        
        internal void Initialize(HydroponicHarvesterController mono)
        {
            _mono = mono;

            if (FindPots())
            {
                _activeClones = new List<TechType>();
                _storage = new Dictionary<TechType, int>();
                grownPlantsRoot = GameObjectHelpers.FindGameObject(gameObject, "Planters");
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
                       var result =  AddItemToContainer(item);
                       if (result) continue;
                       QuickLogger.Error($"Failed to add item to container");
                        Destroy(item.item.gameObject);
                    }
                }
                else
                {
                    QuickLogger.Error($"To inventory item failed to create with techtype {type}");
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[GrowBedManager_AddDummy]{e.Message} : {e.StackTrace}",true);

            }
        }
        
        internal void ShowDisplay()
        {
            foreach (PlantSlot slot in Slots)
            {
                slot.ShowDisplay();
            }
        }

        internal void HideDisplay()
        {
            foreach (PlantSlot slot in Slots)
            {
                slot.HideDisplay();
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
                    Slots[i] = new PlantSlot(this,i, planter, planter.Find("PlanterBounds"),
                        GameObjectHelpers.FindGameObject(gameObject, $"Slot{i+1}Item").AddComponent<SlotItemTab>());
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
            _mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, false);
            slotByID.Clear();
            SetSlotOccupiedState(slotID, false);
        }

        private static EffectType FindEffectType(PlantSlot slotByID)
        {
            QuickLogger.Debug($"PlantSlot: {slotByID.Plantable.underwater}",true);
            return slotByID.Plantable.underwater ? EffectType.Bubbles : EffectType.Smoke;
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
         
            GameObject gameObject = plantable.Spawn(slotByID.Slot, _mono.IsInBase());

            this.SetupRenderers(gameObject, _mono.IsInBase());
            gameObject.SetActive(true);
            slotByID.Plantable = plantable;
            slotByID.PlantModel = gameObject;
            slotByID.SetMaxPlantHeight(MaxPlantsHeight);
            
            _mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, true);
            var growingPlant = gameObject.GetComponent<GrowingPlant>();

            if (growingPlant != null)
            {
                var fcsGrowing = gameObject.AddComponent<FCSGrowingPlant>();
                fcsGrowing.Initialize(growingPlant, this,slotByID.SlotBounds.GetComponent<Collider>().bounds.size, AddActiveClone);
                
                var pickPrefab = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();

                if (pickPrefab != null)
                {
                    slotByID.ReturnTechType = pickPrefab.pickTech;
                }
                else
                {
                    slotByID.ReturnTechType = _currentItemTech;
                }

                Destroy(growingPlant);
            }

            if (plantable.eatable != null)
            {
                plantable.eatable.SetDecomposes(false);
            }

        }

        internal void AddActiveClone(TechType techType)
        {
            _activeClones.Add(techType);
        }

        public void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer);
        }

        public GameObject grownPlantsRoot { get; set; }
        public bool IsInBase()
        {
            return _mono.IsInBase();
        }

        public void ClearGrowBed()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                RemoveItem(i);
            }
        }

        public bool HasItems()
        {
            return Slots?.Any(plantSlot => plantSlot != null && plantSlot.IsOccupied) ?? false;
        }

        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            if (IsFull) return false;
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _currentItemTech = item.item.GetTechType();
            Plantable component = item.item.GetComponent<Plantable>();
            item.item.SetTechTypeOverride(component.plantTechType);
            item.isEnabled = false;
            AddItem(component, GetFreeSlotID());
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var plantable = pickupable.gameObject.GetComponentInChildren<Plantable>();
            if (plantable  != null && !plantable.isSeedling) return false;
            return CanBeStored(1);
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            if (!_storage.ContainsKey(techType)) return null;

            _storage[techType] -= 1;
            TryStartingNextClone();

            if (_storage[techType] == 0)
            {
                _storage.Remove(techType);
            }
            return techType.ToPickupable();
        }

        internal void RemoveSample(TechType techType)
        {
            _activeClones.Remove(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return _storage;
        }

        public bool ContainsItem(TechType techType)
        {
            return _storage.ContainsKey(techType);
        }
        
        internal float StartUpProgress
        {
            get => _progress[(int)ClonePhases.StartUp];
            set => _progress[(int)ClonePhases.StartUp] = value;
        }
        
        internal float GenerationProgress
        {
            get => _progress[(int)ClonePhases.Generating];
            set => _progress[(int)ClonePhases.Generating] = value;
        }

        internal float CoolDownProgress
        {
            get => _progress[(int)ClonePhases.CoolDown];
            set => _progress[(int)ClonePhases.CoolDown] = value;
        }
        
        private void Update()
        {
            //QuickLogger.Debug($"PauseUpdates: {PauseUpdates} || IsConstructed: {_mono.IsConstructed} || CurrentSpeedMode: {_mono.CurrentSpeedMode}",true);

            if (NotAllowToGenerate)
                return;

           var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

            //TODO Deal with Power
            if (!_mono.HasPowerToConsume())
                return;

            if (CoolDownProgress >= CooldownComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Finished Cooldown", true);

                PauseUpdates = true;
                SpawnClone();
                // Finished cool down - See if the next clone can be started                
                TryStartingNextClone(); //TODO check here

                this.PauseUpdates = false;
            }
            else if (this.CoolDownProgress >= 0f)
            {
                // Is currently cooling down
                this.CoolDownProgress = Mathf.Min(CooldownComplete, this.CoolDownProgress + DayNightCycle.main.deltaTime);
            }
            else if (this.GenerationProgress >= QPatch.Configuration.EnergyConsumpion)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Cooldown", true);

                // Finished generating clone - Start cool down
                this.CoolDownProgress = 0f;
            }
            else if (this.GenerationProgress >= 0f)
            {
                _mono.ConsumePower();

                // Is currently generating clone
                this.GenerationProgress = Mathf.Min(QPatch.Configuration.EnergyConsumpion, this.GenerationProgress + energyToConsume);
            }
            else if (this.StartUpProgress >= StartUpComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
                // Finished start up - Start generating clone
                this.GenerationProgress = 0f;
            }
            else if (this.StartUpProgress >= 0f)
            {
                // Is currently in start up routine
                this.StartUpProgress = Mathf.Min(StartUpComplete, this.StartUpProgress + DayNightCycle.main.deltaTime);
            }
        }

        private float CalculateEnergyPerSecond()
        {
            if (CurrentSpeedMode == SpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentSpeedMode);
            return QPatch.Configuration.EnergyConsumpion / creationTime;
        }

        internal void TryStartingNextClone()
        {
            QuickLogger.Debug("Trying to start another clone", true);

            if (CurrentSpeedMode == SpeedModes.Off)
                return;// Powered off, can't start a new clone

            if (this.StartUpProgress < 0f || // Has not started a clone yet
                this.CoolDownProgress == CooldownComplete) // Has finished a clone
            {
                if (!IsFull)
                {
                    QuickLogger.Debug("[Hydroponic Harvester] Start up", true);
                    this.CoolDownProgress = -1f;
                    this.GenerationProgress = -1f;
                    this.StartUpProgress = 0f;
                }
                else
                {
                    QuickLogger.Debug("Cannot start another clone, container is full", true);
                }
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone, another clone is currently in progress", true);
            }
        }

        internal void SpawnClone()
        {
            foreach (TechType sample in _activeClones)
            {
                if (IsFull) break;
                AddToStorage(sample);
            }
        }

        private void AddToStorage(TechType sample)
        {
            if (_storage.ContainsKey(sample))
            {
                _storage[sample] += 1;
            }
            else
            {
                _storage.Add(sample,1);
            }
        }

        public void SetSpeedMode(SpeedModes result)
        {
            QuickLogger.Debug($"Setting SpeedMode to {result}",true);
            CurrentSpeedMode = result;
        }

        public string GetSlotInfo(int slotIndex)
        {
            if (Slots[slotIndex].Plantable != null)
            {
                var nameOfPlant = Language.main.Get(Slots[slotIndex].Plantable.plantTechType);
                if (Slots[slotIndex].GetPlantSeedTechType() != TechType.None)
                {
                    int amount = 0;

                    if (_storage.ContainsKey(Slots[slotIndex].ReturnTechType))
                    {
                        amount = _storage[Slots[slotIndex].ReturnTechType];
                    }
                    
                    return $"{nameOfPlant} x{amount}";
                }                
            }

            return "N/A";
        }

        public void OnItemButtonClicked(string arg1, object arg2)
        {
            var item = RemoveItemFromContainer((TechType) arg2, 1);
            if (item != null)
            {
                PlayerInteractionHelper.GivePlayerItem(item);
            }
        }
    }
}
