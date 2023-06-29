using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine;
using FCSCommon.Utilities;
using Nautilus.Utility;
using System;
using System.Collections;
using UnityEngine;
using UWE;
using static FCS_ProductionSolutions.Configuration.SaveData;
using static FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.CubeGeneratorStateManager;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;

internal class IonCubeGeneratorController : FCSDevice, IFCSSave<SaveData>, IWorkUnit
{
    private const float CUBE_ENERGY_COST = 1500f;

    private FCSStorage _cubeContainer;
    private HoverInteraction _interaction;
    private IonCubeGenSpeedModes _currentSpeedMode;
    private CubeGeneratorStateManager cubeGeneratorStateManager;
    private float _energyToConsume;
    
    private float availablePower => powerRelay.GetPower();

    public GameObject CubePrefab { get; private set; }

    public override void Awake()
    {
        QuickLogger.Debug("Awake");
        base.Awake();

        _cubeContainer = GetComponent<FCSStorage>();
        cubeGeneratorStateManager = GetComponent<CubeGeneratorStateManager>();
        _interaction = gameObject.GetComponent<HoverInteraction>();
    }

    public override void Start()
    {
        QuickLogger.Debug($"Start: {GetPrefabID()}");
        base.Start();

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
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

                    var savedData = _savedData as CubeGeneratorSaveData;

                    _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);

                    //NumberOfCubes = NumberOfCubes();
                    cubeGeneratorStateManager.SwitchState(savedData.State, savedData.Progress);
                    SetCurrentSpeedMode(savedData.CurrentSpeedMode);
                }
            }
            _runStartUpOnEnable = false;
        }
        CoroutineHost.StartCoroutine(this.GetCubePrefab());
    }

    private void Update()
    {

        ChangeState();
        _energyToConsume = energyPerSecond * DayNightCycle.main.deltaTime;
    }

    private void ChangeState()
    {

        if (!IsConnectedToBase)
        {
            SetState(FCSDeviceState.NotConnected);
            return;
        }

        if (CurrentSpeedMode() == IonCubeGenSpeedModes.Off)
        {
            SetState(FCSDeviceState.Off);
            return;
        }

        if (cubeGeneratorStateManager.IsIdle())
        {
            SetState(FCSDeviceState.Idle);
            return;
        }

        SetState(FCSDeviceState.Running);

    }

    public override void Initialize()
    {
        _interaction.onPDAClosed += onPDAClosed;
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        base.Initialize();
    }

    private void onPDAClosed(FCSPDAController pda)
    {
        QuickLogger.Debug("IonCubeGen PDA Closed", true);
        CachedHabitatManager?.OnDeviceUIClosed(this);

        //_cubeContainer.AttemptToOpenStorage(pda);
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;

        if (_cubeContainer is not null)
        {
            reason = LanguageService.NotEmpty();
            return _cubeContainer.IsEmpty();
        }
        return true;
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<CubeGeneratorSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving Cube Gen", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new CubeGeneratorSaveData();
        }

        var save = _savedData as CubeGeneratorSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();

        save.NumberOfCubes = NumberOfCubes();
        save.Progress = cubeGeneratorStateManager.GetCurrentProgress();
        save.State = cubeGeneratorStateManager.GetCurrentStateIndex();
        save.CurrentSpeedMode = CurrentSpeedMode();

        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves Cube Gen {newSaveData.Data.Count}", true);
    }

    #region IWorkUnit
    public void SyncDevice(object device)
    {
        if (device is IonCubeGeneratorController)
        {
            IonCubeGeneratorController masterDevice = device as IonCubeGeneratorController;
            if (masterDevice == this) return;
            SetCurrentSpeedMode(masterDevice.CurrentSpeedMode());
            IsVisibleInPDA = masterDevice.IsVisibleInPDA;
        }
    }

    #endregion

    public override float GetPowerUsage()
    {
        if (!GetStateManager().IsCrafting() || !GameModeUtils.RequiresPower()) return 0;
        return energyPerSecond;
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {GetPowerUsage() * 60:F2}] [Running: {CurrentSpeedMode()}] [Percentage: {GetStateManager().GetState(CubeGeneratorStates.Generating).GetProgressNormalized():P0}]",
        };
    }

    internal bool IsNotAllowedToGenerate()
    {
        return !IsConstructed || CurrentSpeedMode() == IonCubeGenSpeedModes.Off || IsFull();
    }

    internal bool HasPowerToConsume()
    {
        return !GameModeUtils.RequiresPower() || (availablePower >= _energyToConsume);
    }

    internal float GetEnergyToConsume()
    {
        return _energyToConsume;
    }

    internal bool GetRequiresEnergy()
    {
        return GameModeUtils.RequiresPower();
    }

    internal void SpawnCube()
    {
        Debug.Log("Spawnwed Ion Cube");

        var gameObject = GameObject.Instantiate(CubePrefab);
        CubePrefab.SetActive(false);
        gameObject.SetActive(true);

        Pickupable pickupable = gameObject.GetComponent<Pickupable>();
        pickupable.Pickup(false);
        var item = new InventoryItem(pickupable);

        _cubeContainer.container.UnsafeAdd(item);
    }

    internal bool IsFull()
    {
        return _cubeContainer.container.IsFull();
    }

    internal IonCubeGenSpeedModes CurrentSpeedMode()
    {
        return _currentSpeedMode;
    }

    internal void SetCurrentSpeedMode(IonCubeGenSpeedModes speed)
    {
        _currentSpeedMode = speed;

        if (_currentSpeedMode != IonCubeGenSpeedModes.Off)
        {
            var CubeCreationTime = Convert.ToSingle(_currentSpeedMode);
            energyPerSecond = CUBE_ENERGY_COST / CubeCreationTime;
        }
        else // Off State
        {
            energyPerSecond = 0f;
        }
    }

    internal CubeGeneratorStateManager GetStateManager()
    {
        return cubeGeneratorStateManager;
    }

    internal int NumberOfCubes()
    {
        return _cubeContainer.container.GetCount(TechType.PrecursorIonCrystal);
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, onPAClose);
    }

    private void onPAClose(FCSPDAController pda)
    {
        QuickLogger.Debug("IonCubeGen PDA Closed", true);
        CachedHabitatManager?.OnDeviceUIClosed(this);
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

        QuickLogger.Debug("Gettings Reset",true);
        _cubeContainer.Open(transform);

        yield break;
    }

    private IEnumerator GetCubePrefab()
    {
        CoroutineTask<GameObject> result = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorIonCrystal, false);
        yield return result;
        CubePrefab = result.GetResult();
        yield break;
    }
}