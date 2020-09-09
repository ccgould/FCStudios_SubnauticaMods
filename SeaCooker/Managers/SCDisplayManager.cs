using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSTechFabricator.Components;
using FCSTechFabricator.Managers;
using UnityEngine;
using UnityEngine.UI;
using FCSController = FCSTechFabricator.Abstract.FCSController;
using PaginatorButton = AE.SeaCooker.Display.PaginatorButton;

namespace AE.SeaCooker.Managers
{
    internal class SCDisplayManager : AIDisplay
    {
        private SeaCookerController _mono;

        private int _showProcess;
        private int _page;
        private Color _startColor = new Color(0.16796875f, 0.16796875f, 0.16796875f);
        private Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private InterfaceButton _startButton;
        private GameObject _colorGrid;
        private GameObject _fromImage;
        private GameObject _toImage;
        private Image _percentage;
        private Text _fuelPercentage;
        private ColorManager _colorPage;
        private Text _paginator;
        private CustomToggle _cusToggle;
        private GridHelper _seaBreezeGrid;
        private CustomToggle _autoToggle;
        private List<CustomToggle> _sbList = new List<CustomToggle>();

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

                case "HomeBTN":
                    UpdatePage(1);
                    break;

                case "settingsBTN":
                    UpdatePage(2);
                    UpdateSeaBreezes();
                    UpdateCheckedSB();
                    break;

                case "colorPickerBTN":
                    UpdatePage(3);
                    break;

                case "openInputBTN":
                    _mono.StorageManager.OpenInputStorage();
                    break;

                case "openExportBTN":
                    _mono.StorageManager.OpenExportStorage();
                    break;

                case "seaBreezeToggle":
                    var item = (CustomToggle)tag;
                    _mono.StorageManager.SetExportToSeabreeze(item.CheckState());
                    QuickLogger.Debug($"Toggle State: {item.CheckState()}", true);
                    break;

                case "autoToggle":
                    var autoItem = (CustomToggle)tag;
                    var state = autoItem.CheckState();
                    _mono.AutoChooseSeabreeze = state;
                    
                    if (state)
                    {
                        _mono.SelectedSeaBreezeID = String.Empty;
                        UpdateCheckedSB();
                    }

                    QuickLogger.Debug($"Toggle State: {autoItem.CheckState()}", true);
                    break;

                case "ColorItem":
                    var color = (Color)tag;
                    QuickLogger.Debug($"{_mono.gameObject.name} Color Changed to {color.ToString()}", true);
                    _mono.ColorManager.ChangeColor(color);
                    break;

                case "SeaBreezeItem":
                    var seaBreeze = (Color)tag;
                    _mono.ColorManager.ChangeColor(seaBreeze);
                    break;

                case "SeaBreeze":
                    var sb = (FCSConnectableDevice)tag;
                    _mono.SelectedSeaBreezeID = sb.GetPrefabIDString();
                    UpdateCheckedSB();
                    break;
            }
        }

        internal void UpdateCheckedSB()
        {
            _mono.IsSebreezeSelected = false;
            
            foreach (CustomToggle toggle in _sbList)
            {
                var sb =(FCSConnectableDevice)toggle.Tag;

                if (_mono.SelectedSeaBreezeID == sb.GetPrefabIDString())
                {
                    toggle.SetToggleState(true);
                    _mono.SetCurrentSeaBreeze(sb);
                    _mono.IsSebreezeSelected = true;
                    _autoToggle.SetToggleState(false);
                    _mono.AutoChooseSeabreeze = false;
                }
                else
                {
                    toggle.SetToggleState(false);
                }
            }
        }
        
        internal void UpdateCookingButton()
        {
            _startButton.ChangeText(_mono.FoodManager.IsCooking() ? SeaCookerBuildable.Cancel() : SeaCookerBuildable.Start());
        }

        private void UpdatePage(int page)
        {
            _mono.AnimationManager.SetIntHash(_page, page);
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

            #region Settings

            var settings = canvasGameObject.FindChild("Settings")?.gameObject;

            if (settings == null)
            {
                QuickLogger.Error("Unable to find Settings GameObject");
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

            #region Doors
            var doorsResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Doors", out var doorsOutput);

            if (!doorsResult)
            {
                return false;
            }

            var doors = doorsOutput;
            #endregion

            #region Start Button
            var startButtonResult = InterfaceHelpers.CreateButton(home, "Button", "startBTN", InterfaceButtonMode.Background,
        _startColor, _hoverColor, OnButtonClick, out var startButton);
            startButton.TextLineOne = "Start Cooking";

            if (!startButtonResult)
            {
                return false;
            }
            _startButton = startButton;
            #endregion

            #region Color Picker
            var colorPickerResult = InterfaceHelpers.CreateButton(settings, "Paint_BTN", "colorPickerBTN", InterfaceButtonMode.Background,
        OnButtonClick, out var colorPickerButton);
            colorPickerButton.TextLineOne = "Color Picker Page";

            if (!colorPickerResult)
            {
                return false;
            }
            #endregion

            #region Settings BTN
            var settingsResult = InterfaceHelpers.CreateButton(home, "Settings", "settingsBTN", InterfaceButtonMode.Background,
                OnButtonClick, out var settingsButton);
            settingsButton.TextLineOne = SeaCookerBuildable.GoToSettingsPage();

            if (!settingsResult)
            {
                return false;
            }
            #endregion
            
            #region Settings Color BTN
            var settingsCResult = InterfaceHelpers.CreateButton(colorPicker, "Home_BTN", "settingsBTN", InterfaceButtonMode.TextColor,
                OnButtonClick, out var settings_C_BTN);
            settings_C_BTN.ChangeText($"< {SeaCookerBuildable.SettingsPage()}");
            settings_C_BTN.TextLineOne = $"{SeaCookerBuildable.GoToSettingsPage()}";

            if (!settingsCResult)
            {
                QuickLogger.Error($"Can't find settingsBTN");
                return false;
            }
            #endregion

            #region Open Input BTN
            var openInputBTN = InterfaceHelpers.CreateButton(doors, "Open_Input", "openInputBTN", InterfaceButtonMode.TextColor,
        OnButtonClick, out var openInputButton);
            openInputButton.TextLineOne = "Open Input Container";

            if (!openInputBTN)
            {
                return false;
            }
            #endregion

            #region Open Export BTN
            var openExportBTN = InterfaceHelpers.CreateButton(doors, "Open_Export", "openExportBTN", InterfaceButtonMode.TextColor,
        OnButtonClick, out var openExportButton);
            openExportButton.TextLineOne = "Open Export Container";

            if (!openExportBTN)
            {
                return false;
            }
            #endregion

            #region Next BTN
            var nextBTN = InterfaceHelpers.CreatePaginator(colorPicker, "NextPage", 1, _colorPage.ChangeColorPageBy, out var nextButton);
            nextButton.TextLineOne = "Next Page";

            if (!nextBTN)
            {
                return false;
            }
            #endregion

            #region Prev BTN
            var prevBTN = InterfaceHelpers.CreatePaginator(colorPicker, "PrevPage", -1, _colorPage.ChangeColorPageBy, out var prevButton);
            prevButton.TextLineOne = "Previous Page";

            if (!prevBTN)
            {
                return false;
            }
            #endregion

            #region Color Grid
            var colorGridResult = InterfaceHelpers.FindGameObject(colorPicker, "Grid", out var colorGrid);

            if (!colorGridResult)
            {
                return false;
            }
            _colorGrid = colorGrid;
            #endregion
            
            #region SeaBreeze Grid
            var seaBreezeGridResult = InterfaceHelpers.FindGameObject(settings, "Grid", out var seaBreezeGrid);

            if (!seaBreezeGridResult)
            {
                return false;
            }

            _seaBreezeGrid.Setup(4,SeaCookerBuildable.SeaBreezeItemPrefab,settings,Color.white, new Color(0.07f, 0.38f, 0.7f, 1f), OnButtonClick);
            _seaBreezeGrid.OnLoadDisplay += OnLoadDisplay;
            #endregion
            
            #region From Image OMIT
            //var fromImage = InterfaceHelpers.FindGameObject(home, "from_Image", out var from_Image);

            //if (!fromImage)
            //{
            //    return false;
            //}
            //_fromImage = from_Image;
            //uGUI_Icon fromIcon = _fromImage.gameObject.AddComponent<uGUI_Icon>(); 
            #endregion

            #region To Image OMIT
            var toImage = InterfaceHelpers.FindGameObject(home, "to_Image", out var to_Image);

            if (!toImage)
            {
                return false;
            }
            _toImage = to_Image;
            uGUI_Icon toIcon = _toImage.gameObject.AddComponent<uGUI_Icon>();
            #endregion

            #region Percentage Bar
            var percentageResult = InterfaceHelpers.FindGameObject(home, "Preloader_Bar", out var percentage);

            if (!percentageResult)
            {
                return false;
            }
            _percentage = percentage.GetComponent<Image>();
            #endregion
            
            #region Version
            var versionResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Version", out var version);

            if (!versionResult)
            {
                return false;
            }
            var versionLbl = version.GetComponent<Text>();
            versionLbl.text = $"{SeaCookerBuildable.Version()}: {QPatch.Version}";
            #endregion

            #region Paginator
            var paginatorResult = InterfaceHelpers.FindGameObject(colorPicker, "Paginator", out var paginator);

            if (!paginatorResult)
            {
                return false;
            }
            _paginator = paginator.GetComponent<Text>();
            #endregion

            #region Seabreeze Toggle
            var toggleResult = InterfaceHelpers.FindGameObject(settings, "Toggle_SB_Export", out var toggle);

            if (!toggleResult)
            {
                QuickLogger.Error($"Cannot find Toggle_SB_Export on GameObject");
                return false;
            }

            _cusToggle = toggle.AddComponent<CustomToggle>();
            _cusToggle.BtnName = "seaBreezeToggle";
            _cusToggle.ButtonMode = InterfaceButtonMode.Background;
            _cusToggle.OnButtonClick = OnButtonClick;
            _cusToggle.Tag = _cusToggle;
            #endregion

            #region Auto Toggle
            var autoResult = InterfaceHelpers.FindGameObject(settings, "Auto_Toggle", out var autoToggle);

            if (!autoResult)
            {
                QuickLogger.Error($"Cannot find Auto_Toggle on GameObject");
                return false;
            }

            _autoToggle = autoToggle.AddComponent<CustomToggle>();
            _autoToggle.BtnName = "autoToggle";
            _autoToggle.ButtonMode = InterfaceButtonMode.Background;
            _autoToggle.OnButtonClick = OnButtonClick;
            _autoToggle.Tag = _autoToggle;
            #endregion

            return true;
        }

        private void OnLoadDisplay(DisplayData data)
        {
            QuickLogger.Debug("Loading SeaBreeze Display");

            _sbList.Clear();

            var items = _mono.SeaBreezes.Keys;


            if (data.EndPosition > items.Count)
            {
                data.EndPosition = items.Count;
            }

            _seaBreezeGrid.ClearPage();

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {

                var unit = _mono.SeaBreezes.Values.ElementAt(i);
                var unitNameController = unit;
                var unitName = unitNameController.GetName();
                unitNameController.SubscribeToNameChange(OnLabelChanged);

                GameObject itemDisplay = Instantiate(data.ItemsPrefab);

                itemDisplay.transform.SetParent(data.ItemsGrid.transform, false);
                var text = itemDisplay.transform.Find("Text").GetComponent<Text>();
                text.text = unitName;

                var itemButton = itemDisplay.AddComponent<CustomToggle>();
                itemButton.ButtonMode = InterfaceButtonMode.TextColor;
                itemButton.Tag = unit;
                itemButton.TextComponent = text;
                itemButton.OnButtonClick += OnButtonClick;
                itemButton.BtnName = "SeaBreeze";
                unitNameController.SetNameControllerTag(itemButton);
                _sbList.Add(itemButton);
                UpdateCheckedSB();
                QuickLogger.Debug($"Added Unit {unitName}");
            }

            _seaBreezeGrid.UpdaterPaginator(items.Count);
        }
        
        private void OnLabelChanged(string obj, NameController nameController)
        {
            UpdateSeaBreezes();
        }

        public override IEnumerator PowerOn()
        {
            _mono.AnimationManager.SetIntHash(_page, 1);
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
            _page = Animator.StringToHash("Page");
            
            _mono.FoodManager.OnFoodCookedAll += OnFoodCooked;
            _mono.FoodManager.OnCookingStart += OnCookingStart;
            _colorPage = mono.ColorManager;
            _seaBreezeGrid = gameObject.AddComponent<GridHelper>();

            if (FindAllComponents())
            {
                StartCoroutine(CompleteSetup());
            }
            else
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }
            
            _colorPage.SetupGrid(42, SeaCookerBuildable.ColorItemPrefab, _colorGrid, _paginator, OnButtonClick);

            StartCoroutine(CompleteSetup());

            InvokeRepeating(nameof(UpdateScreen), 0, 0.5f);
        }

        private void OnCookingStart(TechType raw, TechType cooked)
        {
            QuickLogger.Debug("Starting Processing Animation", true);

            //ToggleProcessDisplay();
            _mono.UpdateIsRunning();
            QuickLogger.Debug("Update Is Running", true);
            //uGUI_Icon fromIcon = _fromImage.gameObject.GetComponent<uGUI_Icon>();
            //fromIcon.sprite = SpriteManager.Get(raw);
            //QuickLogger.Debug("From Icon", true);
            
            //uGUI_Icon toIcon = _toImage.gameObject.GetComponent<uGUI_Icon>();
            //toIcon.sprite = SpriteManager.Get(cooked);
            //QuickLogger.Debug("From Icon", true);
            
            UpdateCookingButton();
            QuickLogger.Debug("Update Cooking Button", true);
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

        internal void UpdateSeaBreezes()
        {
            if (_seaBreezeGrid == null) return;
            QuickLogger.Debug("Update Seabreezes",true);
            _seaBreezeGrid.DrawPage(_seaBreezeGrid.GetCurrentPage());
        }
    }
}
