using AE.MiniFountainFilter.Buildable;
using AE.MiniFountainFilter.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using FCSCommon.Abstract;
using UnityEngine;
using UnityEngine.UI;
using FCSCommon.Components;
using FCSTechFabricator.Managers;

namespace AE.MiniFountainFilter.Managers
{
    internal class MFFDisplayManager : AIDisplay
    {
        private MiniFountainFilterController _mono;
        private int _page;
        private Color _startColor = new Color(0.16796875f, 0.16796875f, 0.16796875f);
        private Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private InterfaceButton _startButton;
        private GameObject _grid;
        private GameObject _fromImage;
        private GameObject _toImage;
        private Image _button1Progress;
        private ColorManager _colorPage;
        private Text _paginator;
        private Text _button1ProgressNumber;
        private Image _button2Progress;
        private Text _button2ProgressNumber;
        private bool _onInterfaceButton;


        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "homeBTN":
                    _mono.AnimationManager.SetIntHash(_page, 1);
                    break;

                case "waterContainerBTN":

                    if (!QPatch.Configuration.AutoGenerateMode)
                    {
                        _mono.StorageManager.GivePlayerBottle();
                    }
                    else
                    {
                        _mono.StorageManager.OpenStorage();
                    }

                    break;

                case "takeWaterBTN":
                    _mono.TankManager.GivePlayerWater();
                    break;

                case "ColorItem":
                    var color = (Color)tag;
                    QuickLogger.Debug($"{_mono.gameObject.name} Color Changed to {color.ToString()}", true);
                    _mono.ColorManager.ChangeColor(color);
                    break;

                case "colorPickerBTN":
                    _mono.AnimationManager.SetIntHash(_page, 2);
                    break;
            }
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

            canvasGameObject.gameObject.GetComponent<GraphicRaycaster>().ignoreReversedGraphics = false;
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

            var takeWaterResult = InterfaceHelpers.CreateButton(home, "Button_1", "takeWaterBTN", InterfaceButtonMode.Background,
                _startColor, _hoverColor, OnButtonClick, out var takeWaterButton);
            takeWaterButton.TextLineOne = "Take Water";
            takeWaterButton.OnInterfaceButton = SetOnInterfaceButton;
            if (!takeWaterResult)
            {
                return false;
            }
            _startButton = takeWaterButton;

            var colorPickerResult = InterfaceHelpers.CreateButton(home, "Paint_BTN", "colorPickerBTN", InterfaceButtonMode.Background,
                OnButtonClick, out var colorPickerButton);
            colorPickerButton.TextLineOne = "Color Picker Page";
            colorPickerButton.OnInterfaceButton = SetOnInterfaceButton;

            if (!colorPickerResult)
            {
                return false;
            }

            var fuelTankResult = InterfaceHelpers.CreateButton(home, "Button_2", "waterContainerBTN", InterfaceButtonMode.Background,
                _startColor, _hoverColor, OnButtonClick, out var fuelTankButton);
            fuelTankButton.TextLineOne = !QPatch.Configuration.AutoGenerateMode ? "Take Water Bottle" : "Open Water Container";
            fuelTankButton.OnInterfaceButton = SetOnInterfaceButton;

            if (!fuelTankResult)
            {
                return false;
            }

            var homeBTN = InterfaceHelpers.CreateButton(colorPicker, "Home_BTN", "homeBTN", InterfaceButtonMode.Background,
                OnButtonClick, out var homeButton);
            homeButton.TextLineOne = "Home Page";
            homeButton.OnInterfaceButton = SetOnInterfaceButton;

            if (!homeBTN)
            {
                return false;
            }

            var nextBTN = InterfaceHelpers.CreatePaginator(colorPicker, "NextPage", 1, _colorPage.ChangeColorPageBy, out var nextButton);
            nextButton.TextLineOne = "Next Page";
            nextButton.OnInterfaceButton = SetOnInterfaceButton;

            if (!nextBTN)
            {
                return false;
            }

            var prevBTN = InterfaceHelpers.CreatePaginator(colorPicker, "PrevPage", -1, _colorPage.ChangeColorPageBy, out var prevButton);
            prevButton.TextLineOne = "Previous Page";
            prevButton.OnInterfaceButton = SetOnInterfaceButton;


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

            var paginatorResult = InterfaceHelpers.FindGameObject(colorPicker, "Paginator", out var paginator);

            if (!paginatorResult)
            {
                return false;
            }
            _paginator = paginator.GetComponent<Text>();

            var button1ProgressResult = InterfaceHelpers.FindGameObject(home, "Button_1_Progress", out var button1Progress);

            if (!button1ProgressResult)
            {
                return false;
            }
            _button1Progress = button1Progress.GetComponent<Image>();

            var button1ProgressNumResult = InterfaceHelpers.FindGameObject(home, "Button_1_Progress_Number", out var button1ProgressNumber);

            if (!button1ProgressNumResult)
            {
                return false;
            }
            _button1ProgressNumber = button1ProgressNumber.GetComponent<Text>();
            _button1ProgressNumber.text = $"(0%)";

            var button2ProgressResult = InterfaceHelpers.FindGameObject(home, "Button_2_Progress", out var button2Progress);

            if (!button2ProgressResult)
            {
                return false;
            }
            _button2Progress = button2Progress.GetComponent<Image>();

            if (!QPatch.Configuration.AutoGenerateMode)
            {
                _button2Progress.fillAmount = 1;
            }

            var button2ProgressNumResult = InterfaceHelpers.FindGameObject(home, "Button_2_Amount_Number", out var button2ProgressNumber);

            if (!button2ProgressNumResult)
            {
                return false;
            }
            _button2ProgressNumber = button2ProgressNumber.GetComponent<Text>();
            _button2ProgressNumber.text = QPatch.Configuration.AutoGenerateMode ? $"0 {MiniFountainFilterBuildable.Bottles()}" : string.Empty;

            //var versionResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Version", out var version);

            //if (!versionResult)
            //{
            //    return false;
            //}
            //var versionLbl = version.GetComponent<Text>();
            //versionLbl.text = $"{MiniFountainFilterBuildable.Version()}: {QPatch.Version}";

            return true;
        }

        public override IEnumerator PowerOn()
        {
            _mono.AnimationManager.SetIntHash(_page, 1);
            yield return null;
        }

        public override IEnumerator PowerOff()
        {
            _mono.AnimationManager.SetIntHash(_page, 0);
            yield return null;
        }

        public override IEnumerator CompleteSetup()
        {
            StartCoroutine(PowerOn());
            yield return null;
        }

        public void Setup(MiniFountainFilterController mono)
        {
            _mono = mono;

            _page = Animator.StringToHash("Page");

            _mono.StorageManager.OnWaterAdded += OnWaterAdded;
            _mono.StorageManager.OnWaterRemoved += OnWaterRemoved;

            _mono.TankManager.OnTankUpdate += OnTankUpdate;


            _colorPage = mono.ColorManager;

            if (FindAllComponents())
            {
                StartCoroutine(CompleteSetup());
            }
            else
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }

            _colorPage.SetupGrid(25, MiniFountainFilterBuildable.ColorItemPrefab, _grid, _paginator, OnButtonClick);
            
            StartCoroutine(CompleteSetup());
        }

        private void OnTankUpdate()
        {
            QuickLogger.Debug("On Tank Update", true);
            _button1Progress.fillAmount = _mono.TankManager.GetTankPercentageDec();
            _button1ProgressNumber.text = $"{_mono.TankManager.GetTankPercentage()}%";
        }

        private void OnWaterRemoved()
        {
            QuickLogger.Debug("On Water Removed", true);
            UpdateBottleStatus();
        }

        private void OnWaterAdded()
        {
            QuickLogger.Debug("On Water Added", true);
            UpdateBottleStatus();
        }

        private void UpdateBottleStatus()
        {
            if (!QPatch.Configuration.AutoGenerateMode) return;
            _button2Progress.fillAmount = _mono.StorageManager.ContainerPercentage();
            _button2ProgressNumber.text = $"{_mono.StorageManager.NumberOfBottles} {MiniFountainFilterBuildable.Bottles()}";
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

        public override void ItemModified(TechType item, int newAmount = 0)
        {
            throw new NotImplementedException();
        }

        internal bool IsOnInterfaceButton()
        {
            return _onInterfaceButton;
        }

        private void SetOnInterfaceButton(bool value)
        {
            _onInterfaceButton = value;
        }
    }
}
