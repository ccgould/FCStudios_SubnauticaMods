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
        private Text _timeLeftTXT;
        private GameObject _colorPickerPage;
        private const float DelayedStartTime = 0.5f;
        private const float RepeatingUpdateInterval = 1f;
        private const int Main = 1;
        private const int ColorPicker = 4;
        private const int BlackOut = 0;
        private List<SerializableColor> _serializedColors;
        private int COLORS_PER_PAGE = 48;
        private int _maxColorPage = 1;
        private int _currentColorPage = 1;
        private Text _colorPageBottomNumber;
        private Text _colorPageTopNumber;
        private GameObject _colorPageContainer;
        private AMMiniMedBayController _mono;
        private AMMiniMedBayAnimationManager _animatorController;
        private GameObject _healButton;

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
                base.InvokeRepeating(nameof(UpdateDisplay), DelayedStartTime * 3f, RepeatingUpdateInterval);
            _mono = mono;
            _animatorController = mono.AnimationManager;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }

            _serializedColors = JsonConvert.DeserializeObject<List<SerializableColor>>(
                File.ReadAllText(Path.Combine(AssetHelper.GetAssetFolder("FCS_AMMiniMedBay"), "colors.json")));

            if (_serializedColors.Count < 1)
            {
                QuickLogger.Error($"Serialized Colors is empty.", true);
            }

            CheckCurrentPage();

            DrawColorPage(1);

            _initialized = true;
        }

        #endregion

        #region Internal Methods

        internal void OnButtonClick(string btnName, object additionalObject)
        {
            switch (btnName)
            {
                case "HealBTN":
                    _mono.HealPlayer();
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

            #region MainPage

            var mainPage = _canvasGameObject.FindChild("Main")?.gameObject;

            if (mainPage == null)
            {
                QuickLogger.Error("Main Page not found.");
                return false;
            }
            #endregion

            #region StoragePage

            var storagePage = _canvasGameObject.FindChild("Storage")?.gameObject;

            if (storagePage == null)
            {
                QuickLogger.Error("Storage Page not found.");
                return false;
            }
            #endregion

            #region Heal Button

            _healButton = mainPage.FindChild("HealBTN")?.gameObject;

            if (_healButton == null)
            {
                QuickLogger.Error("Heal button not found.");
            }

            var healBtn = _healButton.AddComponent<InterfaceButton>();
            healBtn.BtnName = "HealBTN";
            healBtn.ButtonMode = InterfaceButtonMode.Background;
            healBtn.OnButtonClick = OnButtonClick;
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

            //QuickLogger.Debug($"_currentColorPage: {_currentColorPage} || startingPosition: {startingPosition} || endingPosition: {endingPosition} || COLORS_PER_PAGE {COLORS_PER_PAGE}", true);

            if (endingPosition > _serializedColors.Count)
            {
                endingPosition = _serializedColors.Count;
            }

            ClearColorPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                //QuickLogger.Debug($"Found: {_serializedColors.Count} colors || Element:{i}", true);
                var colorID = _serializedColors.ElementAt(i);
                LoadColorPicker(colorID);
            }

            UpdateColorPaginator();
        }

        private void LoadColorPicker(SerializableColor color)
        {
            GameObject itemDisplay = Instantiate(AMMiniMedBayBuildable.ColorItemPrefab);
            itemDisplay.transform.SetParent(_colorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.ToColor();
        }

        private void UpdateColorPaginator()
        {
            CalculateNewMaxColorPages();
            _colorPageTopNumber.text = _currentColorPage.ToString();
            _colorPageBottomNumber.text = _maxColorPage.ToString();
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
        #endregion
    }
}
