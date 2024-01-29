using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Enums;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UWE;
using static FCS_ProductionSolutions.Configuration.SaveData;
using PlantSlot = FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model.PlantSlot;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono;

internal class HydroponicHarvesterController : FCSDevice, IFCSSave<SaveData>, IProtoTreeEventListener, IFCSDumpContainer
{
    [SerializeField] private HoverInteraction _hoverInteraction;
    [SerializeField] private Planter __plantable;
    [SerializeField] private PlantSlot[] _plantSlots;
    [SerializeField] private Light[] _lights;
    [SerializeField] private DumpContainerSimplified _harvesterStorageContainer;
    [SerializeField] private StorageContainer _storage;
    [SerializeField] private Transform grownPlantsRoot;

    private HarvesterSpeedModes _currentSpeedMode;
    private bool _isLightsOn;
    private int _currentSlotID;

    public override void Awake()
    {
        base.Awake();
        _hoverInteraction.onSettingsKeyPressed += _hoverInteraction_onSettingsKeyPressed;
        _harvesterStorageContainer.Initialize(transform, "Harvester Dump Container", this, 2, 2);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            _runStartUpOnEnable = false;
        }
    }

    public override void Start()
    {
        base.Start();

        if (_savedData == null)
        {
            ReadySaveData();
        }

        LoadSave();
    }

    internal void LoadSave()
    {
        QuickLogger.Debug("Loading Harvester save");

        if (IsFromSave && _savedData != null)
        {
            QuickLogger.Debug($"Is From Save: {GetPrefabID()}");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            QuickLogger.Debug($"Is Save Data Present: {_savedData is not null}");

            if (_savedData is not null)
            {
                QuickLogger.Debug($"Setting Data");

                var save = _savedData as HarvesterSaveDataEntry;

                
                _plantSlots[0].Load(save.Slot1Data);
                _plantSlots[1].Load(save.Slot2Data);
                _plantSlots[2].Load(save.Slot3Data);
                _plantSlots[3].Load(save.Slot4Data);

                SetLightState(save.IsLightOn);


                SetHarvesterSpeed(save.SpeedMode);
            }
        }
        QuickLogger.Debug("Loaded harvester save");

    }

    private void _hoverInteraction_onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<HarvesterSaveDataEntry>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    #region Testing

    private void Test()
    {
        StartCoroutine(TestCo());
    }

    public IEnumerator Test(PlantSlot slot, TechType techType)
    {
        var itemTask = new TaskResult<InventoryItem>();
        yield return techType.ToInventoryItem(itemTask);
        var item = itemTask.Get();

        if (item != null)
        {
            slot.SetPlant(item);
        }
        else
        {
            QuickLogger.Error($"To inventory item failed to create with techtype {techType}");
        }
        yield break;
    }

    public IEnumerator TestCo()
    {
        yield return Test(_plantSlots[0], TechType.MelonSeed);
        yield return Test(_plantSlots[1], TechType.CreepvineSeedCluster);
        yield return Test(_plantSlots[2], TechType.HangingFruit);
        yield return Test(_plantSlots[3], TechType.BulboTreePiece);

        yield break;
    }

    #endregion

    public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        Plantable component = pickupable.GetComponent<Plantable>();

        if (!component)
        {
            return false;
        }

        return !_plantSlots[_currentSlotID].IsOccupied();
    }

    public bool IsAllowedToAdd(Pickupable pickupable, int containerTotal)
    {
        return IsAllowedToAdd(pickupable, false);
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Info("================================ Saving Harvester ============================");

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new HarvesterSaveDataEntry();
        }

        var save = _savedData as HarvesterSaveDataEntry;

        save.Id = GetPrefabID();
        
        //save.ColorTemplate = _colorManager.SaveTemplate();

        save.Slot1Data = _plantSlots[0].Save();

        save.Slot2Data = _plantSlots[1].Save();

        save.Slot3Data = _plantSlots[2].Save();

        save.Slot4Data = _plantSlots[3].Save();

        save.IsLightOn = _isLightsOn;

        save.SpeedMode = _currentSpeedMode;

        newSaveData.Data.Add(save);

        QuickLogger.Info("================================ Saved Harvester ============================");
    }

    internal void SetHarvesterSpeed(HarvesterSpeedModes speed)
    {
        _currentSpeedMode = speed;
        for (int i = 0; i < _plantSlots.Length; i++)
        {
            _plantSlots[i].SetCurrentSpeedMode(speed);
        }
    }

    internal void SetLightState(bool isLightOn)
    {
        _isLightsOn = isLightOn;
        foreach (Light light in _lights)
        {
            light.enabled = isLightOn;
        }
    }

    internal bool GetLightState()
    {
        return _isLightsOn;
    }

    internal HarvesterSpeedModes GetCurrentSpeedMode()
    {
        return _currentSpeedMode;
    }

    internal PlantSlot GetPlantSlot(int i)
    {
        if (_plantSlots.Length <= 0) return null;
        return _plantSlots[i];
    }

    internal bool HasPowerToConsume()
    {
        GameModeUtils.GetGameMode(out GameModeOption mode, out GameModeOption cheats);
        if (mode == GameModeOption.Creative)
        {
            return true;
        }

       return false;
    }

    internal IEnumerator AddItemToItemsContainer(TechType techType)
    {
        //var itemTask = new TaskResult<InventoryItem>();
        //yield return techType.ToInventoryItem(itemTask);
        //var inventoryItem = itemTask.Get();
        //_harvesterStorageContainer.container.UnsafeAdd(inventoryItem);
        yield break;
    }

    internal void Open(int id)
    {
        _currentSlotID = id;
        CoroutineHost.StartCoroutine(OpenStorage());
    }

    public IEnumerator OpenStorage()
    {
        QuickLogger.Debug($"Storage Button Clicked", true);

        //Close FCSPDA so in game pda can open with storage
        FCSPDAController.Main.Close();

        QuickLogger.Debug($"Closing FCS PDA", true);

        QuickLogger.Debug("Attempting to open the In Game PDA", true);
        Player main = Player.main;
        PDA pda = main.GetPDA();

        while (pda != null && pda.isInUse || pda.isOpen)
        {
            QuickLogger.Debug("Waiting for In Game PDA Settings to reset", true);
            yield return null;
        }

        QuickLogger.Debug("Gettings Reset", true);
        _harvesterStorageContainer.OpenStorage();
        yield break;
    }

    internal void ClearSlot(Plantable id)
    {
        __plantable.RemoveItem(id);
    }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        CoroutineHost.StartCoroutine(this.DeserializeAsync()); new NotImplementedException();
    }

    private IEnumerator DeserializeAsync()
    {
        this.Initialize();
        foreach (object obj in this.grownPlantsRoot.transform)
        {
            Transform transform = (Transform)obj;
            GrownPlant component = transform.GetComponent<GrownPlant>();
            if (component != null)
            {
                component.FindSeed();
            }
            else
            {
                transform.gameObject.SetActive(false);
                Debug.LogErrorFormat("Cannot find GrownPlant component on child {0} of {1} grownPlantRoot", new object[]
                {
                    transform.name,
                    base.transform.name
                });
            }
        }
        yield return null;

        var storageContainer = gameObject.GetComponent<StorageContainer>();
        ItemsContainer container = storageContainer.container;
        if (container != null)
        {
            foreach (InventoryItem inventoryItem in container)
            {
                Plantable component2 = inventoryItem.item.GetComponent<Plantable>();
                if (component2 != null)
                {
                    int slotID = component2.GetSlotID();
                    Planter.PlantSlot slotByID = __plantable.GetSlotByID(slotID);

                    QuickLogger.Debug($"Storage Slot: {slotID}");

                    foreach (PlantSlot item in _plantSlots)
                    {
                        if(item.IsValidSlot(slotID))
                        {
                            item.SetActiveSlot(slotByID);
                        }
                    }
                }
            }
        }
        yield break;
    }

    public bool AddItemToContainer(InventoryItem item)
    {
        try
        {
            _plantSlots[_currentSlotID].SetPlant(item);
            return true;
        }
        catch (Exception e)
        {
            QuickLogger.Error(e.Message);

            QuickLogger.Error(e.StackTrace);
        }
        return false;
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
    {
    }
}