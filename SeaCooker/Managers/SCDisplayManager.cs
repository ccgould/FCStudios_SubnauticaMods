using AE.SeaCooker.Buildable;
using AE.SeaCooker.Configuration;
using AE.SeaCooker.Display;
using AE.SeaCooker.Helpers;
using AE.SeaCooker.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AE.SeaCooker.Managers
{
    internal class SCDisplayManager : AIDisplay
    {
        private SeaCookerController _mono;

        private int _showProcess;
        private int _isOnHomePage;
        private Color _startColor = new Color(0.16796875f, 0.16796875f, 0.16796875f);
        private Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private InterfaceButton _startButton;
        private GameObject _grid;
        private GameObject _fromImage;
        private GameObject _toImage;
        private Image _percentage;
        private Text _fuelPercentage;
        private ColorPageHelper _colorPage;
        private Text _paginator;
        private CustomToggle _cusToggle;


        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "startBTN":
                    if (_mono.FoodManager.IsCooking())
                    {
                        _mono.FoodManager.KillCooking();
                    }
                    else
                    {
                        _mono.StorageManager.CookStoredFood();
                    }

                    UpdateCookingButton();

                    break;
                case "colorPickerBTN":
                    UpdateColorPage();
                    break;
                case "homeBTN":
                    UpdateColorPage();
                    break;
                case "openInputBTN":
                    _mono.StorageManager.OpenInputStorage();
                    break;
                case "openExportBTN":
                    _mono.StorageManager.OpenExportStorage();
                    break;
                case "fuelTankBTN":
                    _mono.GasManager.OpenFuelTank();
                    break;
                case "seaBreezeToggle":
                    var item = (CustomToggle)tag;
                    _mono.StorageManager.SetExportToSeabreeze(item.CheckState());
                    QuickLogger.Debug($"Toggle State: {item.CheckState()}", true);
                    break;
                case "ColorItem":
                    var color = (Color)tag;
                    QuickLogger.Debug($"{_mono.gameObject.name} Color Changed to {color.ToString()}", true);
                    _mono.ColorManager.SetCurrentBodyColor(color);
                    break;

            }
        }

        internal void UpdateCookingButton()
        {
            _startButton.ChangeText(_mono.FoodManager.IsCooking() ? SeaCookerBuildable.Cancel() : SeaCookerBuildable.Start());
        }

        private void UpdateColorPage()
        {
            _mono.AnimationManager.SetBoolHash(_isOnHomePage, !_mono.AnimationManager.GetBoolHash(_isOnHomePage));
        }

        public override bool FindAllComponents()
        {
            #region Canvas  
            var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (canvasGameObject == null)
            {
                QuickLogger.Error("Canvas cannot be found");
                return false;
            }
            #endregion

            #region Home

            var home = canvasGameObject.FindChild("Home")?.gameObject;

            if (home == null)
            {
                QuickLogger.Error("Unable to find Home GameObject");
                return false;
            }
            #endregion

            #region ColorPicker

            var colorPicker = canvasGameObject.FindChild("ColorPicker")?.gameObject;

            if (colorPicker == null)
            {
                QuickLogger.Error("Unable to find colorPicker GameObject");
                return false;
            }
            #endregion

            var doorsResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Doors", out var doorsOutput);

            if (!doorsResult)
            {
                return false;
            }

            var doors = doorsOutput;

            var startButtonResult = InterfaceHelpers.CreateButton(home, "Button", "startBTN", InterfaceButtonMode.Background,
                _startColor, _hoverColor, OnButtonClick, out var startButton);
            startButton.TextLineOne = "Start Cooking";

            if (!startButtonResult)
            {
                return false;
            }
            _startButton = startButton;

            var colorPickerResult = InterfaceHelpers.CreateButton(home, "Paint_BTN", "colorPickerBTN", InterfaceButtonMode.Background,
                OnButtonClick, out var colorPickerButton);
            colorPickerButton.TextLineOne = "Color Picker Page";

            if (!colorPickerResult)
            {
                return false;
            }

            var fuelTankResult = InterfaceHelpers.CreateButton(home, "Tank_BTN", "fuelTankBTN", InterfaceButtonMode.Background,
                OnButtonClick, out var fuelTankButton);
            fuelTankButton.TextLineOne = "Open Fuel Tank";

            if (!fuelTankResult)
            {
                return false;
            }

            var homeBTN = InterfaceHelpers.CreateButton(colorPicker, "Home_BTN", "homeBTN", InterfaceButtonMode.TextColor,
                OnButtonClick, out var homeButton);
            homeButton.TextLineOne = "Home Page";
            if (!homeBTN)
            {
                return false;
            }

            var openInputBTN = InterfaceHelpers.CreateButton(doors, "Open_Input", "openInputBTN", InterfaceButtonMode.TextColor,
                OnButtonClick, out var openInputButton);
            openInputButton.TextLineOne = "Open Input Container";

            if (!openInputBTN)
            {
                return false;
            }

            var openExportBTN = InterfaceHelpers.CreateButton(doors, "Open_Export", "openExportBTN", InterfaceButtonMode.TextColor,
                OnButtonClick, out var openExportButton);
            openExportButton.TextLineOne = "Open Export Container";

            if (!openExportBTN)
            {
                return false;
            }

            var nextBTN = InterfaceHelpers.CreatePaginator(colorPicker, "NextPage", 1, _colorPage.ChangeColorPageBy, out var nextButton);
            nextButton.TextLineOne = "Next Page";

            if (!nextBTN)
            {
                return false;
            }

            var prevBTN = InterfaceHelpers.CreatePaginator(colorPicker, "PrevPage", -1, _colorPage.ChangeColorPageBy, out var prevButton);
            prevButton.TextLineOne = "Previous Page";

            if (!prevBTN)
            {
                return false;
            }

            var gridResult = InterfaceHelpers.FindGameObject(colorPicker, "Grid", out var grid);

            if (!gridResult)
            {
                return false;
            }
            _grid = grid;

            var fromImage = InterfaceHelpers.FindGameObject(home, "from_Image", out var from_Image);

            if (!fromImage)
            {
                return false;
            }
            _fromImage = from_Image;
            uGUI_Icon fromIcon = _fromImage.gameObject.AddComponent<uGUI_Icon>();

            var toImage = InterfaceHelpers.FindGameObject(home, "to_Image", out var to_Image);

            if (!toImage)
            {
                return false;
            }
            _toImage = to_Image;
            uGUI_Icon toIcon = _toImage.gameObject.AddComponent<uGUI_Icon>();

            var percentageResult = InterfaceHelpers.FindGameObject(home, "Preloader_Bar", out var percentage);

            if (!percentageResult)
            {
                return false;
            }
            _percentage = percentage.GetComponent<Image>();

            var fuelResult = InterfaceHelpers.FindGameObject(home, "FuelPercentage", out var fuelPercentage);

            if (!fuelResult)
            {
                return false;
            }
            _fuelPercentage = fuelPercentage.GetComponent<Text>();
            _fuelPercentage.text = $"{SeaCookerBuildable.TankPercentage()}: (0%)";


            var versionResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Version", out var version);

            if (!versionResult)
            {
                return false;
            }
            var versionLbl = version.GetComponent<Text>();
            versionLbl.text = $"{SeaCookerBuildable.Version()}: {QPatch.Version}";

            var paginatorResult = InterfaceHelpers.FindGameObject(colorPicker, "Paginator", out var paginator);

            if (!paginatorResult)
            {
                return false;
            }
            _paginator = paginator.GetComponent<Text>();


            var toggleResult = InterfaceHelpers.FindGameObject(home, "Toggle", out var toggle);

            if (!toggleResult)
            {
                return false;
            }

            _cusToggle = toggle.AddComponent<CustomToggle>();
            _cusToggle.BtnName = "seaBreezeToggle";
            _cusToggle.ButtonMode = InterfaceButtonMode.Background;
            _cusToggle.OnButtonClick = OnButtonClick;
            _cusToggle.Tag = _cusToggle;


            return true;
        }

        public override IEnumerator PowerOn()
        {
            _mono.AnimationManager.SetBoolHash(_isOnHomePage, true);
            yield return null;
        }

        public override IEnumerator CompleteSetup()
        {
            StartCoroutine(PowerOn());
            yield return null;
        }

        public void Setup(SeaCookerController mono)
        {
            _mono = mono;

            _showProcess = Animator.StringToHash("ShowProcess");
            _isOnHomePage = Animator.StringToHash("IsOnHomePage");

            _mono.FoodManager.OnFoodCookedAll += OnFoodCooked;
            _mono.FoodManager.OnCookingStart += OnCookingStart;
            _mono.GasManager.OnGasUpdate += OnGasRemoved;

            _colorPage = gameObject.AddComponent<ColorPageHelper>();

            if (FindAllComponents())
            {
                StartCoroutine(CompleteSetup());
            }
            else
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }

            _colorPage.OnButtonClick = OnButtonClick;
            _colorPage.SerializedColors = Mod.SerializedColors();
            _colorPage.ColorsPerPage = 42;
            _colorPage.ColorItemPrefab = SeaCookerBuildable.ColorItemPrefab;
            _colorPage.ColorPageContainer = _grid;
            _colorPage.ColorPageNumber = _paginator;
            _colorPage.Initialize();
            StartCoroutine(CompleteSetup());

            InvokeRepeating(nameof(UpdateScreen), 0, 0.5f);
        }

        private void OnGasRemoved()
        {
            QuickLogger.Debug($"Updating Gas {_mono.GasManager.GetTankPercentage()}");
            UpdateFuelPercentage();
        }

        internal void UpdateFuelPercentage()
        {
            _fuelPercentage.text = $"{SeaCookerBuildable.TankPercentage()}: ({_mono.GasManager.GetTankPercentage()}%)";
        }

        private void OnCookingStart(TechType raw, TechType cooked)
        {
            QuickLogger.Debug("Starting Processing Animation", true);

            //ToggleProcessDisplay();
            _mono.UpdateIsRunning();
            uGUI_Icon fromIcon = _fromImage.gameObject.GetComponent<uGUI_Icon>();
            fromIcon.sprite = SpriteManager.Get(raw);

            uGUI_Icon toIcon = _toImage.gameObject.GetComponent<uGUI_Icon>();
            toIcon.sprite = SpriteManager.Get(cooked);

            UpdateCookingButton();
        }

        private void OnFoodCooked(TechType raw, List<TechType> techTypes)
        {
            UpdateCookingButton();
        }

        private void UpdateScreen()
        {
            //_oxPreloaderBar.fillAmount = _mono.OxygenManager.GetO2LevelPercentage();
            //_oxPreloaderLBL.text = $"{Mathf.RoundToInt(_mono.OxygenManager.GetO2LevelPercentageFull())}%";
            //_healthPreloaderBar.fillAmount = _mono.HealthManager.GetHealthPercentage();
            //_healthPreloaderlbl.text = $"{Mathf.RoundToInt(_mono.HealthManager.GetHealthPercentageFull())}%";
            //_powerUsage.text = $"{OxStationBuildable.PowerUsage()}: <color=#ff0000ff>{_mono.PowerManager.GetPowerUsage()}</color> {OxStationBuildable.PerMinute()}.";
        }

        public override void DrawPage(int page)
        {
            throw new NotImplementedException();
        }

        public override void ClearPage()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        private void UpdateCookingInfo()
        {

        }

        public override void ItemModified(TechType item, int newAmount = 0)
        {
            throw new NotImplementedException();
        }

        public void ToggleProcessDisplay(bool value = true)
        {
            _mono.AnimationManager.SetBoolHash(_showProcess, value);
        }

        public void UpdatePercentage(float percent)
        {
            _percentage.fillAmount = percent;
        }

        internal void ResetProgressBar()
        {
            _percentage.fillAmount = 0f;
        }

        internal void SetSendToSeaBreeze(bool value)
        {
            _cusToggle.SetToggleState(value);
        }
    }
}
