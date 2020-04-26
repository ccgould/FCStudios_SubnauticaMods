using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerFormattingStationDisplay : AIDisplay
    {
        private DSSServerFormattingStationController _mono;
        private GridHelper _filterGrid;
        private int _page;
        private readonly Color _startColor = Color.gray;
        private readonly Color _hoverColor = Color.white;
        
        internal void Setup(DSSServerFormattingStationController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                QuickLogger.Debug("Passed",true);
                _page = Animator.StringToHash("Page");
                _filterGrid?.DrawPage(1);
                PowerOnDisplay();
            }
        }

        public override void PowerOnDisplay()
        {
            GoToPage(FilterPages.Home);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "HomeBTN":
                    GoToPage(FilterPages.Home);
                    break;

                //case "ItemBTN":
                //    _currentBase?.TakeItem((TechType)tag);
                //    break;

                case "AddServerBTN":
                    //TODO Remove this and trigger when server added
                    _mono?.OpenDump();
                    GoToPage(FilterPages.FilterPage);
                    _mono.ToggleDummyServer();
                    break;

                case "RemoveServerBTN":
                    //TODO Give Player back a server
                    GoToPage(FilterPages.Home);
                    _mono.ToggleDummyServer();
                    break;

                    //case "ColorPickerBTN":
                    //    //TODO Check if antenna is attached
                    //    _mono.AnimationManager.SetIntHash(_page, 3);
                    //    break;

                    //case "ColorItem":
                    //    if (_currentColorPage == ColorPage.Terminal)
                    //        _mono.TerminalColorManager.ChangeColorMask((Color)tag);
                    //    else
                    //        _mono.AntennaColorManager.ChangeColorMask((Color)tag);
                    //    break;
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

                #region Filter Page
                var filterPage = InterfaceHelpers.FindGameObject(canvasGameObject, "FilterPage");
                #endregion

                #region Base Grid

                _filterGrid = _mono.gameObject.AddComponent<GridHelper>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(12 - 1, DSSModelPrefab.BaseItemPrefab, filterPage, _startColor, _hoverColor, OnButtonClick); //Minus 1 ItemPerPage because of the added Home button

                #endregion

                #region OpenRackBTNButton
                var removeServerBTN = InterfaceHelpers.FindGameObject(filterPage, "RemoveServerBTN");

                InterfaceHelpers.CreateButton(removeServerBTN, "RemoveServerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.OpenServerRackPage());
                #endregion

                #region CloseRackBTNButton
                var addServerBTN = InterfaceHelpers.FindGameObject(home, "AddServerBTN");

                InterfaceHelpers.CreateButton(addServerBTN, "AddServerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.CloseServerRackPage());
                #endregion

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}: {e.StackTrace}");
                return false;
            }
        }

        private void OnLoadFilterGrid(DisplayData obj)
        {

        }

        internal void GoToPage(FilterPages page)
        {
            _mono.AnimationManager.SetIntHash(_page,(int)page);
        }
    }

    internal enum FilterPages
    {
        Blackout,
        Home,
        FilterPage,
        ColorPage
    }
}
