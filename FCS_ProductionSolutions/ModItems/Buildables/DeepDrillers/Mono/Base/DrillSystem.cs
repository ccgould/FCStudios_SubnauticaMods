using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Systems;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCSCommon.Utilities;
using FMOD;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;

internal abstract class DrillSystem : FCSDevice
{
    [SerializeField] private GroundDetector groundDetector;
    [SerializeField] private FMOD_CustomLoopingEmitter _customLoopingEmitter;


    private DeviceErrorModel _notEnoughPowerError;
    private DeviceErrorModel _noOilError;
    private DeviceErrorModel _drillTurnedOffError;
    private DeviceErrorModel _inventoryFullError;
    private DeviceErrorModel _notOnTerrainError;


    protected BiomeDetection _biomeDetector;
    protected PingInstance _ping;
    protected bool _isBreakSet;
    protected StringBuilder _sb = new();

    private FCSDeepDrillerAnimationHandler _animationHandler;
    private FCSDeviceErrorHandler _err;
    private FCSDeepDrillerContainer _deepDrillerContainer;
    private FCSDeepDrillerOreGenerator _oreGenerator;
    private DumpContainer _oilDumpContainer;
    //private AudioSource _audio;
    //private AudioLowPassFilter _lowPassFilter;
    private FCSDeepDrillerOilHandler _oilHandler;
    private bool _noBiomeMessageSent;
    private bool _biomeFoundMessageSent;
    private bool _wasPlaying;
    private HoverInteraction _interaction;
    private bool canOperate = true;
    private bool _errorsInitialize;
    internal Action<PowercellData> OnBatteryLevelChange { get; set; }
    internal Action<float> OnOilLevelChange { get; set; }
    internal bool IsBeingDeleted { get; set; }
    internal FCSPowerManager PowerManager { get; private protected set; }
    internal abstract bool UseOnScreenUi { get; }

    public override bool IsOperational()
    {

        if(!_errorsInitialize) return false;

        canOperate = true;
               
        if(!PowerManager.HasEnoughPowerToOperate())
        {
            TriggerError(_notEnoughPowerError);
        }

        if (PowerManager.GetPowerState() != FCSPowerStates.Powered)
        {
           canOperate = false;
        }

        if (!_oilHandler.HasOil())
        {
            TriggerError(_noOilError);
        }

        if (_isBreakSet)
        {
            TriggerError(_drillTurnedOffError);
        }

        if (_deepDrillerContainer.IsFull)
        {
            TriggerError(_inventoryFullError);
        }

        if(!groundDetector.IsGroundVisible())
        {
            TriggerError(_notOnTerrainError);
        }

        return canOperate;
    }



    private void InitializeErrors()
    {
        _notEnoughPowerError = new DeviceErrorModel("PS_NoPower", errorHandler, () =>
        {
            return PowerManager.HasEnoughPowerToOperate();
        });

        //_drillPoweredOffError.SetData(errorHandler, () =>
        //{
        //    return PowerManager.GetPowerState() == FCSPowerStates.Powered;
        //});

        _noOilError = new DeviceErrorModel("PS_NeedsOil", errorHandler, () =>
        {
            return _oilHandler.HasOil();
        });

        _drillTurnedOffError = new DeviceErrorModel("AHB_DeviceTurnedOff", errorHandler, () =>
        {
            return !_isBreakSet;
        });

        _inventoryFullError = new DeviceErrorModel("InventoryFull", errorHandler, () =>
        {
            return !_deepDrillerContainer.IsFull;
        });

        _notOnTerrainError = new DeviceErrorModel("AHB_NotOnTerrain",errorHandler, () =>
        {
            return groundDetector.IsGroundVisible();
        });

        _errorsInitialize = true;

    }
    
    public override void Awake()
    {
        base.Awake();

        //sfx.SetParameterValue(FMOD.Studio.PARAMETER_ID)

        //CustomSoundHandler.TryPlayCustomSound("DrillSound", out Channel channel);

        _interaction = gameObject.GetComponent<HoverInteraction>();
        _oilHandler = gameObject.GetComponent<FCSDeepDrillerOilHandler>();
        _animationHandler = gameObject.GetComponent<FCSDeepDrillerAnimationHandler>();
        _deepDrillerContainer = gameObject.GetComponent<FCSDeepDrillerContainer>();
        _oreGenerator = gameObject.GetComponent<FCSDeepDrillerOreGenerator>();
        _oilDumpContainer = gameObject.GetComponent<DumpContainer>();
        _oilHandler = gameObject.GetComponent<FCSDeepDrillerOilHandler>();
        _ping = gameObject.GetComponent<PingInstance>();
        _biomeDetector = gameObject.GetComponent<BiomeDetection>();

        InitializeErrors();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (IsBeingDeleted) return;

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            _runStartUpOnEnable = false;
        }
    }

    public override void OnDeviceRegistered()
    {
       UpdateBeaconName();
    }

    internal BiomeDetection GetBiomeDetector()
    {
        return _biomeDetector;
    }

    internal FCSDeepDrillerOreGenerator GetOreGenerator()
    {
        return _oreGenerator;    
    }

    internal FCSDeepDrillerOilHandler GetOilHandler()
    {
        return _oilHandler;
    }

    private void UpdateDrillShaftState()
    {
        if (!IsConstructed || !IsInitialized) return;

        if (GetIsDrilling())
        {
            _animationHandler.DrillState(true);
            PlaySFX();
        }
        else
        {
            _animationHandler.DrillState(false);
            StopSFX();

        }
    }

    internal DumpContainer GetOilDumpContainter() => _oilDumpContainer;

    internal FCSDeepDrillerContainer GetDDContainer() { return _deepDrillerContainer; }

    public void OreGeneratorOnAddCreated(TechType type)
    {
        QuickLogger.Debug($"Adding Item {type} to Deep Driller Storage", true);
        _deepDrillerContainer.AddItemToContainer(type);
    }

    internal virtual void Update()
    {
        //if (_lowPassFilter != null)
        //{
        //    _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() ||
        //                                     Player.main.IsInBase() ||
        //                                     Player.main.IsInSub() ||
        //                                     Player.main.inSeamoth ||
        //                                     Player.main.inExosuit ? 1566f : 22000f;
        //}

        if (WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 5f) && IsOperational() && !IsBreakSet())
        {
            MainCameraControl.main.ShakeCamera(.3f);
        }
    }

    protected void UpdateEmission()
    {
        MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject,
            _isBreakSet ? Color.red : Color.cyan);
    }

    internal virtual void ConnectDisplay() { }

    public override void Start()
    {
        base.Start();
        
        InvokeRepeating(nameof(UpdateDrillShaftState), .5f, .5f);
        
        UpdateEmission();

        if (_savedData == null)
        {
            ReadySaveData();
        }

        LoadSave();
    }

    internal void UpdateBeaconName(string beaconName = null)
    {
        
        if (string.IsNullOrWhiteSpace(beaconName))
        {
            var defaultName = $"Deep Driller - {UnitID}";
            SetPingName(defaultName);
        }
        else
        {
            SetPingName(beaconName);
        }
    }

    public override void OnDestroy()
    {
        CachedHabitatManager?.UnRegisterDevice(this);
        base.OnDestroy();
        IsBeingDeleted = true;
    }

    internal abstract void LoadSave();

    public override void Initialize()
    {
        base.Initialize();
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        IsInitialized = true;
    }

    internal bool IsBreakSet()
    {
        return _isBreakSet;
    }

    internal void StopSFX()
    {
        if (_customLoopingEmitter.playing)
            _customLoopingEmitter.Stop();
    }

    internal void PlaySFX()
    {
        if (!_customLoopingEmitter.playing)
            _customLoopingEmitter.Play();
    }

    internal void SetPingName(string beaconName)
    {
        _ping.Initialize();
        _ping.SetLabel(beaconName);
        PingManager.NotifyRename(_ping);
    }

    internal void EmptyDrill()
    {
        _deepDrillerContainer.Clear();
    }

    internal bool GetIsDrilling()
    {
        return _oreGenerator.GetIsDrilling();
    }

    internal virtual bool IsPowerAvailable()
    {
        return PowerManager?.IsPowerAvailable() ?? false;
    }

    public virtual IEnumerable<UpgradeFunction> GetUpgrades()
    {
        return null;
    }

    public virtual List<PistonBobbing> GetPistons()
    {
        return null;
    }

    internal void ToggleVisibility(bool value = false)
    {
        _ping.SetVisible(value);
        PingManager.NotifyVisible(_ping);
    }

    internal void ToggleBeacon()
    {
        _ping.enabled = !_ping.enabled;
    }

    public bool IsUnderWater()
    {
        return GetDepth() >= 0.63f;
    }

    internal float GetDepth()
    {
        return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
    }

    //public override bool ChangeBodyColor(ColorTemplate template)
    //{
    //    return _colorManager.ChangeColor(template);
    //}

    //#region IProtoEventListener

    public abstract void Save(SaveData saveDataList, ProtobufSerializer serializer = null);

    public float GetPowerCharge()
    {
        return PowerManager.GetDevicePowerCharge();
    }

    public PowercellData GetBatteryPowerData()
    {
        return PowerManager?.GetBatteryPowerData();
    }

    public float GetOilPercentage()
    {
        return _oilHandler?.GetOilPercent() ?? 0f;
    }

    public string GetOresPerDayCount()
    {
        return _oreGenerator?.GetItemsPerDay();
    }

    public int GetOresPerDayCountInt()
    {
        return _oreGenerator?.GetItemsPerDayInt() ?? 0;
    }

    public string GetPowerUsageAmount()
    {
        return PowerManager?.GetPowerUsage().ToString();
    }

    public bool GetBeaconState()
    {
        return _ping?.visible ?? false;
    }

    internal void SetBeaconState(bool value)
    {
        _ping?.SetVisible(value);
    }

    public string GetPingName()
    {
        return _ping?.GetLabel();
    }

    public override void ReadySaveData()
    {

    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    private void TriggerError(DeviceErrorModel errorSo)
    {
        errorHandler.TriggerError(errorSo);
        canOperate = false;
    }
}