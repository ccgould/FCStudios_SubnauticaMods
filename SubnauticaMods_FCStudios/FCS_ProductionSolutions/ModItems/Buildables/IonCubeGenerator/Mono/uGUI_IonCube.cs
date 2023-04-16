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
    private GameObject _powerOffPage;
    private GameObject _operationPage;
    private GameObject _clocking;
    private TMP_Text _speedMode;
    private Button _lButton;
    private Button _rButton;
    private TMP_Text _percentDisplay;
    private Image _percentageBar;
    private Image _storageBar;
    private TMP_Text _storageAmount;
    private bool _initialized;
    private bool _coroutineStarted;
    private Text _deviceName;
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

        #region Clocking

        _clocking = GameObjectHelpers.FindGameObject(gameObject, "Clocking");

        if (_clocking == null)
        {
            QuickLogger.Error("Clocking not found.");
            return false;
        }
        #endregion

        #region SpeedMode

        GameObject speedMode = _clocking.FindChild("SpeedMode")?.gameObject;

        if (speedMode == null)
        {
            QuickLogger.Error("SpeedMode not found.");
            return false;
        }

        _speedMode = speedMode.GetComponent<TMP_Text>();
        #endregion

        #region LButton BTN

        _lButton = GameObjectHelpers.FindGameObject(gameObject, "LButton")?.GetComponent<Button>();

        if (_lButton == null)
        {
            QuickLogger.Error("LButton not found.");
            return false;
        }

        _lButton.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(_lButton);
            ProcessSpeedChange("LButton");
        });
        #endregion

        #region RButton BTN

        _rButton = GameObjectHelpers.FindGameObject(gameObject, "RButton")?.GetComponent<Button>();

        if (_rButton == null)
        {
            QuickLogger.Error("RButton not found.");
            return false;
        }

        _rButton.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(_rButton);
            ProcessSpeedChange("RButton");
        });
        #endregion

        #region Storage BTN

        var storage_BTN = GameObjectHelpers.FindGameObject(gameObject, "Storage_BTN")?.GetComponent<Button>();

        if (storage_BTN == null)
        {
            QuickLogger.Error("Storage Button Power Button not found.");
            return false;
        }

        storage_BTN.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(storage_BTN);
            _controller.OpenStorage();
        });

        #endregion

        #region Complete

        _percentDisplay = GameObjectHelpers.FindGameObject(gameObject, "Completed_Percent")?.GetComponent<TMP_Text>();

        if (_percentDisplay == null)
        {
            QuickLogger.Error("Complete not found.");
            return false;
        }
        #endregion

        #region Mask

        GameObject mask = GameObjectHelpers.FindGameObject(gameObject, "Mask")?.gameObject;

        if (mask == null)
        {
            QuickLogger.Error("Mask not found.");
            return false;
        }
        #endregion

        #region Mask2

        GameObject mask2 = GameObjectHelpers.FindGameObject(gameObject, "Mask (1)")?.gameObject;

        if (mask2 == null)
        {
            QuickLogger.Error("Mask (1) not found.");
            return false;
        }
        #endregion

        #region Full_Bar
        GameObject fullBar = mask.FindChild("Full_Bar")?.gameObject;

        if (fullBar == null)
        {
            QuickLogger.Error("Full_Bar not found.");
            return false;
        }

        _percentageBar = fullBar.GetComponent<Image>();
        #endregion

        #region StorageBar
        GameObject statusFullBar = mask2.FindChild("Full_Bar")?.gameObject;

        if (statusFullBar == null)
        {
            QuickLogger.Error("Full_Bar not found.");
            return false;
        }

        _storageBar = statusFullBar.GetComponent<Image>();
        #endregion

        #region Storage

        _storageAmount = GameObjectHelpers.FindGameObject(gameObject, "Storage_Amount")?.GetComponent<TMP_Text>();

        if (_storageAmount == null)
        {
            QuickLogger.Error("Storage UI was not found.");
            return false;
        }

        #endregion

        #region CompletedTXT

        GameObject completedTxt = GameObjectHelpers.FindGameObject(gameObject, "Completed_LBL")?.gameObject;

        if (completedTxt == null)
        {
            QuickLogger.Error("Completed_Txt was not found.");
            return false;
        }

        completedTxt.GetComponent<TMP_Text>().text = GetLanguage(DisplayLanguagePatching.CompletedKey);
        #endregion

        #region Storage_LBL

        GameObject storageLbl = GameObjectHelpers.FindGameObject(gameObject, "Storage_LBL")?.gameObject;

        if (storageLbl == null)
        {
            QuickLogger.Error("Storage_LBL was not found.");
            return false;
        }

        storageLbl.GetComponent<TMP_Text>().text = GetLanguage(DisplayLanguagePatching.StorageKey);
        #endregion

        #region Overclock_Txt

        GameObject overClocking = _clocking.FindChild("Overclock_LBL")?.gameObject;

        if (overClocking == null)
        {
            QuickLogger.Error("Overclock_Txt not found.");
            return false;
        }

        overClocking.GetComponent<TMP_Text>().text = GetLanguage(DisplayLanguagePatching.OverClockKey);
        #endregion

        var infoBTN = GameObjectHelpers.FindGameObject(gameObject, "InfoBTN").GetComponent<Button>();
        infoBTN.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(infoBTN);
            FCSPDAController.Main.GetGUI().OnInfoButtonClicked?.Invoke(IonCubeGeneratorBuildable.PatchedTechType);
        });

        _deviceName = GameObjectHelpers.FindGameObject(gameObject, "Label").GetComponent<Text>();

        var backButon = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
        backButon.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(backButon);
            onBackClicked?.Invoke(PDAPages.None);
        });

        var settingsButon = GameObjectHelpers.FindGameObject(gameObject, "SettingsBTN").GetComponent<Button>();
        settingsButon.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(settingsButon);
            onSettingsClicked?.Invoke(_controller);
        });

        return true;
    }

    private void ProcessSpeedChange(string buttonID)
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