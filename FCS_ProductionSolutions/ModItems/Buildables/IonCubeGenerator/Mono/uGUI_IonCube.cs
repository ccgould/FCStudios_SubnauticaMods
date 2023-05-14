using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Buildable;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Languages;
using FCSCommon.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



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
    private Text _deviceName;
    [SerializeField]
    private TMP_Text _overclocking;
    [SerializeField]
    private TMP_Text _storageLBL;
    [SerializeField]
    private TMP_Text _completedTxt;
    private readonly Color _fireBrick = new Color(0.6953125f, 0.1328125f, 0.1328125f);
    private readonly Color _cyan = new Color(0.13671875f, 0.7421875f, 0.8046875f);
    private readonly Color _green = new Color(0.0703125f, 0.92578125f, 0.08203125f);
    private readonly Color _orange = new Color(0.95703125f, 0.4609375f, 0f);
    private const float MaxBar = IonCubeGeneratorController.ProgressComplete;
    private const float BarMinValue = 0.087f;
    private const float BarMaxValue = 0.409f;
    private const int MaxContainerSpaces = CubeGeneratorContainer.MaxAvailableSpaces;
    private const float DelayedStartTime = 0.5f;
    private const float RepeatingUpdateInterval = 1f;

    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;

    public void Initialize(object obj)
    {
        FindAllComponents();

        _controller = (IonCubeGeneratorController)obj;

        _initialized = true;

        //Set data before screen is shown.
        UpdateDisplay();

        //Update the speed text. I dont want this to be updates every second
        UpdateSpeedText();

        Show();
    }

    private void Awake()
    {
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
            _deviceName.text = _controller.GetDeviceName();
        }
        else
        {
            _deviceName.text = $"{_controller.GetDeviceName()} [{_controller.UnitID}]";
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

    public override void OnSettingsButtonClicked()
    {
        onSettingsClicked?.Invoke(_controller);
    }

    public override void OnBackButtonClicked()
    {
        onBackClicked?.Invoke(PDAPages.None);
    }

    public override void OnInfoButtonClicked()
    {
        FCSPDAController.Main.GetGUI().OnInfoButtonClicked?.Invoke(IonCubeGeneratorBuildable.PatchedTechType);
    }

    public void OnStorageButtonClicked()
    {
        _controller.OpenStorage();
    }

    public void ProcessSpeedChange(string buttonID)
    {
        switch (buttonID)
        {
            case "LButton":
                switch (_controller.CurrentSpeedMode)
                {
                    case IonCubeGenSpeedModes.Max:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.High;
                        break;
                    case IonCubeGenSpeedModes.High:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Low;
                        break;
                    case IonCubeGenSpeedModes.Low:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Min;
                        break;
                    case IonCubeGenSpeedModes.Min:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Off;
                        break;
                }
                break;

            case "RButton":
                switch (_controller.CurrentSpeedMode)
                {
                    case IonCubeGenSpeedModes.High:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Max;
                        break;
                    case IonCubeGenSpeedModes.Low:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.High;
                        break;
                    case IonCubeGenSpeedModes.Min:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Low;
                        break;
                    case IonCubeGenSpeedModes.Off:
                        _controller.CurrentSpeedMode = IonCubeGenSpeedModes.Min;
                        break;
                }
                break;
            default:
                QuickLogger.Debug(_controller.CurrentSpeedMode.ToString(), true);
                throw new ArgumentOutOfRangeException();
        }

        UpdateSpeedText();
    }

    private void UpdateSpeedText()
    {
        switch (_controller.CurrentSpeedMode)
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
        _percentDisplay.text = $"{Mathf.RoundToInt(_controller.GenerationPercent * 100)}%";
    }

    private void UpdatePercentageBar()
    {
        if (_controller == null)
        {
            QuickLogger.Error("Mono is null");
            return;
        }

        //float calcBar = _mono.GenerationPercent / MaxBar;

        float outputBar = _controller.GenerationPercent * (BarMaxValue - BarMinValue) + BarMinValue;

        _percentageBar.fillAmount = Mathf.Clamp(outputBar, BarMinValue, BarMaxValue);

    }

    private void UpdateStoragePercentBar()
    {
        float calcBar = (float)(_controller.NumberOfCubes * 1.0 / (MaxContainerSpaces * 1.0));
        float outputBar = calcBar * (BarMaxValue - BarMinValue) + BarMinValue;
        _storageBar.fillAmount = Mathf.Clamp(outputBar, BarMinValue, BarMaxValue);

    }

    private void UpdateStorageAmount()
    {
        _storageAmount.text = $"{_controller.NumberOfCubes}/{MaxContainerSpaces}";

        float percent = (float)(_controller.NumberOfCubes * 1.0 / MaxContainerSpaces * 1.0) * 100.0f;

        if (Math.Round(percent) <= 25)
        {
            _storageBar.color = _cyan;
        }

        if (Math.Round(percent) > 25 && percent <= 50)
        {
            _storageBar.color = _green;
        }

        if (Math.Round(percent) > 50 && percent <= 75)
        {
            _storageBar.color = _orange;
        }

        if (Math.Round(percent) > 75 && percent <= 100)
        {
            _storageBar.color = _fireBrick;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}