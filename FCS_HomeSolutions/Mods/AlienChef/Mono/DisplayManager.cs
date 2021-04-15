using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Converters;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class DisplayManager : AIDisplay
    {
        private AlienChefController _mono;
        private GameObject _grid;
        private CookerItemDialog _cookerItemDialog;
        private Image _cookingPercentageRing;
        private Text _cookingTime;
        private Text _foodSectionLabel;
        private Toggle _pullFromDataStorage;
        private Toggle _sendToSeaBreeze;
        private FoodQueueList _listBTN;
        private OrderWindowDialog _orderWindow;
        private Text _inventoryAmount;
        private GridHelperV2 _itemGrid;
        private List<CookerItemController> _cookerItemControllers = new List<CookerItemController>();
        private PaginatorController _paginatorController;

        internal void Setup(AlienChefController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                _itemGrid.DrawPage();
                LoadKnownCookedItems();
            }
        }

        private void LoadKnownCookedItems()
        {
            if (_mono.Cooker.GetMode() == CookerMode.Cook)
            {

                _foodSectionLabel.text = AuxPatchers.CookedFoods();
            }
            else if(_mono.Cooker.GetMode() == CookerMode.Cure)
            {
                _foodSectionLabel.text = AuxPatchers.CuredFoods();
            }
            else if (_mono.Cooker.GetMode() == CookerMode.Custom)
            {
               _foodSectionLabel.text = AuxPatchers.CustomFoods();
            }

        }

        public override void OnButtonClick(string btnName, object tag)
        {

        }

        public override bool FindAllComponents()
        {
            try
            {
                _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid");
                _cookerItemDialog = GameObjectHelpers.FindGameObject(gameObject, "SelectionWindow").EnsureComponent<CookerItemDialog>();
                _cookerItemDialog.Setup(_mono);
                _cookingPercentageRing = GameObjectHelpers.FindGameObject(gameObject, "Preloader").GetComponent<Image>();
                _cookingTime = GameObjectHelpers.FindGameObject(gameObject, "Time").GetComponent<Text>();
                _orderWindow = GameObjectHelpers.FindGameObject(gameObject, "OrderWindow").EnsureComponent<OrderWindowDialog>();
                _orderWindow.Initialize(_mono);
                _foodSectionLabel = GameObjectHelpers.FindGameObject(gameObject, "CookedFoodLabel").GetComponent<Text>();
                var home = GameObjectHelpers.FindGameObject(gameObject, "Home");

                foreach (Transform cookingItem in _grid.transform)
                {
                    var controller = cookingItem.gameObject.AddComponent<CookerItemController>();
                    controller.Initialize();
                    _cookerItemControllers.Add(controller);
                }

                _paginatorController = GameObjectHelpers.FindGameObject(home, "Paginator").AddComponent<PaginatorController>();
                _paginatorController.Initialize(this);

                _itemGrid = _mono.gameObject.AddComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(18, home, Color.gray, Color.white, null);

                _pullFromDataStorage = GameObjectHelpers.FindGameObject(gameObject, "DataStorageToggle").GetComponent<Toggle>();
                _pullFromDataStorage.onValueChanged.AddListener((state =>
                {
                    _mono.PullFromDataStorage = state;
                }));
                
                _sendToSeaBreeze = GameObjectHelpers.FindGameObject(gameObject, "SeaBreezeExport").GetComponent<Toggle>();
                _sendToSeaBreeze.onValueChanged.AddListener((state =>
                {
                    _mono.IsSendingToSeaBreeze = state;
                }));

                _listBTN = GameObjectHelpers.FindGameObject(gameObject, "ListBTN").EnsureComponent<FoodQueueList>();
                _listBTN.Initialize(_orderWindow);
                _listBTN.TextLineOne = "Orders";

                var cookToggle = GameObjectHelpers.FindGameObject(gameObject, "CookBTN");
                var cookBTNToggle = cookToggle.GetComponent<Toggle>();
                var cookFCSButton = cookToggle.AddComponent<FCSButton>();
                cookFCSButton.TextLineOne = "Cooked Items";

                cookBTNToggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        OnToggleButtonAction("ToggleButton_0");
                    }
                }));

                var cureToggle = GameObjectHelpers.FindGameObject(gameObject, "CureBTN");
                var cureBTNToggle = cureToggle.GetComponent<Toggle>();
                var cureFCSButton = cureToggle.AddComponent<FCSButton>();
                cureFCSButton.TextLineOne = "Cured Items";
                cureBTNToggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        OnToggleButtonAction("ToggleButton_1");
                    }
                }));

                var customToggle = GameObjectHelpers.FindGameObject(gameObject, "CustomFoodBTN");
                var customBTNToggle = customToggle.GetComponent<Toggle>();
                var customFCSButton = customToggle.AddComponent<FCSButton>();
                customFCSButton.TextLineOne = "Other Items";
                customBTNToggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        OnToggleButtonAction("ToggleButton_2");
                    }
                }));

                var inventoryGBTN = GameObjectHelpers.FindGameObject(gameObject, "InventoryBTN");
                _inventoryAmount = inventoryGBTN.GetComponentInChildren<Text>();
                var inventoryBTN = inventoryGBTN.AddComponent<InterfaceButton>();
                inventoryBTN.TextLineOne = "Storage";
                inventoryBTN.BtnName = "InventoryBTN";
                inventoryBTN.OnButtonClick += (s, o) =>
                {
                    Player main = Player.main;
                    PDA pda = main.GetPDA();
                    if (pda != null)
                    {
                        Inventory.main.SetUsedStorage(_mono.StorageSystem.ItemsContainer);
                        pda.Open(PDATab.Inventory, null, null, 4f);
                    }
                };


            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.Source);
                QuickLogger.Error(e.InnerException);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                Dictionary<TechType,TechType> grouped = null;
                
                if (_mono.Cooker.GetMode() == CookerMode.Cook)
                {
                    grouped = CraftData.cookedCreatureList;
                }
                else if (_mono.Cooker.GetMode() == CookerMode.Cure)
                {
                    grouped = Mod.CuredCreatureList;
                }
                else if (_mono.Cooker.GetMode() == CookerMode.Custom)
                {
                    grouped = Mod.CustomFoods;
                }
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _cookerItemControllers[i].Reset();
                }

                if (grouped == null) return;

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _cookerItemControllers[w++].Set(new CookingItem{TechType = grouped.ElementAt(i).Key,ReturnItem = grouped.ElementAt(i).Value,CookerMode = _mono.Cooker.GetMode()}, _cookerItemDialog);
                }

                _itemGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_itemGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnToggleButtonAction(string button)
        {
            if (button.Equals("ToggleButton_0"))
            {
                _mono.Cooker.ChangeMode(CookerMode.Cook);
            }
            
            if (button.Equals("ToggleButton_1"))
            {
                _mono.Cooker.ChangeMode(CookerMode.Cure);
            }

            if (button.Equals("ToggleButton_2"))
            {
                _mono.Cooker.ChangeMode(CookerMode.Custom);
            }

            _itemGrid.DrawPage(1);
            LoadKnownCookedItems();
        }

        public void UpdateStorageAmount(int amount)
        {
            QuickLogger.Debug("Updating Storage Amount",true);
            _inventoryAmount.text = amount.ToString();
        }

        public void UpdatePercentage(float process)
        {
            _cookingPercentageRing.fillAmount = process;
        }

        public void UpdateCookingTime(float cookingTime)
        {
            _cookingTime.text = TimeConverters.SecondsToMS(cookingTime);
        }

        public void AddToOrder(CookerItemController cookerItemDialog, int amount)
        {
            _orderWindow.AddItemToList(cookerItemDialog,amount);
        }

        public override void GoToPage(int index)
        {
            _itemGrid.DrawPage(index);
        }
    }
}
