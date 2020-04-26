using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSTerminalDisplay : AIDisplay
    {
        private DSSTerminalController _mono;
        private readonly Color _startColor = Color.black;
        private readonly Color _hoverColor = Color.white;
        private int _page;
        private GridHelper _baseGrid;
        private GridHelper _baseItemsGrid;
        private BaseManager _currentBase;
        private TransferData _currentData;
        private ColorManager _terminalColorPage;
        private ColorManager _antennaColorPage;
        private GameObject _antennaColorPicker;
        private ColorPage _currentColorPage;

        internal void Setup(DSSTerminalController mono)
        {
            _mono = mono;
            _terminalColorPage = mono.TerminalColorManager;
            


            if (FindAllComponents())
            {
                _page = Animator.StringToHash("TerminalPage");
                _baseGrid.DrawPage(1);
                PowerOnDisplay();
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "BaseBTN":
                    _currentBase = ((TransferData) tag).Manager;
                    _currentData = (TransferData) tag;
                    QuickLogger.Debug($"Base clicked: {_currentBase}");
                    _mono.AnimationManager.SetIntHash(_page, 2);
                    Refresh();
                    break;

                case "HomeBTN":
                    _mono.AnimationManager.SetIntHash(_page, 1);
                    break;

                case "ItemBTN":
                    _currentBase?.TakeItem((TechType)tag);
                    break;

                case "DumpBTN":
                    _currentBase?.OpenDump(_currentData);
                    break;

                case "TerminalColorBTN":
                    _mono.AnimationManager.SetIntHash(_page,4);
                    _currentColorPage = ColorPage.Terminal;
                    break;

                case "AntennaColorBTN":
                    //TODO Check if antenna is attached
                    _mono.AnimationManager.SetIntHash(_page, 5);
                    _currentColorPage = ColorPage.Antenna;
                    break;

                case "ColorPickerBTN":
                    //TODO Check if antenna is attached
                    _mono.AnimationManager.SetIntHash(_page, 3);
                    break;

                case "ColorItem":
                    if (_currentColorPage == ColorPage.Terminal)
                        _mono.TerminalColorManager.ChangeColorMask((Color) tag);
                    else
                        _mono.AntennaColorManager.ChangeColorMask((Color) tag);
                    break;
            }
        }

        internal void UpdateAntennaColorPage()
        {
            _antennaColorPage = _mono?.AntennaColorManager;

            if (_antennaColorPage != null)
            {
                #region ColorPage
                _antennaColorPage.SetupGrid(90, DSSModelPrefab.ColorItemPrefab, _antennaColorPicker, OnButtonClick, _startColor, _hoverColor);
                #endregion
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
            #endregion

            #region Home
            var home = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
            #endregion

            #region BaseItemsPage
            var baseItemsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "BaseItemsPage");
            #endregion

            #region ColorPageMain
            var colorMainPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPageMain");
            #endregion

            #region ScreenColorPicker

            var screenColorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "TerminalColorPage");

            #endregion

            #region AntennnaColorPicker

            _antennaColorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "AntennaColorPage");

            #endregion

            #region Base Grid

            _baseGrid = _mono.gameObject.AddComponent<GridHelper>();
            _baseGrid.OnLoadDisplay += OnLoadBaseGrid;
            _baseGrid.Setup(12-1, DSSModelPrefab.BaseItemPrefab, home, _startColor, _hoverColor, OnButtonClick); //Minus 1 ItemPerPage because of the added Home button
            
            #endregion

            #region Base Items Page

            _baseItemsGrid = _mono.gameObject.AddComponent<GridHelper>();
            _baseItemsGrid.OnLoadDisplay += OnLoadBaseItemsGrid;
            _baseItemsGrid.Setup(44, DSSModelPrefab.ItemPrefab, baseItemsPage, _startColor, _hoverColor, OnButtonClick);
            #endregion
            
            #region DumpBTNButton
            var closeBTN = InterfaceHelpers.FindGameObject(baseItemsPage, "DumpButton");

            InterfaceHelpers.CreateButton(closeBTN, "DumpBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase());
            #endregion

            #region ColorPickerBTN
            var colorPickerBTN = InterfaceHelpers.FindGameObject(home, "ColorPickerBTN");

            InterfaceHelpers.CreateButton(colorPickerBTN, "ColorPickerBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase());//TODO replace
            #endregion

            #region ColorPickerMainHomeBTN
            var colorPickerMainHomeBTN = InterfaceHelpers.FindGameObject(colorMainPage, "HomeBTN");

            InterfaceHelpers.CreateButton(colorPickerMainHomeBTN, "HomeBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase());//TODO replace
            #endregion

            #region Terminal Color BTN
            var terminalColorBTN = InterfaceHelpers.FindGameObject(colorMainPage, "TerminalColorBTN");

            InterfaceHelpers.CreateButton(terminalColorBTN, "TerminalColorBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase()); //TODO Change
            #endregion

            #region DumpBTNButton
            var antennaColorBTN = InterfaceHelpers.FindGameObject(colorMainPage, "AntennaColorBTN");

            InterfaceHelpers.CreateButton(antennaColorBTN, "AntennaColorBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase()); //TODO Change
            #endregion

            #region ColorPage
            _terminalColorPage.SetupGrid(90, DSSModelPrefab.ColorItemPrefab, screenColorPicker, OnButtonClick, _startColor, _hoverColor);
            #endregion
            
            return true;
        }

        private void OnLoadBaseItemsGrid(DisplayData data)
        {
            QuickLogger.Debug($"OnLoadBaseItemsGrid : {data.ItemsGrid}", true);

            _baseItemsGrid.ClearPage();

            if(_currentBase == null) return;

            var grouped = _currentBase.GetItemsWithin()?.ToList();

            


            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {

                GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);

                var itemBTN = buttonPrefab.AddComponent<InterfaceButton>();
                itemBTN.ButtonMode = InterfaceButtonMode.Background;
                itemBTN.STARTING_COLOR = _startColor;
                itemBTN.HOVER_COLOR = _hoverColor;
                itemBTN.BtnName = "ItemBTN";
                itemBTN.Tag = grouped[i].Key;
                itemBTN.OnButtonClick = OnButtonClick;
                
                uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                trashIcon.sprite = SpriteManager.Get(grouped[i].Key);
            }
            _baseItemsGrid.UpdaterPaginator(grouped.Count);
        }

        internal void OnLoadBaseGrid(DisplayData data)
        {
            _baseGrid.ClearPage();

            var grouped = BaseManager.BaseAntennas;
            

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            GameObject button1Prefab = Instantiate(data.ItemsPrefab);
            button1Prefab.transform.SetParent(data.ItemsGrid.transform, false);
            button1Prefab.GetComponentInChildren<Text>().text = "HOME";

            var mainBTN = button1Prefab.AddComponent<InterfaceButton>();
            mainBTN.ButtonMode = InterfaceButtonMode.Background;
            mainBTN.STARTING_COLOR = _startColor;
            mainBTN.HOVER_COLOR = _hoverColor;
            mainBTN.BtnName = "BaseBTN";
            mainBTN.Tag = new TransferData { AntennaController = null, Manager = _mono.Manager };
            mainBTN.OnButtonClick = OnButtonClick;


            if (_mono.Manager.GetCurrentBaseAntenna() != null)
            {
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        if (buttonPrefab != null)
                        {
                            QuickLogger.Debug("Destroying Tab", true);
                            Destroy(buttonPrefab);
                        }
                        return;
                    }
                    
                    if (grouped[i].Manager == _mono.Manager) continue;

                    CreateButton(data, buttonPrefab, grouped, i);
                }
            }
            
            _baseGrid.UpdaterPaginator(grouped.Count);
        }

        private void CreateButton(DisplayData data, GameObject buttonPrefab, List<IBaseAntenna> grouped, int i)
        {
            buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
            buttonPrefab.GetComponentInChildren<Text>().text = grouped[i].GetName().TruncateWEllipsis(30);

            var mainBTN = buttonPrefab.AddComponent<InterfaceButton>();
            mainBTN.ButtonMode = InterfaceButtonMode.Background;
            mainBTN.STARTING_COLOR = _startColor;
            mainBTN.HOVER_COLOR = _hoverColor;
            mainBTN.BtnName = "BaseBTN";
            mainBTN.Tag = new TransferData {AntennaController = grouped[i], Manager = grouped[i].Manager};
            mainBTN.OnButtonClick = OnButtonClick;
        }

        public override void PowerOnDisplay()
        {
            _mono.AnimationManager.SetIntHash(_page,1);
        }

        public void Refresh()
        {
            _baseGrid.DrawPage();
            _baseItemsGrid.DrawPage();
        }
    }

    internal struct TransferData
    {
        public IBaseAntenna AntennaController { get; set; }
        public BaseManager Manager { get; set; }
    }
}

internal enum ColorPage
{
    Terminal,
    Antenna
}
