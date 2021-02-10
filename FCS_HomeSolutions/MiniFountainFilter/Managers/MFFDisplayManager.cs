using System;
using System.Collections;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.MiniFountainFilter.Mono;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.MiniFountainFilter.Managers
{
    internal class MFFDisplayManager : AIDisplay
    {
        private MiniFountainFilterController _mono;
        private int _page;
        private Color _startColor = new Color(0.5f, 0.5f, 0.5f);
        private Color _hoverColor =Color.white;
        private Text _button1ProgressNumber;
        private Text _button2ProgressNumber;

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "homeBTN":
                    _mono.AnimationManager.SetIntHash(_page, 1);
                    break;

                case "waterContainerBTN":

                    if (!QPatch.Configuration.MiniFountainFilterAutoGenerateMode)
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
                    _mono.GetColorManager().ChangeColor(color);
                    break;

                case "colorPickerBTN":
                    _mono.AnimationManager.SetIntHash(_page, 2);
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
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

                var home = GameObjectHelpers.FindGameObject(canvasGameObject, "Home");

                var button1 = GameObjectHelpers.FindGameObject(canvasGameObject, "Button_1");
                var button2 = GameObjectHelpers.FindGameObject(canvasGameObject, "Button_2");
                
                InterfaceHelpers.CreateButton(button1, "takeWaterBTN", InterfaceButtonMode.Background, OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "Take Water");
                
                InterfaceHelpers.CreateButton(button2, "waterContainerBTN", InterfaceButtonMode.Background, OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, !QPatch.Configuration.MiniFountainFilterAutoGenerateMode ? "Take Water Bottle" : "Open Water Container");
                
                InterfaceHelpers.FindGameObject(home, "Button_1_Progress", out var button1Progress);

                InterfaceHelpers.FindGameObject(home, "Button_1_Progress_Number", out var button1ProgressNumber);
                _button1ProgressNumber = button1ProgressNumber.GetComponent<Text>();
                _button1ProgressNumber.text = $"(0%)";

                InterfaceHelpers.FindGameObject(home, "Button_2_Progress", out var button2Progress);
                
                InterfaceHelpers.FindGameObject(home, "Button_2_Amount_Number", out var button2ProgressNumber);

                _button2ProgressNumber = button2ProgressNumber.GetComponent<Text>();
                _button2ProgressNumber.text = QPatch.Configuration.MiniFountainFilterAutoGenerateMode ? $"0 {MiniFountainFilterBuildable.Bottles()}" : string.Empty;

                //var versionResult = InterfaceHelpers.FindGameObject(canvasGameObject, "Version", out var version);

                //if (!versionResult)
                //{
                //    return false;
                //}
                //var versionLbl = version.GetComponent<Text>();
                //versionLbl.text = $"{MiniFountainFilterBuildable.Version()}: {QPatch.Version}";
                
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[FindAllComponents]: {e.Message}");
                return false;
            }
            return true;
        }

        public override void PowerOnDisplay()
        {
            _mono.AnimationManager.SetIntHash(_page, 1);
        }

        public override void HibernateDisplay()
        {
            _mono.AnimationManager.SetIntHash(_page, 0);
        }


        public void Setup(MiniFountainFilterController mono)
        {
            _mono = mono;

            _page = Animator.StringToHash("Page");

            if (FindAllComponents())
            {

                _mono.StorageManager.OnWaterAdded += OnWaterAdded;
                _mono.StorageManager.OnWaterRemoved += OnWaterRemoved;
                _mono.TankManager.OnTankUpdate += OnTankUpdate;

                PowerOnDisplay();
            }
            else
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }
            PowerOnDisplay();
        }

        private void OnTankUpdate()
        {
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
            if (!QPatch.Configuration.MiniFountainFilterAutoGenerateMode) return;
            _button2ProgressNumber.text = $"{_mono.StorageManager.NumberOfBottles} {MiniFountainFilterBuildable.Bottles()}";
        }
        
        public void AboveWaterMessage()
        {
            _mono.AnimationManager.SetIntHash(_page, 3);
        }
    }
}
