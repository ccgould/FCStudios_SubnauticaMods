using ExStorageDepot.Buildable;
using ExStorageDepot.Enumerators;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Linq;
using FCSCommon.Abstract;
using UnityEngine;
using UnityEngine.UI;
using InterfaceButton = FCSCommon.Components.InterfaceButton;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotDisplayManager : AIDisplay
    {
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private ExStorageDepotController _mono;
        private GameObject _storageLabels;
        private GridHelper _grid;
        private Text _plier;
        private Text _itemCount;
        private bool _isBeingDestroyed;

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

            ITEMS_PER_PAGE = 16;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                DrawPage(1);
                SetItemCount(_mono.Storage.GetTotalCount());
                //_mono.Storage.OnContainerUpdate += OnContainerUpdate;
                InvokeRepeating(nameof(OnContainerUpdate),0.5f,0.5f);
            }
            else
            {
                QuickLogger.Error("Failed to find all components for the display.");
            }

            if (mono.NameController == null)
            {
                QuickLogger.Error("Name Controller is null", true);
                return;
            }

            mono.NameController.OnLabelChanged += OnLabelChanged;

            UpdateLabels();
        }

        private void OnContainerUpdate()
        {
            QuickLogger.Debug($"Updating Container.",true);
           _grid.DrawPage();
            SetItemCount(_mono.Storage.GetTotalCount());
        }

        private void OnLabelChanged(string obj)
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (_storageLabels == null) return;

            foreach (Transform label in _storageLabels.transform)
            {
                QuickLogger.Debug("Changing label.", true);
                var text = label.GetComponent<Text>();
                text.text = _mono.NameController.GetCurrentName();
                QuickLogger.Debug($"Label {label.name} changed.", true);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "ItemBTN":
                    _mono.Storage.AttemptToTakeItem((TechType)tag);
                    break;
                case "DumpBTN":
                    QuickLogger.Debug($"In {btnName} switch", true);
                    _mono.AnimationManager.ToggleDriveState();
                    _mono.DumpContainer.OpenStorage();
                    break;

                case "RenameBTN":
                    QuickLogger.Debug($"In {btnName} switch", true);
                    _mono.NameController.Show();
                    break;
                case "LButton":
                    QuickLogger.Debug($"Current Multiplier = {_mono.BulkMultiplier}");

                    switch (_mono.BulkMultiplier)
                    {
                        case BulkMultipliers.TimesTen:
                            _mono.BulkMultiplier = BulkMultipliers.TimesEight;
                            break;
                        case BulkMultipliers.TimesEight:
                            _mono.BulkMultiplier = BulkMultipliers.TimesSix;
                            break;
                        case BulkMultipliers.TimesSix:
                            _mono.BulkMultiplier = BulkMultipliers.TimesFour;
                            break;
                        case BulkMultipliers.TimesFour:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTwo;
                            break;
                        case BulkMultipliers.TimesTwo:
                            _mono.BulkMultiplier = BulkMultipliers.TimesOne;
                            break;
                    }

                    UpdateMultiplier();
                    break;

                case "RButton":
                    QuickLogger.Debug($"Current Multiplier = {_mono.BulkMultiplier}");

                    switch (_mono.BulkMultiplier)
                    {
                        case BulkMultipliers.TimesEight:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTen;
                            break;
                        case BulkMultipliers.TimesSix:
                            _mono.BulkMultiplier = BulkMultipliers.TimesEight;
                            break;
                        case BulkMultipliers.TimesFour:
                            _mono.BulkMultiplier = BulkMultipliers.TimesSix;
                            break;
                        case BulkMultipliers.TimesTwo:
                            _mono.BulkMultiplier = BulkMultipliers.TimesFour;
                            break;
                        case BulkMultipliers.TimesOne:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTwo;
                            break;
                    }
                    UpdateMultiplier();
                    break;
            }
        }

        internal void UpdateMultiplier()
        {
            switch (_mono.BulkMultiplier)
            {
                case BulkMultipliers.TimesOne:
                    _plier.text = $"x1";
                    _mono.Storage.SetMultiplier(1);
                    break;
                case BulkMultipliers.TimesTwo:
                    _plier.text = $"x2";
                    _mono.Storage.SetMultiplier(2);
                    break;
                case BulkMultipliers.TimesFour:
                    _plier.text = $"x4";
                    _mono.Storage.SetMultiplier(4);
                    break;
                case BulkMultipliers.TimesSix:
                    _plier.text = $"x6";
                    _mono.Storage.SetMultiplier(6);
                    break;
                case BulkMultipliers.TimesEight:
                    _plier.text = $"x8";
                    _mono.Storage.SetMultiplier(8);
                    break;
                case BulkMultipliers.TimesTen:
                    _plier.text = $"x10";
                    _mono.Storage.SetMultiplier(10);
                    break;
            }
        }
        
        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas

                var canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvas == null)
                {
                    QuickLogger.Error("Canvas could not be found!");
                    return false;
                }

                #endregion

                #region Home Screen

                var home = InterfaceHelpers.FindGameObject(canvas, "Home");

                #endregion

                #region Multiplier

                var multiplier = InterfaceHelpers.FindGameObject(home, "Multiplier");

                #endregion

                #region Item Count

                _itemCount = InterfaceHelpers.FindGameObject(home, "StorageAmount")?.GetComponent<Text>();

                #endregion

                #region Plier

                _plier = InterfaceHelpers.FindGameObject(multiplier, "plier")?.GetComponent<Text>();

                #endregion

                #region Storage Labels

                _storageLabels = InterfaceHelpers.FindGameObject(home, "Storage_Labels");

                #endregion

                #region Grid

                var grid = InterfaceHelpers.FindGameObject(home, "Grid");
                _grid = _mono.gameObject.AddComponent<GridHelper>();
                _grid.OnLoadDisplay += OnLoadDisplay;
                _grid.Setup(16, ExStorageDepotBuildable.ItemPrefab, home, _startColor, _hoverColor, OnButtonClick);

                #endregion

                #region Dump Button

                var dumpBTN = InterfaceHelpers.FindGameObject(home, "Dump_Button");
                InterfaceHelpers.CreateButton(dumpBTN, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, ExStorageDepotBuildable.AddToExStorage());

                #endregion

                #region Rename Button

                var renameBTN = InterfaceHelpers.FindGameObject(home, "Rename_Button");
                InterfaceHelpers.CreateButton(renameBTN, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, ExStorageDepotBuildable.RenameStorage());

                #endregion
                
                #region Multiplier Previous Button

                var multiplierPrevBtn = InterfaceHelpers.FindGameObject(multiplier, "Prev_BTN");
                InterfaceHelpers.CreateButton(multiplierPrevBtn, "LButton", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "");

                #endregion

                #region Multiplier Next Button

                var multiplierNextBtn = InterfaceHelpers.FindGameObject(multiplier, "Next_BTN");
                InterfaceHelpers.CreateButton(multiplierNextBtn, "RButton", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "");

                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}:\n{e.StackTrace}");
                return false;
            }
            return true;
        }

        private void OnLoadDisplay(DisplayData data)
        {
            if (_isBeingDestroyed) return;

            _grid.ClearPage();

            var grouped = _mono.Storage.GetItemsWithin();
            
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
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                var item = grouped.ElementAt(i);
                LoadDisplay(data,item.Key, item.Value);
            }
            _grid.UpdaterPaginator(grouped.Count);
        }
        
        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.ToggleScreenState();

        }
        
        private void LoadDisplay(DisplayData data, TechType elementTechType, int techTypeCount)
        {
            GameObject buttonPrefab = Instantiate(ExStorageDepotBuildable.ItemPrefab);
            buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
            var mainBtn = buttonPrefab.AddComponent<InterfaceButton>();
            var text = buttonPrefab.GetComponentInChildren<Text>();
            text.text = techTypeCount.ToString();
            mainBtn.ButtonMode = InterfaceButtonMode.Background;
            mainBtn.STARTING_COLOR = _startColor;
            mainBtn.HOVER_COLOR = _hoverColor;
            mainBtn.TextLineOne = string.Format(ExStorageDepotBuildable.TakeItemFormat(), Language.main.Get(elementTechType));
            mainBtn.OnButtonClick = OnButtonClick;
            mainBtn.BtnName = "ItemBTN";
            mainBtn.Tag = elementTechType;
            uGUI_Icon icon = buttonPrefab.transform.Find("Image").gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(elementTechType);
        }

        internal void SetItemCount(int count)
        {
            _itemCount.text = $"Item Count: {count}/{QPatch.Config.MaxStorage}";
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }
    }
}
