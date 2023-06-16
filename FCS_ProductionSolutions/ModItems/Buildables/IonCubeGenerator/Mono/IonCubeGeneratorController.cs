using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static FCS_ProductionSolutions.Configuration.SaveData;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;

internal class IonCubeGeneratorController : FCSDevice, IFCSSave<SaveData>, IWorkUnit
{
    private const float CubeEnergyCost = 1500f;
    private IonCubeGenSpeedModes _currentMode = StartingMode;
    private PowerRelay _connectedRelay = null;
    private float CubeCreationTime = CubeEnergyCost;
    private float EnergyConsumptionPerSecond = 0f;
    private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
    private PowerRelay ConnectedRelay
    {
        get
        {
            while (_connectedRelay == null)
                UpdatePowerRelay();

            return _connectedRelay;
        }
    }
    private ICubeContainer _cubeContainer;
    private CubeGeneratorSaveData _saveData;
    private float _lastConsumedEnergy;
    internal const float ProgressComplete = 100f;
    internal const IonCubeGenSpeedModes StartingMode = IonCubeGenSpeedModes.Off;
    internal const float StartUpComplete = 4f;
    internal const float CooldownComplete = 19f;
    public bool IsFull => _cubeContainer.IsFull;

    private float AvailablePower => ConnectedRelay.GetPower();

    public bool PauseUpdates { get; set; } = false;

    public IonCubeGenSpeedModes CurrentSpeedMode
    {
        get => _currentMode;
        set
        {
            IonCubeGenSpeedModes previousMode = _currentMode;
            _currentMode = value;
            if (_currentMode != IonCubeGenSpeedModes.Off)
            {
                CubeCreationTime = Convert.ToSingle(_currentMode);
                EnergyConsumptionPerSecond = CubeEnergyCost / CubeCreationTime;

                if (previousMode == IonCubeGenSpeedModes.Off)
                    TryStartingNextCube();
            }
            else // Off State
            {
                EnergyConsumptionPerSecond = 0f;
            }
        }
    }

    public float StartUpProgress
    {
        get => _progress[(int)CubePhases.StartUp];
        set => _progress[(int)CubePhases.StartUp] = value;
    }

    public float GenerationProgress
    {
        get => _progress[(int)CubePhases.Generating];
        set => _progress[(int)CubePhases.Generating] = value;
    }

    public float CoolDownProgress
    {
        get => _progress[(int)CubePhases.CoolDown];
        set => _progress[(int)CubePhases.CoolDown] = value;
    }

    public float StartUpPercent => Mathf.Max(0f, StartUpProgress / StartUpComplete);

    public float GenerationPercent => Mathf.Max(0f, GenerationProgress / CubeEnergyCost);

    public float CoolDownPercent => Mathf.Max(0f, CoolDownProgress / CooldownComplete);

    public int NumberOfCubes
    {
        get => _cubeContainer.NumberOfCubes;
        set => _cubeContainer.NumberOfCubes = value;
    }

    public bool NotAllowToGenerate => PauseUpdates || !IsConstructed || CurrentSpeedMode == IonCubeGenSpeedModes.Off;

    #region Unity methods

    public override void Awake()
    {
        QuickLogger.Debug("Awake");
        base.Awake();

        if (_cubeContainer == null)
        {
            _cubeContainer = new CubeGeneratorContainer(this);
        }

        var interaction = gameObject.GetComponent<HoverInteraction>();
        interaction.onSettingsKeyPressed += onSettingsKeyPressed;

        UpdatePowerRelay();

        IsInitialized = true;
    }

    public  CubeGeneratorSaveData ReadySaveData1()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        var data = ModSaveManager.GetSaveData<CubeGeneratorSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData: {data.StartUpProgress}");
        return data;
    }

    public override void ReadySaveData()
    {
    }

    public override void Start()
    {
        QuickLogger.Debug($"Start: {GetPrefabID()}");
        base.Start();
        UpdatePowerRelay();

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
                    var g = ReadySaveData1();


                    QuickLogger.Debug($"Is Save Data Present: {g is not null}");
                     if (g is not null)
                    {
                    QuickLogger.Debug($"Setting Data");

                    //_colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);

                    NumberOfCubes = g.NumberOfCubes;
                    StartUpProgress = g.StartUpProgress;
                    GenerationProgress = g.GenerationProgress;
                    CoolDownProgress = g.CoolDownProgress;
                    CurrentSpeedMode = g.CurrentSpeedMode;
                    QuickLogger.Debug($"StartUp Progress Mode: {g.StartUpProgress} || {StartUpProgress}", false);
                    QuickLogger.Debug($"Current Speed Mode: {g.CurrentSpeedMode} || {CurrentSpeedMode}", false);
                    QuickLogger.Debug($"In OnProtoDeserialize Save Loaded: {GetPrefabID()}", false);

                }
                
                }
            }

            _runStartUpOnEnable = false;
        }
    }

    private void Update()
    {

        ChangeState();

        if (NotAllowToGenerate)
            return;
                 
        float energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
        bool requiresEnergy = GameModeUtils.RequiresPower();
        bool hasPowerToConsume = !requiresEnergy || (AvailablePower >= energyToConsume);

        if (!hasPowerToConsume)
            return;

        if (CoolDownProgress >= CooldownComplete)
        {
            // Logger.Log(Logger.Level.Debug, "IonCube Generator - Finished Cooldown", showOnScreen: true);

            PauseUpdates = true;
            _cubeContainer.NumberOfCubes++;
            // Finished cool down - See if the next cube can be started                
            TryStartingNextCube();

            PauseUpdates = false;
        }
        else if (CoolDownProgress >= 0f)
        {
            // Is currently cooling down
            CoolDownProgress = Mathf.Min(CooldownComplete, CoolDownProgress + DayNightCycle.main.deltaTime);
        }
        else if (GenerationProgress >= CubeEnergyCost)
        {
            // Logger.Log(Logger.Level.Debug, "IonCube Generator - Cooldown", showOnScreen: true);

            // Finished generating cube - Start cool down
            CoolDownProgress = 0f;
        }
        else if (GenerationProgress >= 0f)
        {
            if (requiresEnergy)
            {
                ConnectedRelay.ConsumeEnergy(energyToConsume, out float amountConsumed);
            }

            // Is currently generating cube
            GenerationProgress = Mathf.Min(CubeEnergyCost, GenerationProgress + energyToConsume);
        }
        else if (StartUpProgress >= StartUpComplete)
        {
            // Logger.Log(Logger.Level.Debug, "IonCube Generator - Generating", showOnScreen: true);
            // Finished start up - Start generating cube
            GenerationProgress = 0f;
        }
        else if (StartUpProgress >= 0f)
        {
            // Is currently in start up routine
            StartUpProgress = Mathf.Min(StartUpComplete, StartUpProgress + DayNightCycle.main.deltaTime);
        }
    }

    private void ChangeState()
    {

        if(!IsConnectedToBase)
        {
            SetState(FCSDeviceState.NotConnected);
            return;
        }

        if (CurrentSpeedMode == IonCubeGenSpeedModes.Off)
        {
            SetState(FCSDeviceState.Off);
            return;
        }

        if(NotAllowToGenerate  || CoolDownProgress >= 0f)
        {
            SetState(FCSDeviceState.Idle);
            return;
        }

        SetState(FCSDeviceState.Running);

    }

    public override void OnEnable()
    {
        QuickLogger.Debug($"OnEnable: {GetPrefabID()}");
        base.OnEnable();
        
    }

    #endregion

    private void TryStartingNextCube()
    {
        // Logger.Log(Logger.Level.Debug, "Trying to start another cube", showOnScreen: true);

        if (CurrentSpeedMode == IonCubeGenSpeedModes.Off)
            return;// Powered off, can't start a new cube

        if (StartUpProgress < 0f || // Has not started a cube yet
            CoolDownProgress == CooldownComplete) // Has finished a cube
        {
            if (!_cubeContainer.IsFull)
            {
                // Logger.Log(Logger.Level.Debug, "IonCube Generator - Start up", showOnScreen: true);
                CoolDownProgress = -1f;
                GenerationProgress = -1f;
                StartUpProgress = 0f;
            }
            else
            {
                // Logger.Log(Logger.Level.Debug, "Cannot start another cube, container is full", showOnScreen: true);
            }
        }
        else
        {
            // Logger.Log(Logger.Level.Debug, "Cannot start another cube, another cube is currently in progress", showOnScreen: true);
        }
    }

    private void UpdatePowerRelay()
    {
        PowerRelay relay = PowerSource.FindRelay(transform);
        if (relay != null && relay != _connectedRelay)
        {
            _connectedRelay = relay;
            // Logger.Log(Logger.Level.Debug, "PowerRelay found at last!");
        }
        else
        {
            _connectedRelay = null;
        }
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;

        if (_cubeContainer is not null)
        {
            reason = LanguageService.NotEmpty();
            return _cubeContainer.NumberOfCubes == 0;
        }

        return true;
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;
        if (constructed)
        {
            if (base.isActiveAndEnabled)
            {
                if (!this.IsInitialized)
                {
                    this.Initialize();
                }
                this.IsInitialized = true;
                return;
            }
            _runStartUpOnEnable = true;
        }
    }

    public void OpenStorage()
    {
        StartCoroutine(_cubeContainer.OpenStorage());
    }

    internal void OnAddItemEvent(InventoryItem item)
    {
        Buildable.deconstructionAllowed = false;
    }

    internal void OnRemoveItemEvent(InventoryItem item)
    {
        Buildable.deconstructionAllowed = _cubeContainer.NumberOfCubes == 0;
        TryStartingNextCube();
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        this.PauseUpdates = true;
        QuickLogger.Debug($"In OnProtoDeserialize: {GetPrefabID()}", false);
        if (this._saveData == null)
        {
            var data = ReadySaveData1();

            QuickLogger.Debug($"{data.StartUpProgress}");
        }
        IsFromSave = true;
                           

        this.PauseUpdates = false;
    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {
        this.PauseUpdates = true;
        QuickLogger.Debug($"In OnProtoSerialize: {GetPrefabID()}", false);
        if (!ModSaveManager.IsSaving())
        {
            QuickLogger.Info("Saving " + this.GetPrefabID(), false);
            ModSaveManager.Save();
            QuickLogger.Info("Saved " + this.GetPrefabID(), false);
        }
        this.PauseUpdates = false;
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings",true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, onPAClose);
    }

    private void onPAClose(FCSPDAController pda)
    {
        QuickLogger.Debug("IonCubeGen PDA Closed",true);
        CachedHabitatManager?.OnDeviceUIClosed(this);

        _cubeContainer.AttemptToOpenStorage(pda);
    }

    internal bool IsCrafting()
    {
        return GenerationProgress > -1f && GenerationProgress < CubeEnergyCost;
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving vube Gen",true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new CubeGeneratorSaveData();
        }

        var save = _savedData as CubeGeneratorSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();
        save.NumberOfCubes = NumberOfCubes;

        save.Progress = _progress;


        save.StartUpProgress = StartUpProgress;
        save.GenerationProgress = GenerationProgress;
        save.CoolDownProgress = CoolDownProgress;
        save.CurrentSpeedMode = CurrentSpeedMode;

        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves Cube Gen {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {GetPowerUsage() * 60:F2}] [Running: {_currentMode}] [Percentage: {GenerationPercent:P0}]",
        };
    }

    public override float GetPowerUsage()
    {
        if (CoolDownProgress > 0 || !GameModeUtils.RequiresPower()) return 0;
        return EnergyConsumptionPerSecond;
    }


    public void AddDummyWaring()
    {
        AddWarning("0000", "Test Description", WarningType.High, FaultType.Fault);
        AddWarning("0001", "Test Description", WarningType.Low, FaultType.Warning);
        AddWarning("0002", "Test Description", WarningType.High, FaultType.Fault);

    }

    public void SyncDevice(object device)
    {
        QuickLogger.Debug($"Attempting to sync device {GetPrefabID()}");
        QuickLogger.Debug($"Is Ion Cube: {device is IonCubeGeneratorController}");
        if (device is IonCubeGeneratorController)
        {
            IonCubeGeneratorController masterDevice = device as IonCubeGeneratorController;
            QuickLogger.Debug($"Is Master: {masterDevice == this}");
            if (masterDevice == this) return;
            QuickLogger.Debug($"Setting");
            CurrentSpeedMode = masterDevice.CurrentSpeedMode;
            IsVisibleInPDA = masterDevice.IsVisibleInPDA;
        }
    }
}