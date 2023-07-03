﻿using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Languages;
using FCSCommon.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using static FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.CubeGeneratorStateManager;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;

internal class uGUI_IonCube : Page, IuGUIAdditionalPage
{ 
    private IonCubeGeneratorController _controller;
    [SerializeField]
    private GameObject _powerOffPage;
    [SerializeField]
    private GameObject _operationPage;
    [SerializeField]
    private GameObject _clocking;
    [SerializeField]
    private TMP_Text _speedMode;
    [SerializeField]
    private TMP_Text _percentDisplay;
    [SerializeField]
    private Image _percentageBar;
    [SerializeField]
    private Image _storageBar;
    [SerializeField]
    private TMP_Text _storageAmount;
    private bool _initialized;
    private bool _coroutineStarted;
    [SerializeField]
    private TMP_Text _overclocking;
    [SerializeField]
    private TMP_Text _storageLBL;
    [SerializeField]
    private TMP_Text _completedTxt;
    private MenuController _menuController;
    private readonly Color _fireBrick = new Color(0.6953125f, 0.1328125f, 0.1328125f);
    private readonly Color _cyan = new Color(0.13671875f, 0.7421875f, 0.8046875f);
    private readonly Color _green = new Color(0.0703125f, 0.92578125f, 0.08203125f);
    private readonly Color _orange = new Color(0.95703125f, 0.4609375f, 0f);
    private const float MaxBar = 100;
    private const float BarMinValue = 0.087f;
    private const float BarMaxValue = 0.409f;
    private const int MaxContainerSpaces = CubeGeneratorContainer.MaxAvailableSpaces;
    private const float DelayedStartTime = 0.5f;
    private const float RepeatingUpdateInterval = 1f;

    public override void Enter(object obj)
    {
        base.Enter(obj);

        FindAllComponents();

        if(obj is not null) 
        {
            _controller = obj as IonCubeGeneratorController;
        }

        _initialized = true;

        //Set data before screen is shown.
        //UpdateDisplay();

        //Update the speed text. I dont want this to be updates every second
        UpdateSpeedText();
    }

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }

    public override void Awake()
    {
        base.Awake();

        if (!_coroutineStarted)
            InvokeRepeating(nameof(UpdateDisplay), DelayedStartTime * 3f, RepeatingUpdateInterval);

        DisplayLanguagePatching.AdditionPatching();
    }

    private void UpdateDisplay()
    {
        if (!_initialized)
            return;

        _coroutineStarted = true;

        UpdateDeviceName();

        UpdatePercentageBar();

        UpdateStoragePercentBar();

        UpdateStorageAmount();

        UpdatePercentageText();
    }

    private void UpdateDeviceName()
    {
        

        var isNameSimlar = _controller.GetDeviceName().Equals(_controller.UnitID);

        if(isNameSimlar)
        {
            FCSPDAController.Main.ui.SetPDAAdditionalLabel(_controller.GetDeviceName());
        }
        else
        {
            FCSPDAController.Main.ui.SetPDAAdditionalLabel($"{_controller.GetDeviceName()} [{_controller.UnitID}]");
        }

    }

    private bool FindAllComponents()
    {
        if (_initialized) return true;

        //#region PowerOffPage

        //_powerOffPage = GameObjectHelpers.FindGameObject(gameObject, "PowerOffPage");

        //if (_powerOffPage == null)
        //{
        //    QuickLogger.Error("PowerOffPage not found.");
        //    return false;
        //}
        //#endregion

        //#region OperationPage

        //_operationPage = GameObjectHelpers.FindGameObject(gameObject, "OperationPage");

        //if (_operationPage == null)
        //{
        //    QuickLogger.Error("OperationPage not found.");
        //    return false;
        //}
        //#endregion


        _completedTxt.text = GetLanguage(DisplayLanguagePatching.CompletedKey);
        _storageLBL.text = GetLanguage(DisplayLanguagePatching.StorageKey);
        _overclocking.text = GetLanguage(DisplayLanguagePatching.OverClockKey);
        return true;
    }

    public void OnStorageButtonClicked()
    {
        CoroutineHost.StartCoroutine(_controller.OpenStorage());
    }

    public void ProcessSpeedChange(string buttonID)
    {
        switch (buttonID)
        {
            case "LButton":
                switch (_controller.CurrentSpeedMode())
                {
                    case IonCubeGenSpeedModes.Max:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.High);
                        break;
                    case IonCubeGenSpeedModes.High:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Low);
                        break;
                    case IonCubeGenSpeedModes.Low:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Min);
                        break;
                    case IonCubeGenSpeedModes.Min:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Off);
                        break;
                }
                break;

            case "RButton":
                switch (_controller.CurrentSpeedMode())
                {
                    case IonCubeGenSpeedModes.High:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Max);
                        break;
                    case IonCubeGenSpeedModes.Low:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.High);
                        break;
                    case IonCubeGenSpeedModes.Min:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Low);
                        break;
                    case IonCubeGenSpeedModes.Off:
                        _controller.SetCurrentSpeedMode(IonCubeGenSpeedModes.Min);
                        break;
                }
                break;
            default:
                QuickLogger.Debug(_controller.CurrentSpeedMode().ToString(), true);
                throw new ArgumentOutOfRangeException();
        }

        UpdateSpeedText();
    }

    private void UpdateSpeedText()
    {
        switch (_controller.CurrentSpeedMode())
        {
            case IonCubeGenSpeedModes.Off:
                _speedMode.text = GetLanguage(DisplayLanguagePatching.OffKey);
                break;
            case IonCubeGenSpeedModes.Max:
                _speedMode.text = GetLanguage(DisplayLanguagePatching.MaxKey);
                break;
            case IonCubeGenSpeedModes.High:
                _speedMode.text = GetLanguage(DisplayLanguagePatching.HighKey);
                break;
            case IonCubeGenSpeedModes.Low:
                _speedMode.text = GetLanguage(DisplayLanguagePatching.LowKey);
                break;
            case IonCubeGenSpeedModes.Min:
                _speedMode.text = GetLanguage(DisplayLanguagePatching.MinKey);
                break;
        }
    }

    private string GetLanguage(string key)
    {
        return Language.main.Get(key);
    }

    private void UpdatePercentageText()
    {
        _percentDisplay.text = $"{Mathf.RoundToInt(_controller.GetStateManager().GetState(CubeGeneratorStates.Generating).GetProgressNormalized() * 100)}%";
    }

    private void UpdatePercentageBar()
    {
        if (_controller == null)
        {
            QuickLogger.Error("Mono is null");
            return;
        }

        //float calcBar = _mono.GenerationPercent / MaxBar;

        float outputBar = _controller.GetStateManager().GetState(CubeGeneratorStates.Generating).GetProgressNormalized() * (BarMaxValue - BarMinValue) + BarMinValue;

        _percentageBar.fillAmount = Mathf.Clamp(outputBar, BarMinValue, BarMaxValue);

    }

    private void UpdateStoragePercentBar()
    {
        float calcBar = (float)(_controller.NumberOfCubes() * 1.0 / (MaxContainerSpaces * 1.0));
        float outputBar = calcBar * (BarMaxValue - BarMinValue) + BarMinValue;
        _storageBar.fillAmount = Mathf.Clamp(outputBar, BarMinValue, BarMaxValue);

    }

    private void UpdateStorageAmount()
    {
        _storageAmount.text = $"{_controller.NumberOfCubes()}/{MaxContainerSpaces}";

        float percent = (float)(_controller.NumberOfCubes() * 1.0 / MaxContainerSpaces * 1.0) * 100.0f;

        if (Mathf.Round(percent) <= 25)
        {
            _storageBar.color = _cyan;
        }

        if (Mathf.Round(percent) > 25 && percent <= 50)
        {
            _storageBar.color = _green;
        }

        if (Mathf.Round(percent) > 50 && percent <= 75)
        {
            _storageBar.color = _orange;
        }

        if (Mathf.Round(percent) > 75 && percent <= 100)
        {
            _storageBar.color = _fireBrick;
        }
    }

    public IFCSObject GetController()
    {
        return _controller;
    }

    public override void Exit()
    {
        base.Exit();

    }
}