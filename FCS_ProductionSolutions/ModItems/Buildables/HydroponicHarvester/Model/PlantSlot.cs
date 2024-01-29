using FCS_AlterraHub.Core.Helpers;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Enums;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model;
internal class PlantSlot : MonoBehaviour
{
    [SerializeField] private HydroponicHarvesterController controller;
    [SerializeField] private ParticleSystem[] waterFX;
    [SerializeField] private ParticleSystem smokeFX;
    [SerializeField] private float energyConsumption = 15000f;
    [SerializeField] private int maxCapacity = 50;
    [SerializeField] private int slotId;
    [SerializeField] private StorageContainer _storageContainer;
    [SerializeField] private Planter planter;
    [SerializeField] private int[] validBigSlots;
    [SerializeField] private int[] validSmallSlots;


    internal float GenerationProgress
    {
        get => _progress[(int)ClonePhases.Generating];
        set => _progress[(int)ClonePhases.Generating] = value;
    }
    public Action OnStatusUpDate;
    private Planter.PlantSlot _activePlantSlot;
    private bool _effectsIsPlaying;
    private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
    private bool isOccupied => _activePlantSlot?.isOccupied ?? false;
    private HarvesterSpeedModes _currentHarvesterMode;
    private bool pauseUpdates;
    private bool IsFull => _count >= maxCapacity;
    private int _count;


    private void Start()
    {
        InvokeRepeating(nameof(CheckFx), 1, 1);
    }

    private void CheckFx()
    {
        if (GetPlantable() is null)
        {
            if(_effectsIsPlaying)
            {
                if (waterFX is not null)
                {
                    foreach (ParticleSystem waterFX in waterFX)
                    {
                        waterFX.Stop();
                    }
                }

                if (smokeFX is not null)
                {
                    smokeFX.Stop();
                }

                _effectsIsPlaying = false;
            }

            return;
        }

        if (!_effectsIsPlaying)
        {
            if (waterFX is not null && GetPlantable().underwater)
            {
                foreach (ParticleSystem waterFX in waterFX)
                {
                    waterFX.Play();
                    _effectsIsPlaying = true;
                }
            }

            else if (smokeFX is not null && !_effectsIsPlaying && GetPlantable().aboveWater)
            {
                smokeFX.Play();
                _effectsIsPlaying = true;
            }
        } 
    }

    public bool NotAllowToGenerate()
    {
        return pauseUpdates 
            || _currentHarvesterMode == HarvesterSpeedModes.Off 
            || _activePlantSlot is null 
            || _activePlantSlot.plantable is null 
            || _activePlantSlot.plantable.linkedGrownPlant is null;
    }

    internal void SetCurrentSpeedMode(HarvesterSpeedModes speed)
    {
        HarvesterSpeedModes previousMode = _currentHarvesterMode;

        _currentHarvesterMode = speed;

        if (_currentHarvesterMode != HarvesterSpeedModes.Off)
        {
            if (previousMode == HarvesterSpeedModes.Off)
                TryStartingNextClone();
        }
    }

    internal HarvesterSpeedModes GetCurrentHarvesterSpeedMode()
    {
        return _currentHarvesterMode;
    }

    private void Update()
    {
        if (controller is null || planter is null || validBigSlots is null || validBigSlots == null) return;
        GenerateClone();
    }

    private void GenerateClone()
    {
        if (NotAllowToGenerate())
            return;

        //QuickLogger.Debug($"1");


        var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

        //QuickLogger.Debug($"2");


        if (!controller.HasPowerToConsume())
            return;

        //QuickLogger.Debug($"3");


        if (GenerationProgress >= energyConsumption)
        {
            //QuickLogger.Debug("[Hydroponic Harvester] Generated Clone", true);
            pauseUpdates = true;
            GenerationProgress = -1f;
            SpawnClone();
            TryStartingNextClone();
            pauseUpdates = false;
        }
        else if (GenerationProgress >= 0f)
        {
            //QuickLogger.Debug($"Generating");

            // Is currently generating clone
            GenerationProgress = Mathf.Min(energyConsumption, GenerationProgress + energyToConsume);
        }

        //QuickLogger.Debug($"=================== PlantSlot {Id}: Update =====================");
    }

    internal void SetActiveSlot(Planter.PlantSlot plantSlot)
    {
       // SetReturnTypes(plantSlot.plantable.plantTechType);
        _activePlantSlot = plantSlot;
        OnStatusUpDate?.Invoke();
    }

    public Plantable GetPlantable()
    {
        return _activePlantSlot?.plantable;
    }

    private float CalculateEnergyPerSecond()
    {
        if (_currentHarvesterMode == HarvesterSpeedModes.Off) return 0f;
        var creationTime = Convert.ToSingle(_currentHarvesterMode);
        return energyConsumption / creationTime;
    }

    private void Clear()
    {
        _count = 0;
    }

    internal void SetPlant(InventoryItem plant)
    {
        Plantable component = plant.item.GetComponent<Plantable>();
        plant.item.SetTechTypeOverride(component.plantTechType, false);
        plant.isEnabled = false;

        var avaliableSlot = -1;

        if (component.size == Plantable.PlantSize.Small)
        {
            foreach (var slot in validSmallSlots)
            {
                var curSlot = planter.smallPlantSlots.FirstOrDefault(x => x.id.Equals(slot));

                if (!curSlot.isOccupied)
                {
                    SetActiveSlot(curSlot);
                    avaliableSlot = slot;
                    break;
                }
            }
        }
        else
        {
            foreach (var slot in validBigSlots)
            {
                if (!planter.bigPlantSlots[slot].isOccupied)
                {
                    SetActiveSlot(planter.bigPlantSlots[slot]);
                    avaliableSlot = slot;
                    break;
                }
            }
        }

        planter.AddItem(component, avaliableSlot);
        _storageContainer.container.UnsafeAdd(plant);
    }

    internal bool TryClear(bool forceClear = false, bool clearContainer = false)
    {
        if (!forceClear && _count > 0)
        {
            //GrowBedManager.HarvesterController.DisplayManager.ShowMessage(AuxPatchers.PleaseClearHarvesterSlot());
            return false;
        }

        Clear();

        return true;
    }
    
    public bool RemoveItem()
    {
        _count--;
        TryStartingNextClone();
        return true;
    }

    public bool CanRemoveItem() => _count > 0;

    public void AddItem()
    {
        if (IsFull) return;
        _count++;
        StartCoroutine(controller.AddItemToItemsContainer(GetSeedReturnType()));
        OnStatusUpDate?.Invoke();
    }

    internal bool IsPlantFullyGrown()
    {
        if (_activePlantSlot?.plantable?.linkedGrownPlant is null) return false;
        return true;
    }

    public TechType GetReturnType()
    {

        //Check to see if plant is grown
        if (IsPlantFullyGrown())
        {
            var linkedPlant = _activePlantSlot.plantable.linkedGrownPlant;

            //Check to see if its a pickplant
            var pickPrefab = linkedPlant.GetComponent<PickPrefab>();
            if(pickPrefab is not null)
            {
                return pickPrefab.pickTech;
            }

            //Check to see if fruitplant tree
            var fruitPlant = linkedPlant.GetComponent<FruitPlant>();
            if(fruitPlant is not null)
            {
                return fruitPlant.fruits[0].pickTech;
            }

            //if none
            return GetSeedReturnType();
        }
        else
        {
            return TechType.None;
        }
    }

    public TechType GetSeedReturnType()
    {  
        if(_activePlantSlot is not null)
        {
            return CraftData.GetHarvestOutputData(_activePlantSlot.plantable.plantTechType);
        }

        return TechType.None;
    }

    internal void SpawnClone()
    {
        AddItem();
    }

    private void TryStartingNextClone()
    {
        QuickLogger.Debug("Trying to start another clone", true);

        if (_currentHarvesterMode == HarvesterSpeedModes.Off)
            return;// Powered off, can't start a new clone

        if (IsFull)
        {
            QuickLogger.Debug("Cannot start another clone, container is full", true);
            return;
        }

        if (Mathf.Approximately(GenerationProgress, -1f))
        {
            QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
            GenerationProgress = 0f;
        }
        else
        {
            QuickLogger.Debug("Cannot start another clone", true);
        }
    }

    public int GetCount()
    {
        return _count;
    }

    public int GetMaxCapacity()
    {
        return maxCapacity;
    }

    public void SetCount(int count)
    {
        _count = count;
    }

    internal bool HasItems()
    {
        return GetCount() > 0;
    }

    internal bool IsOccupied()
    {
        return isOccupied;
    }

    internal bool GivePlayerItem()
    {
        if (PlayerInteractionHelper.GivePlayerItem(GetReturnType()))
        {
            _count -= 1;
            return true;
        }

        return false;
    }

    internal bool IsValidSlot(int slotByID)
    {
        var bigSlotsResult = Array.IndexOf(validBigSlots,slotByID);
        var smallSlotsResult = Array.IndexOf(validSmallSlots,slotByID);

        return bigSlotsResult > -1 || smallSlotsResult > -1;
    }

    internal SlotData Save()
    {
       return new SlotData
        {
            Amount = _count,
            GenerationProgress = GenerationProgress,
        };
    }

    internal void Load(SlotData slot1Data)
    {
        _count = slot1Data.Amount;
        GenerationProgress = slot1Data.GenerationProgress;
    }
}

public class SlotData
{
    public int Amount { get; set; }
    public float GenerationProgress { get; set; }
}