using System.Collections;
using System.Linq;
using AE.IntraBaseTeleporter.Buildables;
using AE.IntraBaseTeleporter.Mono;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AE.IntraBaseTeleporter.Managers
{
    internal class BTDisplayManager : AIDisplay
    {
        private BaseTeleporterController _mono;
        private Color _startColor = new Color(0.1484375f, 0.98828125f, 0.203125f);
        private Color _hoverColor = new Color(1f, 1f, 1f, 1f);
        private int _page;
        private Text _textField;
        private ColorPageHelper _colorPage;
        private GridHelper _teleportGrid;

        public void Setup(BaseTeleporterController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");

            _colorPage = gameObject.AddComponent<ColorPageHelper>();
            _teleportGrid = gameObject.AddComponent<GridHelper>();

            if (!FindAllComponents())
            {
                QuickLogger.Error<BTDisplayManager>($"Cant Find All Components");
                return;
            }
            
            PowerOnDisplay();
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "ColorPageBTN":
                    _mono.AnimationManager.SetIntHash(_page,2);
                    break;
                case "HomeBTN":
                    _mono.AnimationManager.SetIntHash(_page, 1);
                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColor((Color)tag);
                    break;
                case "Unit":
                    var unit = (BaseTeleporterController) tag;
                    unit.TeleportManager.TeleportPlayer(_mono);
                    break;
                case "RenameBTN":
                    _mono.NameController.Show();
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

            #region Color Picker Button
            var colorPickerBtn = InterfaceHelpers.FindGameObject(home, "ColorPickerBTN");

            if (colorPickerBtn == null) return false;

            InterfaceHelpers.CreateButton(colorPickerBtn, "ColorPageBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor);
            #endregion

            #region Color Picker Button
            var renameBtn = InterfaceHelpers.FindGameObject(home, "RenameBTN");

            if (renameBtn == null) return false;

            InterfaceHelpers.CreateButton(renameBtn, "RenameBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor);
            #endregion

            #region Text Field

            _textField = InterfaceHelpers.FindGameObject(home, "UnitName")?.GetComponent<Text>();

            #endregion

            #region Grid
            var gridResult = InterfaceHelpers.FindGameObject(home, "Grid", out var grid);

            if (!gridResult)
            {
                QuickLogger.Error<BTDisplayManager>("Cant find grid on home page");
                return false;
            }

            var _paginator = InterfaceHelpers.FindGameObject(home, "Paginator");

            if (_paginator == null)
            {
                QuickLogger.Error<BTDisplayManager>("Cant find paginator on color home page");
                return false;
            }

            _teleportGrid.Initialize(BaseTeleporterBuildable.ItemPrefab, grid, _paginator, 7, OnButtonClick);
            _teleportGrid.OnLoadDisplay += OnLoadDisplay;
            #endregion

            var _colorGrid = InterfaceHelpers.FindGameObject(colorPicker, "Grid");

            if (_colorGrid == null)
            {
                QuickLogger.Error<BTDisplayManager>("Cant find color page on home page");
                return false;
            }

            var _colorPaginator = InterfaceHelpers.FindGameObject(colorPicker, "Paginator");

            if (_colorPaginator == null)
            {
                QuickLogger.Error<BTDisplayManager>("Cant find paginator on color picker page");
                return false;
            }

            #region ColorPage
            _colorPage.OnButtonClick = OnButtonClick;
            _colorPage.SerializedColors = ColorList.Colors;
            _colorPage.ColorsPerPage = 30;
            _colorPage.ColorItemPrefab = BaseTeleporterBuildable.ColorItemPrefab;
            _colorPage.ColorPageContainer = _colorGrid;
            _colorPage.ColorPageNumber = _colorPaginator.GetComponent<Text>();
            _colorPage.Initialize();
            #endregion

            #region Home Button
            var homeBtn = InterfaceHelpers.FindGameObject(colorPicker, "HomeBTN");

            if (homeBtn == null) return false;

            InterfaceHelpers.CreateButton(homeBtn, "HomeBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor);
            #endregion

            #region Prev Color Button
            var prevColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "PrevButton");

            if (prevColorBtn == null) return false;

            InterfaceHelpers.CreatePaginator(prevColorBtn, -1,_colorPage.ChangeColorPageBy,_startColor,_hoverColor);
            #endregion

            #region Next Color Button
            var nextColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "NextButton");

            if (nextColorBtn == null) return false;

            InterfaceHelpers.CreatePaginator(nextColorBtn, 1, _colorPage.ChangeColorPageBy, _startColor, _hoverColor);
            #endregion

            #region Prev  Button
            var prevBtn = InterfaceHelpers.FindGameObject(home, "PrevButton");

            if (prevBtn == null) return false;

            InterfaceHelpers.CreatePaginator(prevBtn, -1, _teleportGrid.ChangePageBy, _startColor, _hoverColor);
            #endregion

            #region Next Color Button
            var nextBtn = InterfaceHelpers.FindGameObject(home, "NextButton");

            if (nextBtn == null) return false;

            InterfaceHelpers.CreatePaginator(nextBtn, 1, _teleportGrid.ChangePageBy, _startColor, _hoverColor);
            #endregion

            return true;
        }

        private void OnLoadDisplay(GameObject itemPrefab, GameObject itemsGrid,int stPos,int endPos)
        {
            QuickLogger.Debug("Loading Teleports Display");

            var items = _mono.Manager.BaseUnits.Where(x => x != _mono).ToList();


            if (endPos > items.Count)
            {
                endPos = items.Count;
            }
            
            _teleportGrid.ClearPage();
            
            for (int i = stPos; i < endPos; i++)
            {
                var unit = items[i];
                var unitName = unit.GetName();
                
                if(unit == _mono) continue;

                GameObject itemDisplay = Instantiate(itemPrefab);

                itemDisplay.transform.SetParent(itemsGrid.transform, false);
                var text = itemDisplay.transform.Find("Location_LBL").GetComponent<Text>();
                text.text = unitName;

                var itemButton = itemDisplay.AddComponent<InterfaceButton>();
                itemButton.ButtonMode = InterfaceButtonMode.TextColor;
                itemButton.Tag = unit;
                itemButton.TextComponent = text;
                itemButton.OnButtonClick += OnButtonClick;
                itemButton.BtnName = "Unit";

                QuickLogger.Debug($"Added Unit {unitName}");
            }

            _teleportGrid.UpdaterPaginator(items.Count);
        }

        internal void SetDisplay(string text)
        {
            if (_textField == null) return;
            _textField.text = text;
        }

        public override void PowerOnDisplay()
        {
            QuickLogger.Debug("PowerOnDisplay");
            _mono.AnimationManager.SetIntHash(_page, 1);
        }

        public override IEnumerator PowerOff()
        {
            _mono.AnimationManager.SetIntHash(_page, 0);
            yield return null;
        }

        public GameObject GetNameTextBox()
        {
            return _textField.gameObject;
        }

        internal void UpdateUnits()
        {
            QuickLogger.Debug($"Updating Unit Named: {_mono.GetName()}");
            _teleportGrid.DrawPage();
        }
    }
}
