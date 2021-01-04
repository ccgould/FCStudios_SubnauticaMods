using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
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
        private readonly ObservableCollection<CraftingItem> _craftingItems = new ObservableCollection<CraftingItem>();
        private Text _orderCount;
        private NumberPadController _numberPad;
        private GameObject _itemSelectionArea;
        private GameObject _productionStatusArea;
        private StatusController _productionStatusController;

        internal void Setup(DSSAutoCrafterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                
            }
        }

        internal void RefreshCraftables()
        {
            _craftingGrid.DrawPage();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "PowerBTN":
                    _mono.ToggleBreaker();
                    break;

                case "StartButton":
                    _mono.CraftManager.StartOperation();
                    _itemSelectionArea.SetActive(false);
                    _productionStatusArea.SetActive(true);
                    break;

                case "StopButton":
                    _mono.CraftManager.StopOperation();
                    _itemSelectionArea.SetActive(true);
                    _productionStatusArea.SetActive(false);
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

                var startBtn = GameObjectHelpers.FindGameObject(gameObject, "PlayButton");
                InterfaceHelpers.CreateButton(startBtn, "StartBTN", InterfaceButtonMode.Background, OnButtonClick,
                    Color.gray, Color.white, MAX_INTERACTION_DISTANCE);

                var stopBtn = GameObjectHelpers.FindGameObject(gameObject, "StopBTN");
                InterfaceHelpers.CreateButton(stopBtn, "StopBTN", InterfaceButtonMode.Background, OnButtonClick,
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

        private void OnLoadProductionGrid(DisplayData data)
        {
            try
            {
                var grouped = _craftingItems;

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
                    grouped = grouped.Where(p => Language.main.Get(p).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage; i++)
                {
                    _craftingButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _craftingButtons[i].Set(grouped.ElementAt(i),this);
                }
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
            return _craftingItems.IndexOf(craftingItem);
        }

        public void AddNewCraftingItem(CraftingItem craftingItem)
        {
            if(_craftingItems.Any(x=>x.TechType == craftingItem.TechType)) return; 
            _craftingItems.Add(craftingItem);
            _productionGrid.DrawPage();
            UpdateOrderCount();
        }

        private void UpdateOrderCount()
        {
            _orderCount.text = $"{_craftingItems.Count}/6";
        }

        public void RemoveCraftingItem(CraftingItem craftingItem)
        {
            _craftingItems.Remove(craftingItem);
            _productionGrid.DrawPage();
            foreach (DSSProductionItem item in _productionButtons)
            {
                item.UpdateIndex();
            }
            UpdateOrderCount();
        }

        public void Move(CraftingItem craftingItem, int index)
        {
            var currentIndex = _craftingItems.IndexOf(craftingItem);

            var activeItems = _productionButtons.Count(x => x.gameObject.activeSelf);

            QuickLogger.Debug($"Current Index: {currentIndex} || Going to Index: {currentIndex + index} || Active Item: {activeItems}",true);
            
            if (currentIndex + index == -1 || currentIndex + index == activeItems) return;
            _craftingItems.Move(currentIndex, currentIndex + index);
            _productionGrid.DrawPage();
        }

        public void OpenNumberPad(CraftingItem item)
        {
            _numberPad.Show(item);
        }
    }

    internal class StatusController : MonoBehaviour
    {
        private uGUI_Icon _slot1Icon;
        private Text _slot1Text;
        private uGUI_Icon _slot2Icon;
        private Text _slot2Text;

        private void Start()
        {
            _slot1Icon = gameObject.FindChild("ProductSlot1").FindChild("Icon").AddComponent<uGUI_Icon>();
            _slot1Text = gameObject.FindChild("ProductSlot1").FindChild("ItemCount").AddComponent<Text>();

            _slot2Icon = gameObject.FindChild("ProductSlot2").FindChild("Icon").AddComponent<uGUI_Icon>();
            _slot2Text = gameObject.FindChild("ProductSlot2").FindChild("ItemCount").AddComponent<Text>();
        }
    }
}