using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Managers;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Enumerators;
using QuantumTeleporter.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumTeleporter.Managers
{
    internal class QTDisplayManager : AIDisplay
    {
        private QuantumTeleporterController _mono;
        private readonly Color _startColor = new Color(0.1484375f, 0.98828125f, 0.203125f);
        private readonly Color _hoverColor = new Color(1f, 1f, 1f, 1f);
        private int _page;
        private Text _textField;
        private ColorManager _colorPage;
        private GridHelper _teleportGrid;
        private Toggle _isGlobalToggle;
        internal QTTabs SelectedTab;
        private InterfaceButton _intraTeleBtn;
        private InterfaceButton _globalTeleBtn;
        private QuantumTeleporterController _toUnit;
        private QTTeleportTypes _tType;
        private Text _destination;
        private bool _initialized;
        private const float MaxInteractionRange = 1f;
        internal void Setup(QuantumTeleporterController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");

            _colorPage = mono.ColorManager;
            _teleportGrid = gameObject.AddComponent<GridHelper>();

            if (!FindAllComponents())
            {
                QuickLogger.Error<QTDisplayManager>($"Cant Find All Components");
                return;
            }
            
            PowerOnDisplay();

            SetTab(QTTabs.Intra);

            BaseManager.OnGlobalChanged += OnGlobalChanged;

            _initialized = true;
        }

        private void OnGlobalChanged()
        {
            UpdateUnits();
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
                    _toUnit = (QuantumTeleporterController) tag;
                    
                    var ttype = QTTeleportTypes.Global;

                    if (SelectedTab == QTTabs.Intra)
                    {
                        ttype = QTTeleportTypes.Intra;
                    }

                    _tType = ttype;

                    if (_toUnit.PowerManager.HasEnoughPower(ttype))
                    {
                        _destination.text = $"[{_toUnit.GetName()}]";
                        _mono.AnimationManager.SetIntHash(_page, 3);
                    }
                    break;

                case "RenameBTN":
                    _mono.NameController.Show();
                    break;

                case "GlobalTeleBtn":
                    SetTab(QTTabs.Global);
                    break;

                case "IntraTeleBtn":
                    SetTab(QTTabs.Intra);
                    break;
                case "ConfirmYesBtn":
                    ConfirmTeleport();
                    break;
                case "ConfirmNoBtn":
                    _mono.AnimationManager.SetIntHash(_page,1);
                    break;
            }
        }
        
        private void ConfirmTeleport()
        {
            TeleportManager.TeleportPlayer(_mono, _toUnit, _tType);
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

            #region L_Panel

            var lPanel = canvasGameObject.FindChild("Left_Panel")?.gameObject;

            if (lPanel == null)
            {
                QuickLogger.Error("Unable to find L_Panel GameObject");
                return false;
            }
            #endregion

            #region R_Panel

            var rPanel = canvasGameObject.FindChild("Right_Panel")?.gameObject;

            if (rPanel == null)
            {
                QuickLogger.Error("Unable to find R_Panel GameObject");
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

            #region confirmation

            var confirmation = canvasGameObject.FindChild("Confirmation")?.gameObject;

            if (confirmation == null)
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
            var colorPickerBtn = InterfaceHelpers.FindGameObject(rPanel, "ColorPickerBTN");

            if (colorPickerBtn == null) return false;

            InterfaceHelpers.CreateButton(colorPickerBtn, "ColorPageBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MaxInteractionRange);
            #endregion

            #region Color Picker Button
            var renameBtn = InterfaceHelpers.FindGameObject(rPanel, "RenameBTN");

            if (renameBtn == null) return false;

            InterfaceHelpers.CreateButton(renameBtn, "RenameBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MaxInteractionRange);
            #endregion

            #region Text Field

            _textField = InterfaceHelpers.FindGameObject(lPanel, "UnitName")?.GetComponent<Text>();

            #endregion

            #region Grid
            var gridResult = InterfaceHelpers.FindGameObject(home, "Grid", out var grid);

            if (!gridResult)
            {
                QuickLogger.Error<QTDisplayManager>("Cant find grid on home page");
                return false;
            }

            var _paginator = InterfaceHelpers.FindGameObject(home, "Paginator");

            if (_paginator == null)
            {
                QuickLogger.Error<QTDisplayManager>("Cant find paginator on color home page");
                return false;
            }

            _teleportGrid.Initialize(QuantumTeleporterBuildable.ItemPrefab, grid, _paginator, 7, OnButtonClick);
            _teleportGrid.OnLoadDisplay += OnLoadDisplay;
            #endregion

            #region Color Grid
            var _colorGrid = InterfaceHelpers.FindGameObject(colorPicker, "Grid");

            if (_colorGrid == null)
            {
                QuickLogger.Error<QTDisplayManager>("Cant find color page on home page");
                return false;
            } 
            #endregion

            #region Color Paginator
            var _colorPaginator = InterfaceHelpers.FindGameObject(colorPicker, "Paginator");

            if (_colorPaginator == null)
            {
                QuickLogger.Error<QTDisplayManager>("Cant find paginator on color picker page");
                return false;
            } 
            #endregion

            #region ColorPage
            _colorPage.SetupGrid(30, QuantumTeleporterBuildable.ColorItemPrefab, _colorGrid, _colorPaginator.GetComponent<Text>(), OnButtonClick);
            #endregion

            #region Home Button
            var homeBtn = InterfaceHelpers.FindGameObject(colorPicker, "HomeBTN");

            if (homeBtn == null) return false;

            InterfaceHelpers.CreateButton(homeBtn, "HomeBTN", InterfaceButtonMode.Background,
                OnButtonClick, _startColor, _hoverColor, MaxInteractionRange);
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
            
            #region Global Toggle
            var isGlobalToggle = InterfaceHelpers.FindGameObject(lPanel, "Toggle");

            if (isGlobalToggle == null) return false;

            var globalToggleLabel = InterfaceHelpers.FindGameObject(isGlobalToggle, "Label");
            if (globalToggleLabel == null) return false;
            globalToggleLabel.GetComponent<Text>().text = QuantumTeleporterBuildable.MakeGlobalUnit();

            _isGlobalToggle = isGlobalToggle.GetComponent<Toggle>();
            _isGlobalToggle.onValueChanged.AddListener(delegate
            {
                ToggleValueChanged(_isGlobalToggle);
            });
            #endregion

            #region Intra Tele Button
            var intraTele = InterfaceHelpers.FindGameObject(home, "Intra_TeleBtn");

            if (intraTele == null) return false;

            _intraTeleBtn = InterfaceHelpers.CreateButton(intraTele, "IntraTeleBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.black, Color.white, MaxInteractionRange);
            _intraTeleBtn.ChangeText(QuantumTeleporterBuildable.LocalNetwork());
            #endregion

            #region GLobal Tele Button
            var globalTele = InterfaceHelpers.FindGameObject(home, "Global_TeleBtn");

            if (globalTele == null) return false;

            _globalTeleBtn = InterfaceHelpers.CreateButton(globalTele, "GlobalTeleBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.black, Color.white, MaxInteractionRange);
            _globalTeleBtn.ChangeText(QuantumTeleporterBuildable.GlobalNetwork());
            #endregion

            #region Information Label
            var unitInfo = InterfaceHelpers.FindGameObject(lPanel, "UnitNameInfo");

            if (unitInfo == null) return false;

            var infoText = unitInfo.GetComponent<Text>();

            infoText.text = LeftPanelText();
            #endregion

            #region Yes Button
            var yesBTNGO = InterfaceHelpers.FindGameObject(confirmation, "YesBTN");

            if (yesBTNGO == null) return false;

                var yesBTN = InterfaceHelpers.CreateButton(yesBTNGO, "ConfirmYesBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.black, Color.white, MaxInteractionRange);
                yesBTN.ChangeText(QuantumTeleporterBuildable.Confirm());
            #endregion

            #region No Button
            var noBTNGO = InterfaceHelpers.FindGameObject(confirmation, "NoBTN");

            if (noBTNGO == null) return false;

            var noBTN = InterfaceHelpers.CreateButton(noBTNGO, "ConfirmNoBtn", InterfaceButtonMode.Background,
                OnButtonClick, Color.black, Color.white, MaxInteractionRange);
            noBTN.ChangeText(QuantumTeleporterBuildable.Cancel());
            #endregion

            #region Confirmation
            var confirmMessage = InterfaceHelpers.FindGameObject(confirmation, "Message");

            if (confirmMessage == null) return false;

            confirmMessage.GetComponent<Text>().text = QuantumTeleporterBuildable.ConfirmMessage();
            #endregion

            #region Destination
            var destination = InterfaceHelpers.FindGameObject(confirmation, "Destination");

            if (destination == null) return false;

            _destination = destination.GetComponent<Text>();
            #endregion

            return true;
        }

        private string LeftPanelText()
        {
            var sb = new StringBuilder();
            sb.Append($"<size=90><color=aqua><b>{QuantumTeleporterBuildable.Location()}: </b></color></size>");
            sb.Append(Environment.NewLine);
            sb.Append($"<size=100><color=yellow>{_mono.transform.position}</color></size>");
            sb.Append(Environment.NewLine);
            sb.Append($"<size=70><color=aqua><b>{QuantumTeleporterBuildable.GlobalTeleport()}: </b></color></size>");
            sb.Append(Environment.NewLine);
            sb.Append($"<size=100><color=yellow>({QPatch.Configuration.GlobalTeleportPowerUsage})</color> {QuantumTeleporterBuildable.PerUnit()}</size>");
            sb.Append(Environment.NewLine);
            sb.Append($"<size=70><color=aqua><b>{QuantumTeleporterBuildable.InternalTeleport()}: </b></color></size>");
            sb.Append(Environment.NewLine);
            sb.Append($"<size=100><color=yellow>({QPatch.Configuration.InternalTeleportPowerUsage})</color> {QuantumTeleporterBuildable.PerUnit()}</size>");
            return sb.ToString();
        }

        private void ToggleValueChanged(Toggle change)
        {
            _mono.SetIsGlobal(change.isOn);
           QuickLogger.Debug(change.isOn ? "Adding to Global List" : "Removing from Global List");
        }

        private void OnLoadDisplay(GameObject itemPrefab, GameObject itemsGrid,int stPos,int endPos)
        {
            List<FCSController> items = null;

            if (SelectedTab == QTTabs.Global)
            {
                items = BaseManager.GlobalUnits.Where(x => x != _mono).ToList();
            }
            else if(SelectedTab == QTTabs.Intra)
            {
                items = _mono.Manager.BaseUnits.Where(x => x != _mono).ToList();
            }

            if (items == null)
            {
                QuickLogger.Error<QTDisplayManager>("Items list returned null");
                return;
            }

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
                var statusLBL = InterfaceHelpers.FindGameObject(itemDisplay, "Status_LBL");

                if (itemDisplay == null || itemsGrid == null)
                {
                    if (itemDisplay != null)
                    {
                        Destroy(itemDisplay);
                    }
                    return;
                }
                        
                itemDisplay.transform.SetParent(itemsGrid.transform, false);
                var textField = itemDisplay.transform.Find("Location_LBL").gameObject;
                var text = textField.GetComponent<Text>();
                text.text = unitName;

                var statusUpdater = itemDisplay.AddComponent<ItemDisplayStatusHandler>();
                statusUpdater.Initialize((QuantumTeleporterController)unit, textField,statusLBL,this, MaxInteractionRange);
   
                var itemButton = itemDisplay.AddComponent<InterfaceButton>();
                itemButton.ButtonMode = InterfaceButtonMode.TextColor;
                itemButton.Tag = unit;
                itemButton.TextComponent = text;
                itemButton.OnButtonClick += OnButtonClick;
                itemButton.BtnName = "Unit";
                itemButton.MaxInteractionRange = MaxInteractionRange;

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

        internal GameObject GetNameTextBox()
        {
            return _textField.gameObject;
        }

        internal void UpdateUnits()
        {
            if (!_initialized) return;
            _teleportGrid.DrawPage();
        }

        internal void SetGlobalCheckBox(bool dataIsGlobal)
        {
            _isGlobalToggle.isOn = dataIsGlobal;

            if (SelectedTab == QTTabs.Global)
            {
                BaseManager.UpdateGlobalTargets();
            }
        }

        internal void RefreshTabs()
        {
            SetTab(SelectedTab);
        }

        internal void SetTab(QTTabs tab)
        {
            switch (tab)
            {
                case QTTabs.Global:
                    SelectedTab = QTTabs.Global;
                    _globalTeleBtn.Select();
                    _intraTeleBtn.DeSelect();
                    UpdateUnits();
                    break;

                case QTTabs.Intra:
                    SelectedTab = QTTabs.Intra;
                    _globalTeleBtn.DeSelect();
                    _intraTeleBtn.Select();
                    UpdateUnits();
                    break;
            }
        }

        internal QTTabs GetSelectedTab()
        {
            return SelectedTab;
        }

        internal void ShowTeleportPage(bool value)
        {
            _mono.AnimationManager.SetIntHash(_page, value ? 4 : 1);
        }
    }
}
