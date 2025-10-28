using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseUtilityUnit.Mono;
internal class BaseUtilityUnitController : FCSDevice, IFCSSave
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private ParticleSystem[] _bubbles;

    [SerializeField] private Text _percent;
    [SerializeField] private Image _percentBar;
    [SerializeField] private MotorHandler _fanMotor;
    
    private FMOD_CustomEmitter _audioManager;
    private bool _prevPowerState;
    private HoverInteraction _interactionHelper;
    private BaseUtilityOxygenManager oxygenManager;
    private int _isRunningHash;
    private AnimationManager _animationManager;


    public override void Awake()
    {
        base.Awake();
        _audioManager = GetComponent<FMOD_CustomEmitter>();
        _interactionHelper = GetComponent<HoverInteraction>();
        _animationManager = GetComponent<AnimationManager>();
        oxygenManager = GetComponent<BaseUtilityOxygenManager>();
        _isRunningHash = Animator.StringToHash("IsRunning");

    }

    public override void Start()
    {
        base.Start();


        if (CachedHabitatManager == null)
        {
            TurnOffDevice();
        }
        else
        {
            TurnOnDevice();
        }
    }

    public override Vector3 GetPosition()
    {
        return transform.position;
    }

    public override void OnEnable()
    {
        _interactionHelper.onSettingsKeyPressed += InteractionHelper_onSettingsKeyPressed;

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

                var save = _savedData as BaseUtilityEntry;

                oxygenManager.SetO2Level(save.O2Level);
                //_colorManager.LoadTemplate(save.ColorTemplate);

            }

            _runStartUpOnEnable = false;
        }
    }

    private void OnDisable()
    {
        _interactionHelper.onSettingsKeyPressed -= InteractionHelper_onSettingsKeyPressed;
    }

    private void InteractionHelper_onSettingsKeyPressed(TechType obj)
    {
        QuickLogger.Debug("[Base Utility Unit] Take Oxygen", true);

        oxygenManager.GivePlayerO2();
    }

    public override void Initialize()
    {
        base.Initialize();
        oxygenManager.Initialize(this);
        oxygenManager.OnOxygenUpdated += (amount, percentage) =>
        {
            _percent.text = $"{percentage:P0}";
            _percentBar.fillAmount = percentage;
        };

        InvokeRepeating(nameof(UpdateAnimation), 1f, 1f);
        IsInitialized = true;
    }

    private void UpdateAnimation()
    {
        if (CachedHabitatManager == null)
        {
            _fanMotor.StopMotor();
            CancelInvoke(nameof(UpdateAnimation));
            return;
        }
        var currentState = CachedHabitatManager.HasEnoughPower(GetPowerUsage());
        if (_prevPowerState == currentState) return;
        _prevPowerState = currentState;

        foreach (ParticleSystem bubble in _bubbles)
        {
            if (currentState)
            {
                bubble.Play();
                _fanMotor.StartMotor();
                _audioManager.Play();
            }
            else
            {
                bubble.Stop();
                _fanMotor.StopMotor();
                _audioManager.Stop();
            }
        }

        _animationManager.SetBoolHash(_isRunningHash, currentState);
    }

    public override float GetPowerUsage()
    {
        return CachedHabitatManager == null ? 0f : energyPerSecond;
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;
        if (constructed)
        {
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                IsInitialized = true;
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }
    }

    public override void TurnOffDevice()
    {
        _canvas.SetActive(false);
    }

    public override void TurnOnDevice()
    {
        _canvas.SetActive(true);
    }


    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = FCSModsAPI.PublicAPI.GetSaveData<BaseUtilityEntry>(id);//ModSaveManager.GetSaveData<BaseUtilityEntry>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void SaveDevice()
    {
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new BaseUtilityEntry();
        }

        var save = _savedData as BaseUtilityEntry;


        save.Id = GetPrefabID();
        save.O2Level = oxygenManager.GetO2Level();
        save.ColorTemplate = _colorManager.SaveTemplate();
        QuickLogger.Debug($"Saving ID {save.Id}");
        //newSaveData.Data.Add(save);
        FCSModsAPI.PublicAPI.PushSaveData(save);
    }

    internal BaseUtilityOxygenManager GetOxygenManager()
    {
        return oxygenManager;
    }
}
