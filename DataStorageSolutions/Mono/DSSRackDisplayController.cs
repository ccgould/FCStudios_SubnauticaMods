using System;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSRackDisplayController : AIDisplay
    {
        private DSSRackController _mono;
        private int _page;
        private readonly Color _startColor = Color.gray;
        private readonly Color _hoverColor = Color.white;
        private Text _counter;
        private ColorManager _colorPickerPage;

        internal void Setup(DSSRackController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");
            _colorPickerPage = mono.ColorManager;

            if (!FindAllComponents()) return;

            UpdateContainerAmount();
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "AddServerBTN":
                    _mono.ToggleRackState(true);
                    _mono.DumpContainer.OpenStorage();
                    break;
                case "OpenBTN":
                    _mono.ToggleRackState();
                    break;
                case "CloseBTN":
                    _mono.ToggleRackState();
                    break;
                case "ColorPickerBTN":
                    GoToPage(RackPages.ColorPicker);
                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColorMask((Color) tag);
                    break;
                case "HomeBTN":
                    GoToPage(RackPages.Home);
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
                #endregion

                #region Home
                var home = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region Powered Off Page
                var poweredOffPage = InterfaceHelpers.FindGameObject(canvasGameObject, "PoweredOff");
                #endregion

                #region ColorPickerPage

                var colorPickerPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPage");

                #endregion

                #region OpenRackBTNButton
                var openRackBTN = InterfaceHelpers.FindGameObject(home, "OpenBTN");

                InterfaceHelpers.CreateButton(openRackBTN, "OpenBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.OpenServerRackPage());
                #endregion

                #region CloseRackBTNButton
                var closeBTN = InterfaceHelpers.FindGameObject(home, "CloseBTN");

                InterfaceHelpers.CreateButton(closeBTN, "CloseBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.CloseServerRackPage());
                #endregion

                #region ColorPickerBTN
                var colorPickerBTN = InterfaceHelpers.FindGameObject(home, "ColorPicker");

                InterfaceHelpers.CreateButton(colorPickerBTN, "ColorPickerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.ColorPage());
                #endregion

                #region ColorPickerMainHomeBTN
                var colorPickerHomeBTN = InterfaceHelpers.FindGameObject(colorPickerPage, "HomeBTN");

                InterfaceHelpers.CreateButton(colorPickerHomeBTN, "HomeBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.GoToHome());
                #endregion

                #region ColorPage
                _colorPickerPage.SetupGrid(20, DSSModelPrefab.ColorItemPrefab, colorPickerPage, OnButtonClick, _startColor, _hoverColor);
                #endregion

                #region AddServerBTN
                var addServerBTN = InterfaceHelpers.FindGameObject(home, "AddServerBTN");

                InterfaceHelpers.CreateButton(addServerBTN, "AddServerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.AddServer());
                #endregion

                _counter = InterfaceHelpers.FindGameObject(home, "Counter")?.GetComponent<Text>();

                var poweredOffMessage = InterfaceHelpers.FindGameObject(poweredOffPage, "Text")?.GetComponent<Text>();
                poweredOffMessage.text = AuxPatchers.PoweredOff();
                
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message} || {e.StackTrace}");
                return false;
            }
        }

        internal void UpdateContainerAmount()
        {
            if(_mono == null || _counter == null) return;
            var total = _mono.GetTotalStorage();
            _counter.text = $"{total.x} / {total.y}";
        }

        public override void PowerOnDisplay()
        {
            GoToPage(RackPages.Home);
        }

        public override void PowerOffDisplay()
        {
            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Unpowered)
            {
                GoToPage(RackPages.Blackout);
            }
            else if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Tripped)
            {
                GoToPage(RackPages.Tripped);
            }

        }

        internal void GoToPage(RackPages page)
        {
            if(!_mono.IsConstructed) return;
            _mono.AnimationManager.SetIntHash(_page, (int)page);
        }
    }
}
