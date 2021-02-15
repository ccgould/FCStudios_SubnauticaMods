using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSAutoCrafterDisplay : AIDisplay
    {
        private DSSAutoCrafterController _mono;
        private string _currentSearchString;
        private GridHelperV2 _craftingGrid;
        private GridHelperV2 _productionGrid;
        private readonly List<DSSCraftingItem> _craftingButtons = new List<DSSCraftingItem>();
        private readonly List<DSSProductionItem> _productionButtons = new List<DSSProductionItem>();
        private Text _orderCount;
        private NumberPadController _numberPad;
        private GameObject _itemSelectionArea;
        private GameObject _productionStatusArea;
        private StatusController _productionStatusController;
        private GameObject _startBtn;
        private GameObject _stopBtn;
        private PaginatorController _paginatorController;
        
        internal void Setup(DSSAutoCrafterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                
            }
        }

        internal void Refresh()
        {
            _craftingGrid.DrawPage();
            _productionGrid.DrawPage();
            UpdateOrderCount();
        }

        internal StatusController GetStatusController()
        {
            return _productionStatusController;
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "PowerBTN":
                    _mono.ToggleBreaker();
                    break;

                case "StartBTN":
                    foreach (CraftingItem item in _mono.CraftingItems)
                    {
                        if (item.Amount <= 0)
                        {
                            QuickLogger.ModMessage(AuxPatchers.AmountIsZero(Language.main.Get(item.TechType)));
                            return;
                        }
                    }
                    _mono.CraftManager.StartOperation(_productionStatusController);
                    _itemSelectionArea.SetActive(false);
                    _productionStatusArea.SetActive(true);
                    _startBtn.SetActive(false);
                    _stopBtn.SetActive(true);
                    break;

                case "StopBTN":
                    foreach (DSSProductionItem button in _productionButtons)
                    {
                        button.Reset();
                    }
                    _mono.CraftManager.StopOperation();
                    _itemSelectionArea.SetActive(true);
                    _productionStatusArea.SetActive(false);
                    _startBtn.SetActive(true);
                    _stopBtn.SetActive(false);
                    _mono.CraftManager.Reset(true);
                    _productionStatusController.Reset();
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "ItemsGrid").transform)
                {
                    var invButton = invItem.gameObject.EnsureComponent<DSSCraftingItem>();
                    invButton.ButtonMode = InterfaceButtonMode.Background;
                    invButton.BtnName = "CraftingBTN";
                    invButton.OnButtonClick += OnButtonClick;
                    _craftingButtons.Add(invButton);
                }

                foreach (Transform productionItem in GameObjectHelpers.FindGameObject(gameObject, "ProductionGrid").transform)
                {
                    var productionButton = productionItem.gameObject.EnsureComponent<DSSProductionItem>();
                    _productionButtons.Add(productionButton);
                }

                _itemSelectionArea = GameObjectHelpers.FindGameObject(gameObject, "ItemSelectionArea");

                _productionStatusArea = GameObjectHelpers.FindGameObject(gameObject, "ProgressStatus");
                _productionStatusController = _productionStatusArea.AddComponent<StatusController>();

                _startBtn = GameObjectHelpers.FindGameObject(gameObject, "PlayButton");
                InterfaceHelpers.CreateButton(_startBtn, "StartBTN", InterfaceButtonMode.Background, OnButtonClick,
                    Color.gray, Color.white, MAX_INTERACTION_DISTANCE);

                _stopBtn = GameObjectHelpers.FindGameObject(gameObject, "StopButton");
                InterfaceHelpers.CreateButton(_stopBtn, "StopBTN", InterfaceButtonMode.Background, OnButtonClick,
                    Color.gray, Color.white, MAX_INTERACTION_DISTANCE);

                var powerBtn = GameObjectHelpers.FindGameObject(gameObject, "PowerBTN");
                InterfaceHelpers.CreateButton(powerBtn, "PowerBTN", InterfaceButtonMode.Background, OnButtonClick,
                    Color.white, new Color(0, 1, 1, 1), MAX_INTERACTION_DISTANCE);
                
                _craftingGrid = _mono.gameObject.AddComponent<GridHelperV2>();
                _craftingGrid.OnLoadDisplay += OnLoadCraftingGrid;
                _craftingGrid.Setup(20, gameObject, Color.gray, Color.white, OnButtonClick, "ItemsGrid");

                _orderCount = GameObjectHelpers.FindGameObject(gameObject, "OrderCount").GetComponent<Text>();
                _orderCount.text = "0/6";

                _productionGrid = _mono.gameObject.AddComponent<GridHelperV2>();
                _productionGrid.OnLoadDisplay += OnLoadProductionGrid;
                _productionGrid.Setup(6, gameObject, Color.gray, Color.white, OnButtonClick, "ProductionGrid");

                _numberPad = GameObjectHelpers.FindGameObject(gameObject, "NumberPad").AddComponent<NumberPadController>();

                _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
                _paginatorController.Initialize(this);
                #region Search
                var inputField = GameObjectHelpers.FindGameObject(gameObject, "InputField");
                var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
                text.text = AuxPatchers.SearchForItemsMessage();

                var searchField = inputField.AddComponent<SearchField>();
                searchField.OnSearchValueChanged += newSearch => {
                    _currentSearchString = newSearch;
                    _craftingGrid.DrawPage();
                };
                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message);
                QuickLogger.Debug(e.StackTrace);
                return false;
            }

            return true;
        }

        public override void GoToPage(int index)
        {
            _craftingGrid.DrawPage(index);
        }

        private void OnLoadProductionGrid(DisplayData data)
        {
            try
            {
                var grouped = _mono.CraftingItems;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage; i++)
                {
                    _productionButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _productionButtons[i].Set(grouped.ElementAt(i),this);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadCraftingGrid(DisplayData data)
        {
            try
            {
                var grouped = Mod.Craftables;

                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p).ToLower().Contains(_currentSearchString.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                QuickLogger.Debug($"Resetting crafting buttons: {data.MaxPerPage}");
                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    QuickLogger.Debug($"Resetting index {i} out of {data.MaxPerPage}");
                    _craftingButtons[i].Reset();
                }

                int w = 0;

                QuickLogger.Debug($"Setting crafting buttons: {data.MaxPerPage}");
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    QuickLogger.Debug($"Trying to set : {i}");
                    _craftingButtons[w++].Set(grouped.ElementAt(i), this);
                    QuickLogger.Debug($"Set Crafting: {i}");
                }


                _craftingGrid.UpdaterPaginator(grouped.Count);
                QuickLogger.Debug($"Max Pages: { _craftingGrid.GetMaxPages()}", true);
                _paginatorController.ResetCount(_craftingGrid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal int GetProductIndex(CraftingItem craftingItem)
        {
            return _mono.CraftingItems.IndexOf(craftingItem);
        }

        public void AddNewCraftingItem(CraftingItem craftingItem)
        {
            if(_mono.CraftingItems.Count == 6 || _mono.CraftingItems.Any(x=>x.TechType == craftingItem.TechType)) return; 
            _mono.CraftingItems.Add(craftingItem);
            _productionGrid.DrawPage();
            UpdateOrderCount();
        }

        internal void UpdateOrderCount()
        {
            _orderCount.text = $"{_mono.CraftingItems.Count}/6";
        }

        public void RemoveCraftingItem(CraftingItem craftingItem)
        {
            _mono.CraftingItems.Remove(craftingItem);
            
            foreach (DSSProductionItem item in _productionButtons)
            {
                item.UpdateIndex();
            }
            UpdateOrderCount();
        }

        public void Move(CraftingItem craftingItem, int index)
        {
            var currentIndex = _mono.CraftingItems.IndexOf(craftingItem);

            var activeItems = _productionButtons.Count(x => x.gameObject.activeSelf);

            QuickLogger.Debug($"Current Index: {currentIndex} || Going to Index: {currentIndex + index} || Active Item: {activeItems}",true);
            
            if (currentIndex + index == -1 || currentIndex + index == activeItems) return;
            _mono.CraftingItems.Move(currentIndex, currentIndex + index);
            _productionGrid.DrawPage();
        }

        public void OpenNumberPad(CraftingItem item)
        {
            _numberPad.Show(item);
        }

        public DSSAutoCrafterController GetController()
        {
            return _mono;
        }
    }
}