using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UWE;
using static FCS_EnergySolutions.Configuration.SaveData;



namespace FCS_EnergySolutions.ModItems.Buildables.JetStream.Mono;

internal class JetStreamController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField]private JetStreamT242PowerManager _powerManager;
    [SerializeField]private MotorHandler _motor;
    [SerializeField]private RotorHandler _tilter;
    [SerializeField]private RotorHandler _rotor;
    [SerializeField]private HoverInteraction _interaction;

    private int _isOperational;

    #region Unity Methods       

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

                QuickLogger.Debug("Loading Save", true);

                var save = _savedData as JetStreamT242SaveData;

                //_colorManager.LoadTemplate(_savedData.ColorTemplate);

                //QuickLogger.Debug($"IsIncreasing: {save.MotorSaveData.IsIncreasing} | Is Underwater: {IsUnderWater()}", true);


                if (save.MotorSaveData.IsIncreasing && IsUnderWater())
                {
                    //ChangeStatusLight();
                    //_rotor?.ResetToMag();
                    //_rotor?.Run();
                    QuickLogger.Debug("Attempting to turn on device from save", true);

                    CoroutineHost.StartCoroutine(LoadFromSave(save));
                    
                }
                else
                {
                    TurnOffDevice();
                }

            }

            //_runStartUpOnEnable = false;
        }
    }

    private IEnumerator LoadFromSave(JetStreamT242SaveData save)
    {
        while (!LargeWorldStreamer.main.IsWorldSettled())
        {
            yield return null;
        }
        QuickLogger.Debug("Trying to From Save");
        TurnOnDevice();
        //_powerManager.LoadFromSave(save);
        //_motor?.Load(save.MotorSaveData);
        //_tilter?.LoadSave(save.RotorSaveData);
        QuickLogger.Debug("Leaded From Save");

        yield break;
    }

    #endregion

    public override void Awake()
    {
        base.Awake();

        _isOperational = Animator.StringToHash("IsOperational");
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
    }

    private void onSettingsKeyPressed(TechType type)
    {
        QuickLogger.Debug("On settings pressed", true);
        _powerManager.ToggleDevice();
    }

    internal bool IsUpright()
    {
        if (Mathf.Approximately(transform.up.y, 1f))
        {
            return true;
        }

        return false;
    }

    public override Vector3 GetPosition()
    {
        return transform.position;
    }

    public override void Initialize()
    {
        TurnOffDevice();
        //MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 5);
        //MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
        IsInitialized = true;
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

    public bool IsUnderWater()
    {
        return GetDepth() > 3.0f;
    }

    internal float GetDepth()
    {
        return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
    }

    public float GetCurrentSpeed()
    {
        if (_motor == null || _tilter == null || _rotor == null) return 0;
        return _motor.GetSpeed();
    }

    public override void TurnOnDevice()
    {
        QuickLogger.Debug("Turn On", true);

        _tilter?.ChangePosition(0, false);
        _tilter?.Run();

        _rotor?.ResetToMag();
        _rotor?.Run();

        _motor?.RPMByPass(_powerManager.GetBiomeData());
        _motor?.StartMotor();

        ChangeStatusLight();

        _powerManager?.ChangePowerState(FCSPowerStates.Powered);
    }

    public override void TurnOffDevice()
    {
        QuickLogger.Debug("Turn Off", true);
        ChangeStatusLight(false);
        _tilter?.ChangePosition(-90);
        _tilter?.Stop();
        _motor?.StopMotor();
        _powerManager?.ChangePowerState(FCSPowerStates.Tripped);
    }

    private void ChangeStatusLight(bool isOperational = true)
    {
        if (isOperational)
        {

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
        }
        else
        {
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
        }
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<JetStreamT242SaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new JetStreamT242SaveData();
        }

        var save = _savedData as JetStreamT242SaveData;


        save.Id = GetPrefabID();

        QuickLogger.Debug($"Saving ID {save.Id}", true);
        //_savedData.ColorTemplate = _colorManager.SaveTemplate();
        _powerManager.Save(save);
        
        save.MotorSaveData = _motor.Save();
        save.RotorSaveData = _tilter.Save();
        newSaveData.Data.Add(save);
    }

    public override string[] GetDeviceStats()
    {

        if (!IsUpright())
        {
            return new[]
            {
                        AuxPatchers.NotOnPlatform()
            };
        }


        if (!IsUnderWater())
        {
            return new[]
            {
                        AuxPatchers.NotUnderWater(),
                        AuxPatchers.NotUnderWaterDesc()
            };
        }

        return new[]
        {
                    Language.main.GetFormat(AuxPatchers.JetStreamOnHover(),
                        UnitID,Mathf.RoundToInt(_powerManager.GetPowerSource().GetPower()),
                        Mathf.RoundToInt(_powerManager.GetPowerSource().GetMaxPower()),
                        (_powerManager.GetEnergyProducing() * 60).ToString("N1")),
                    AuxPatchers.JetStreamCurrentStateFormatted(_powerManager.GetPowerState().ToString())

        };
        //data1.HandHoverPDAHelperEx(techType);

        //if (GameInput.GetButtonDown(GameInput.Button.Exit))
        //{
        //    if (_powerState != FCSPowerStates.Powered)
        //    {
        //        _mono.ActivateTurbine();
        //    }
        //    else
        //    {
        //        _mono.DeActivateTurbine();
        //    }
        //}





        //return new string[]
        //{
        //     AuxPatchers.SolarClusterHover(Mathf.RoundToInt(_powerManager.GetRechargeScalar() * 100f),
        //                Mathf.RoundToInt(_powerManager.GetPower()), Mathf.RoundToInt(_powerManager.GetMaxPower()),
        //                Mathf.RoundToInt((_powerManager.GetRechargeScalar() * 0.20f/*0.25f old value */ * 5f) * 13f))
        //};
    }
}