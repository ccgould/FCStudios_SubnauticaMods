using AMMiniMedBay.Buildable;
using AMMiniMedBay.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AMMiniMedBay.Configuration;
using FCSCommon.Extensions;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace AMMiniMedBay.Display
{
    /// <summary>
    /// A component that controls the screens UI input functions
    /// </summary>
    internal class AMMiniMedBayDisplay : MonoBehaviour
    {
        #region Private Members

        private GameObject _canvasGameObject;
        private bool _coroutineStarted;
        private bool _initialized;
        private const float DelayedStartTime = 0.5f;
        private const float RepeatingUpdateInterval = 1f;
        private const int Main = 1;
        private const int NonPower = 2;
        private const int ColorPicker = 3;
        private const int BlackOut = 0;
        private List<ColorVec4> _serializedColors;
        private int COLORS_PER_PAGE = 72;
        private int _maxColorPage = 1;
        private int _currentColorPage = 1;
        private Text _colorPageNumber;
        private GameObject _colorPageContainer;
        private AMMiniMedBayController _mono;
        private AMMiniMedBayAnimationManager _animatorController;
        private GameObject _healButton;
        private Text _storageTxt;
        private int _currentPage = 1;
        private Text _healthPercentage;
        private GameObject _n2Status;

        #endregion

        #region Public Properties
        public bool ShowBootScreen { get; set; } = true;
        public int BootTime { get; set; } = 3;
        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        internal void Setup(AMMiniMedBayController mono)
        {
            if (!_coroutineStarted)
                InvokeRepeating(nameof(UpdateDisplay), DelayedStartTime * 3f, RepeatingUpdateInterval);
            _mono = mono;
            _animatorController = mono.AnimationManager;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }

            _mono.PowerManager.OnPowerOutage += OnPowerOutage;
            _mono.PowerManager.OnPowerResume += OnPowerResume;

            _serializedColors = ColorList.Colors;

            if (_serializedColors.Count < 1)
            {
                QuickLogger.Error($"Serialized Colors is empty.", true);
            }

            CheckCurrentPage();

            DrawColorPage(1);

            _initialized = true;
        }

        private void OnPowerResume()
        {
            StartCoroutine(RestorePageEnu());
        }

        private void OnPowerOutage()
        {
            var screen = _mono.AnimationManager.GetIntHash(_mono.PageHash);

            if (screen != NonPower)
            {
                _currentPage = screen;
            }

            StartCoroutine(NoPowerScreenEnu());
        }

        #endregion

        #region Internal Methods

        internal void OnButtonClick(string btnName, object additionalObject)
        {
            switch (btnName)
            {
                case "HealBTN":
                    _currentPage = _mono.AnimationManager.GetIntHash(_mono.PageHash);
                    _mono.HealPlayer();
                    break;
                case "HomeBTN":
                    StartCoroutine(BootScreenEnu());
                    break;
                case "CPBTN":
                    StartCoroutine(ColorPickerScreenEnu());
                    break;
                case "StorageBTN":
                    if (Player.main == null) return;
                    QuickLogger.Debug($"Clicked on Storage Container", true);
                    _mono.Container.OpenStorage();
                    break;
                case "ColorItem":
                    var color = (Color)additionalObject;
                    MaterialHelpers.ChangeMaterialColor("AMMiniMedBay_BaseColor", _mono.gameObject, color);
                    QuickLogger.Debug($"{_mono.gameObject.name} Color Changed to {color.ToString()}");
                    _mono.SetCurrentBodyColor(color);
                    break;
            }
        }

        internal void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
        }

        #endregion

        #region Private Methods

        private bool FindAllComponents()
        {
            #region Canvas
            _canvasGameObject = this.gameObject.GetComponentInChildren<Canvas>()?.gameObject;
            if (_canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }
            #endregion

            #region HandTarget

            var storage = _canvasGameObject.FindChild("HandTarget");

            if (storage == null)
            {
                QuickLogger.Error("Storage not found.");
                return false;
            }


            var storageTarget = storage.AddComponent<InterfaceButton>();
            storageTarget.BtnName = "StorageBTN";
            storageTarget.ButtonMode = InterfaceButtonMode.None;
            storageTarget.OnButtonClick = OnButtonClick;
            storageTarget.TextLineOne = GetLanguage(AMMiniMedBayBuildable.OpenStorageKey);
            storageTarget.Container = _mono.Container;
            #endregion

            #region MainPage

            var mainPage = _canvasGameObject.FindChild("Main")?.gameObject;

            if (mainPage == null)
            {
                QuickLogger.Error("Main Page not found.");
                return false;
            }
            #endregion

            #region Color Picker Page

            var colorPickerPage = _canvasGameObject.FindChild("ColorPicker")?.gameObject;

            if (colorPickerPage == null)
            {
                QuickLogger.Error("Color Picker Page not found.");
                return false;
            }
            #endregion

            #region Power Page

            var powerPage = _canvasGameObject.FindChild("NoPower")?.gameObject;

            if (powerPage == null)
            {
                QuickLogger.Error("Power Page not found.");
                return false;
            }
            #endregion

            #region Storage Text

            _storageTxt = _canvasGameObject.FindChild("StorageTXT").GetComponent<Text>();

            if (_storageTxt == null)
            {
                QuickLogger.Error("Storage Text not found.");
                return false;
            }
            #endregion

            #region Heal Button

            _healButton = mainPage.FindChild("HealBTN")?.gameObject;

            if (_healButton == null)
            {
                QuickLogger.Error("Heal button not found.");
                return false;
            }

            var healBtn = _healButton.AddComponent<InterfaceButton>();
            healBtn.BtnName = "HealBTN";
            healBtn.ButtonMode = InterfaceButtonMode.Background;
            healBtn.OnButtonClick = OnButtonClick;
            healBtn.TextLineOne = GetLanguage(AMMiniMedBayBuildable.HealKey);
            #endregion

            #region Home Button

            var homeButton = colorPickerPage.FindChild("Image")?.gameObject;

            if (homeButton == null)
            {
                QuickLogger.Error("Home Button Not Found not found.");
                return false;
            }

            var homeBtn = homeButton.AddComponent<InterfaceButton>();
            homeBtn.BtnName = "HomeBTN";
            homeBtn.ButtonMode = InterfaceButtonMode.Background;
            homeBtn.OnButtonClick = OnButtonClick;
            homeBtn.TextLineOne = GetLanguage(AMMiniMedBayBuildable.HomeKey);
            #endregion

            #region Color Picker Button

            var cPButton = mainPage.FindChild("Paint_Spray")?.gameObject;

            if (cPButton == null)
            {
                QuickLogger.Error("Color Picker Button not found.");
                return false;
            }

            var cpBtn = cPButton.AddComponent<InterfaceButton>();
            cpBtn.BtnName = "CPBTN";
            cpBtn.ButtonMode = InterfaceButtonMode.Background;
            cpBtn.OnButtonClick = OnButtonClick;
            cpBtn.TextLineOne = GetLanguage(AMMiniMedBayBuildable.ColorPickerKey);
            #endregion

            #region L Nav

            var lNavButton = colorPickerPage.FindChild("L_Nav")?.gameObject;

            if (lNavButton == null)
            {
                QuickLogger.Error("LNav button not found.");
                return false;
            }

            var lNavBtn = lNavButton.AddComponent<PaginatorButton>();
            lNavBtn.AmountToChangePageBy = -1;
            lNavBtn.ChangePageBy = ChangeColorPageBy;
            lNavBtn.HoverTextLineOne = GetLanguage(AMMiniMedBayBuildable.OnHoverLPaginatorKey);
            #endregion

            #region R Nav

            var rNavButton = colorPickerPage.FindChild("R_Nav")?.gameObject;

            if (rNavButton == null)
            {
                QuickLogger.Error("RNav button not found.");
                return false;
            }

            var rNavBtn = rNavButton.AddComponent<PaginatorButton>();
            rNavBtn.AmountToChangePageBy = 1;
            rNavBtn.ChangePageBy = ChangeColorPageBy;
            rNavBtn.HoverTextLineOne = GetLanguage(AMMiniMedBayBuildable.OnHoverRPaginatorKey);
            #endregion

            #region Page Number

            _colorPageNumber = colorPickerPage.FindChild("Text").GetComponent<Text>();
            if (_colorPageNumber == null)
            {
                QuickLogger.Error("Page Number not found");
                return false;
            }
            #endregion

            #region Color Container

            _colorPageContainer = colorPickerPage.FindChild("Grid")?.gameObject;
            if (_colorPageContainer == null)
            {
                QuickLogger.Error("Color Grid not found");
                return false;
            }
            #endregion

            #region Health Staus Text

            var health_LBL = mainPage.FindChild("Health_LBL").GetComponent<Text>();

            if (health_LBL == null)
            {
                QuickLogger.Error("Health Status label not found.");
                return false;
            }

            health_LBL.text = GetLanguage(AMMiniMedBayBuildable.HealthStatusLBLKey);
            #endregion

            #region N2 Status Text

            _n2Status = mainPage.FindChild("N2TXT")?.gameObject;

            if (_n2Status == null)
            {
                QuickLogger.Error("N2 Status label not found.");
                return false;
            }

            #endregion

            #region Health Text

            _healthPercentage = mainPage.FindChild("PlayerHealthTXT").GetComponent<Text>();

            if (_healthPercentage == null)
            {
                QuickLogger.Error("Player Health TXT label not found.");
                return false;
            }

            #endregion

            #region No Power Text

            var noPowerText = powerPage.FindChild("NoPowerTxt").GetComponent<Text>();

            if (noPowerText == null)
            {
                QuickLogger.Error("No Power TXT label not found.");
                return false;
            }

            noPowerText.text = GetLanguage(AMMiniMedBayBuildable.NoPowerKey);

            #endregion

            #region No Power Message 

            var noPowerMessageText = powerPage.FindChild("MessageTXT").GetComponent<Text>();

            if (noPowerMessageText == null)
            {
                QuickLogger.Error("No Power Message TXT label not found.");
                return false;
            }

            noPowerMessageText.text = GetLanguage(AMMiniMedBayBuildable.NoPowerMessage);

            #endregion

            return true;
        }

        private string GetLanguage(string key)
        {
            return Language.main.Get(key);
        }

        private void UpdateDisplay()
        {
            if (!_initialized)
                return;

            _coroutineStarted = true;
        }

        private void BootScreen()
        {
            StartCoroutine(BootScreenEnu());
        }

        private void ColorPickerScreen()
        {
            StartCoroutine(ColorPickerScreenEnu());
        }

        private void MainScreen()
        {
            StartCoroutine(MainScreenEnu());
        }

        private void CheckCurrentPage()
        {
            if (_animatorController.GetIntHash(_mono.PageHash) == BlackOut)
            {
                _animatorController.SetIntHash(_mono.PageHash, Main);
            }
        }

        private void OnTimerChanged(string obj)
        {
            //_timeLeftTXT.text = $"{GetLanguage(DisplayLanguagePatching.TimeLeftKey)} {obj}";
        }

        private void CalculateNewMaxColorPages()
        {
            _maxColorPage = Mathf.CeilToInt((_serializedColors.Count - 1) / COLORS_PER_PAGE) + 1;
            if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }
        }

        private void DrawColorPage(int page)
        {
            _currentColorPage = page;

            if (_currentColorPage <= 0)
            {
                _currentColorPage = 1;
            }
            else if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }

            int startingPosition = (_currentColorPage - 1) * COLORS_PER_PAGE;
            int endingPosition = startingPosition + COLORS_PER_PAGE;
            
            if (endingPosition > _serializedColors.Count)
            {
                endingPosition = _serializedColors.Count;
            }

            ClearColorPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var colorID = _serializedColors.ElementAt(i);
                LoadColorPicker(colorID);
            }

            UpdateColorPaginator();
        }

        private void LoadColorPicker(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(AMMiniMedBayBuildable.ColorItemPrefab);
            itemDisplay.transform.SetParent(_colorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.Vector4ToColor();
        }

        private void UpdateColorPaginator()
        {
            CalculateNewMaxColorPages();
            _colorPageNumber.text = $"{_currentColorPage.ToString()} | {_maxColorPage}";
        }

        private void ClearColorPage()
        {
            for (int i = 0; i < _colorPageContainer.transform.childCount; i++)
            {
                Destroy(_colorPageContainer.transform.GetChild(i).gameObject);
            }
        }
        #endregion

        #region IEnumerators
        private IEnumerator MainScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _animatorController.SetIntHash(_mono.PageHash, Main);
        }

        private IEnumerator BootScreenEnu()
        {
            yield return new WaitForEndOfFrame();

            if (ShowBootScreen)
            {
                _animatorController.SetIntHash(_mono.PageHash, Main);
                yield return new WaitForSeconds(BootTime);
            }

            MainScreen();
        }

        private IEnumerator ColorPickerScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _animatorController.SetIntHash(_mono.PageHash, ColorPicker);
        }

        private IEnumerator NoPowerScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _animatorController.SetIntHash(_mono.PageHash, NonPower);
        }
        private IEnumerator RestorePageEnu()
        {
            yield return new WaitForEndOfFrame();
            QuickLogger.Debug($"Prev Page = {_currentPage}");
            _animatorController.SetIntHash(_mono.PageHash, _currentPage);
        }

        #endregion

        internal void ChangeStorageAmount(int value)
        {
            _storageTxt.text = value.ToString();
            QuickLogger.Debug($"Storage Number {value}", true);
        }

        internal void UpdatePlayerHealthPercent(int value)
        {
            if(_healthPercentage == null) return;
            _healthPercentage.text = $"{value}%";
        }

        internal void Destroy()
        {
            if (_mono != null)
            {
                _mono.PowerManager.OnPowerOutage -= OnPowerOutage;
                _mono.PowerManager.OnPowerResume -= OnPowerResume;
            }
        }

        public void UpdatePlayerNitrogen(float depth)
        {
            if (!_n2Status.activeSelf)
            {
                QuickLogger.Debug("Show Nitrogen Display", true);
                _n2Status.SetActive(true);
            }

            _n2Status.GetComponent<Text>().text = $"<size=96><B>N</B></size><size=  24>2</size><size= 96> {Mathf.CeilToInt(depth)} </size>";
        }

        public void HidePlayerNitrogen()
        {
            if (_n2Status.activeSelf)
            {
                QuickLogger.Debug("Hide Nitrogen Display", true);
                _n2Status.SetActive(false);
            }
        }
    }
}
