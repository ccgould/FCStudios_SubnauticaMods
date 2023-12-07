using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Buildable;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Enums;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Managers;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FCS_AlterraHub.Configuation.SaveData;
using static RootMotion.FinalIK.InteractionTrigger.Range;

namespace FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;

internal class OreCrusherController : FCSDevice, IFCSSave<SaveData>
{

    private const int MAXITEMLIMIT = 10;

    private bool _wasDrillSoundPlaying;
    private Queue<TechType> _oreQueue = new();
    private float _timeLeft;
    private bool _isBreakerTripped;
    private OreConsumerStatus _status = OreConsumerStatus.None;
    public Action OnProcessingCompleted { get; set; }

    [SerializeField] private List<PistonBobbing> _pistons;
    [SerializeField] private MotorHandler crusherMotorHandler;
    [SerializeField] private MotorHandler antennaMotorHandler;
    [SerializeField] private HoverInteraction _interaction;
    [SerializeField] private DumpContainer _dumpContainer;
    [SerializeField] private FCSStorage _storage;
    [SerializeField] private AudioSource _drillSound;
    [SerializeField] private AudioLowPassFilter _lowPassFilter;


    private OreCrusherEffectsManager effectsManager;
    private OreConsumerSpeedModes _currentSpeedMode = OreConsumerSpeedModes.Min;
    private OreConsumerSpeedModes _pendingSpeedMode = OreConsumerSpeedModes.Min;


    public override void Awake()
    {
        base.Awake();           
    }

    internal OreConsumerSpeedModes GetPendingSpeedMode()
    { 
        return _pendingSpeedMode; 
    }

    public override float GetPowerUsage()
    {
        return _oreQueue != null && _oreQueue?.Count > 0 && !_isBreakerTripped ? energyPerSecond * (int)_currentSpeedMode : 0;
    }

    public override void Start()
    {
        effectsManager = gameObject.GetComponent<OreCrusherEffectsManager>();

        _storage.ItemsContainer.onAddItem += AddItemToContainer;
        _storage.isAllowedToAdd = new IsAllowedToAdd(IsAllowedToAdd);

        base.Start();
    }

    public override void OnEnable()
    {
        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();


                }


                var save = _savedData as OreConsumerDataEntry;


                if (save.OreQueue != null)
                {
                    _oreQueue = save.OreQueue;
                    _timeLeft = save.TimeLeft;
                }

                if (save.IsBreakerTripped)
                {
                    _isBreakerTripped = true;
                }

                _currentSpeedMode = save.CurrentSpeedMode == OreConsumerSpeedModes.Off ? OreConsumerSpeedModes.Low : save.CurrentSpeedMode;
                _pendingSpeedMode = save.PendingSpeedMode == OreConsumerSpeedModes.Off ? OreConsumerSpeedModes.Low : save.PendingSpeedMode;
                crusherMotorHandler.SpeedByPass(save.RPM);
               // _colorManager.LoadTemplate(save.ColorTemplate);
            }

            _runStartUpOnEnable = false;
        }
    }

    private void Update()
    {
        if (!IsConstructed || !IsInitialized || CachedHabitatManager == null) return;

        if (_lowPassFilter != null)
        {
            _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() ||
                                             Player.main.IsInBase() ||
                                             Player.main.IsInSub() ||
                                             Player.main.inSeamoth ||
                                             Player.main.inExosuit ? 1566f : 22000f;
        }

        if (_drillSound != null && _drillSound.isPlaying)
        {
            if (WorldHelpers.CheckIfPaused())
            {
                _drillSound.Pause();
                _wasDrillSoundPlaying = true;
            }

            if (_wasDrillSoundPlaying && !WorldHelpers.CheckIfPaused())
            {
                _drillSound.Play();
                _wasDrillSoundPlaying = false;
            }
        }

        if (IsOperational() && _oreQueue.Count > 0)
        {
            //effectsManager.TriggerFX();

            _timeLeft -= DayNightCycle.main.deltaTime;

            if (_timeLeft < 0)
            {
                if (_currentSpeedMode != _pendingSpeedMode)
                {
                    _currentSpeedMode = _pendingSpeedMode;
                }

                AppendMoney(GetOreValue());
                _oreQueue.Dequeue();
                CalculateTimeLeft();
                //effectsManager.TriggerFX();
                OnProcessingCompleted?.Invoke();
            }
        }

        if (WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 5f) && IsOperational() && _oreQueue.Any() && Plugin.Configuration.OreCrusherCameraShake)
        {
            MainCameraControl.main.ShakeCamera(.3f);
        }
    }

    public decimal GetOreValue()
    {
        if (!_oreQueue.Any()) return 0;
        return OreManager.GetOrePrice(_oreQueue.Peek());
    }

    private void AppendMoney(decimal price)
    {
        decimal deduction = 0;
        AccountService.main.AddFinances(price);

        if (GamePlayService.Main.GetAutomaticDebitDeduction() && !AccountService.main.IsDebitPaid())
        {
            QuickLogger.Debug("Getting ready to deduct", true);
            deduction = MathHelpers.PercentageOfNumber(Convert.ToDecimal(GamePlayService.Main.GetRate()), price);
            AccountService.main.PayDebit(null, deduction);
        }
    }

    public bool CanBeStored(int amount, TechType techType)
    {
        if (amount + _oreQueue.Count > MAXITEMLIMIT) return false;
        return OreManager.ValidResource(techType);
    }

    public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        return CanBeStored(_dumpContainer.GetCount() + 1, pickupable.GetTechType());
    }

    internal string GetProcessingItemString()
    {
        return !_oreQueue.Any() ? Language.main.Get("Empty") : Language.main.Get(_oreQueue.Peek());
    }

    public string GetTimeLeftString()
    {
        return _timeLeft.ToString("N0");
    }

    public Queue<TechType> GetOreQueue()
    {
        return _oreQueue;
    }

    public void ChangeSpeedMultiplier(OreConsumerSpeedModes value)
    {
        if (_oreQueue.Any())
        {
            _pendingSpeedMode = value;
        }
        else
        {
            _pendingSpeedMode = _currentSpeedMode = value;
            CalculateTimeLeft();
            CalculateTargetTime();
        }
    }

    public void AddItemToContainer(InventoryItem item)
    {
        try
        {
            _oreQueue.Enqueue(item.item.GetTechType());
            Destroy(item.item.gameObject);
        }
        catch (Exception e)
        {
            QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
        }
    }

    public override void Initialize()
    {
        if (IsInitialized) return;

        CalculateTimeLeft();

        InvokeRepeating(nameof(UpdateVisibleElements), 1, 1);

        InvokeRepeating(nameof(UpdateAnimation), 1f, 1f);

        antennaMotorHandler.StartMotor();

        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;

        _interaction.IsAllowedToInteract += Interaction_IsAllowedToInteract;

        base.Initialize();
    }

    private bool Interaction_IsAllowedToInteract()
    {
        return AccountService.main.HasBeenRegistered();
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, onPDAClose);
    }

    internal void OpenStorage()
    {
        _dumpContainer.OpenStorage();
    }

    private void onPDAClose(FCSPDAController pda)
    {
        //deviceNameLbl.text = GetDeviceName();
    }

    private void UpdateVisibleElements()
    {

        if (CachedHabitatManager == null)
        {
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
            _status = OreConsumerStatus.None;
            return;
        }

        if (IsOperational() && !antennaMotorHandler.IsRunning)
        {
            antennaMotorHandler.StartMotor();
        }
        else if (antennaMotorHandler.IsRunning)
        {
            antennaMotorHandler.StopMotor();
        }

        var isPowered = !_isBreakerTripped && CachedHabitatManager.GetPowerState() != PowerSystem.Status.Offline;

        if (!isPowered && _status != OreConsumerStatus.Tripped)
        {
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
            _status = OreConsumerStatus.Tripped;
            return;
        }

        if (_oreQueue.Any() && isPowered && IsOperational() && _status != OreConsumerStatus.Running)
        {
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            _status = OreConsumerStatus.Running;
            return;
        }

        if (!_oreQueue.Any() && _status != OreConsumerStatus.Idle && isPowered)
        {
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.yellow);
            _status = OreConsumerStatus.Idle;
        }
    }

    public override bool IsOperational()
    {
        if (IsInitialized && 
            IsConstructed && 
            CachedHabitatManager != null && 
            _oreQueue != null && 
            CachedHabitatManager.HasEnoughPower(GetPowerUsage()) && 
            !_isBreakerTripped) return true;

        //&& CachedHabitatManager.IsDeviceConnected(GetPrefabID())
        return false;
    }

    private void UpdateAnimation()
    {
        if (!IsConstructed || !IsInitialized || CachedHabitatManager == null) return;

        if (_oreQueue != null && _oreQueue.Count > 0 && (CachedHabitatManager.GetPowerState() == PowerSystem.Status.Normal || CachedHabitatManager.GetPowerState() == PowerSystem.Status.Emergency) && !_isBreakerTripped)
        {
            crusherMotorHandler.StartMotor();
            crusherMotorHandler.RPMByPass(30 * (int)_currentSpeedMode);
            crusherMotorHandler.SetIncreaseRate(5);
            foreach (PistonBobbing piston in _pistons)
            {
                piston.SetState(true);
            }
            if (_drillSound != null && !_drillSound.isPlaying)
            {
                _drillSound.Play();
            }
        }
        else
        {
            if (_drillSound != null && _drillSound.isPlaying)
            {
                _drillSound.Stop();
            }
            crusherMotorHandler.StopMotor();
            foreach (PistonBobbing piston in _pistons)
            {
                piston.SetState(false);
            }
        }
    }

    private void CalculateTimeLeft()
    {
        _timeLeft = CalculateTargetTime();
    }

    private float CalculateTargetTime()
    {
        return OreCrusherBuildable.OreProcessingTime / (int)_currentSpeedMode;
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<OreConsumerDataEntry>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new OreConsumerDataEntry();
        }

        var save = _savedData as OreConsumerDataEntry;


        save.Id = GetPrefabID();
        save.OreQueue = _oreQueue;
        save.TimeLeft = _timeLeft;
        save.RPM = crusherMotorHandler.GetRPM();
        //save.ColorTemplate = _colorManager.SaveTemplate();
        save.BaseId = CachedHabitatManager.GetBaseID().ToString();
        save.IsBreakerTripped = _isBreakerTripped;
        save.CurrentSpeedMode = _currentSpeedMode;
        save.PendingSpeedMode = _pendingSpeedMode;
        QuickLogger.Debug($"Saving ID {save.Id}", true);
        newSaveData.Data.Add(_savedData);
    }

    public void Test()
    {
        StartCoroutine(AddDepletedRodToEquipmentAsync());
    }

    private IEnumerator AddDepletedRodToEquipmentAsync()
    {
        CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Copper, true);
        yield return request;
        Pickupable component = Instantiate(request.GetResult()).GetComponent<Pickupable>();
        component.Pickup(false);
        if(IsAllowedToAdd(component,false))
        {
            AddItemToContainer(new InventoryItem(component));
        }
        else
        {
            Destroy(component.gameObject);
        }

        yield break;
    }

    public override string[] GetDeviceStats()
    {
        var totalTime = CalculateTargetTime();
        var remainingTime = totalTime - _timeLeft;
        var percentage = remainingTime / totalTime;

        return new string[]
        {
            $"[EPM: {GetPowerUsage() * 60:F2}] [Status: {_status}] [Percentage: {percentage:P0}]",
        };
    }

}