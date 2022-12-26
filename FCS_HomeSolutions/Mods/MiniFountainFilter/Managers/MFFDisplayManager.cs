using System;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.MiniFountainFilter.Managers
{
    internal class MFFDisplayManager : AIDisplay
    {
        private MiniFountainFilterController _mono;
        private int _page;
        private Color _startColor = new Color(0.5f, 0.5f, 0.5f);
        private Color _hoverColor =Color.white;
        private Text _button1ProgressNumber;
        private Text _button2ProgressNumber;
        private GameObject _home;
        private GameObject _aboveWater;
        private GameObject _canvasGameObject;

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "waterContainerBTN":

                    if (!Main.Configuration.MiniFountainFilterAutoGenerateMode)
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
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas  
                _canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (_canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }

                _canvasGameObject.gameObject.GetComponent<GraphicRaycaster>().ignoreReversedGraphics = false;
                #endregion

                _home = GameObjectHelpers.FindGameObject(_canvasGameObject, "Home");
                _aboveWater = GameObjectHelpers.FindGameObject(_canvasGameObject, "AboveWaterError");

                var button1 = GameObjectHelpers.FindGameObject(_canvasGameObject, "Button_1");
                var button2 = GameObjectHelpers.FindGameObject(_canvasGameObject, "Button_2");

                
                InterfaceHelpers.CreateButton(button1, "takeWaterBTN", InterfaceButtonMode.Background, OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "Take Water");
                
                InterfaceHelpers.CreateButton(button2, "waterContainerBTN", InterfaceButtonMode.Background, OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, !Main.Configuration.MiniFountainFilterAutoGenerateMode ? "Take Water Bottle" : "Open Water Container");
                
                InterfaceHelpers.FindGameObject(_home, "Button_1_Progress", out var button1Progress);

                InterfaceHelpers.FindGameObject(_home, "Button_1_Progress_Number", out var button1ProgressNumber);
                _button1ProgressNumber = button1ProgressNumber.GetComponent<Text>();
                _button1ProgressNumber.text = $"(0%)";

                InterfaceHelpers.FindGameObject(_home, "Button_2_Progress", out var button2Progress);
                
                InterfaceHelpers.FindGameObject(_home, "Button_2_Amount_Number", out var button2ProgressNumber);

                _button2ProgressNumber = button2ProgressNumber.GetComponent<Text>();
                _button2ProgressNumber.text = Main.Configuration.MiniFountainFilterAutoGenerateMode ? $"0 {MiniFountainFilterBuildable.Bottles()}" : string.Empty;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[FindAllComponents]: {e.Message}");
                return false;
            }
            return true;
        }

        public override void TurnOnDisplay()
        {
            _canvasGameObject.SetActive(true);
        }

        public override void TurnOffDisplay()
        {
            _canvasGameObject.SetActive(false);
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
            InvokeRepeating(nameof(AboveWaterMessage),1f,1f);
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
            if (!Main.Configuration.MiniFountainFilterAutoGenerateMode) return;
            _button2ProgressNumber.text = $"{_mono.StorageManager.NumberOfBottles} {MiniFountainFilterBuildable.Bottles()}";
        }
        
        private void AboveWaterMessage()
        {
            if(_mono == null || _aboveWater == null || _home == null) return;

            if (!_mono.IsUnderWater())
            {
                _aboveWater.SetActive(true);
                _home.SetActive(false);
                return;
            }

            _aboveWater.SetActive(false);
            _home.SetActive(true);
        }
    }
}
