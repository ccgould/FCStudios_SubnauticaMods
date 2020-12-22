using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Components;
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
        private FCSToggleGroup _toggleGroup;
        private Text _foodSectionLabel;
        private FCSToggleButton _pullFromDataStorage;
        private FCSToggleButton _sendToSeaBreeze;
        private FoodQueueList _listBTN;
        private OrderWindowDialog _orderWindow;

        internal void Setup(AlienChefController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                LoadKnownCookedItems();
                _toggleGroup.Select("ButtonToggle_1");
            }
        }

        private void LoadKnownCookedItems()
        {
            if (_grid == null || _mono?.Cooker == null) return;
            for (int i = 0; i < _grid?.transform?.childCount; i++)
            {
                var item = _grid?.transform?.GetChild(i)?.gameObject;

                if (item != null)
                {
                    Destroy(item);
                }
            }
            if (_mono.Cooker.GetMode() == CookerMode.Cook || _mono.Cooker.GetMode() == CookerMode.Custom)
            {
                foreach (KeyValuePair<TechType, TechType> cookingPair in CraftData.cookedCreatureList)
                {
                    var newItem = GameObject.Instantiate(ModelPrefab.CookerItemPrefab);
                    var controller = newItem.EnsureComponent<CookerItemController>();
                    controller.Initialize(cookingPair, _cookerItemDialog, _mono.Cooker.GetMode());
                    newItem.transform.SetParent(_grid.transform, false);
                }

                _foodSectionLabel.text = AuxPatchers.CookedFoods();
            }
            else if(_mono.Cooker.GetMode() == CookerMode.Cure)
            {
                foreach (KeyValuePair<TechType, TechType> cookingPair in Mod.CuredCreatureList)
                {
                    var newItem = GameObject.Instantiate(ModelPrefab.CookerItemPrefab);
                    var controller = newItem.EnsureComponent<CookerItemController>();
                    controller.Initialize(cookingPair, _cookerItemDialog, _mono.Cooker.GetMode());
                    newItem.transform.SetParent(_grid.transform, false);
                }
                _foodSectionLabel.text = AuxPatchers.CuredFoods();
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
                _toggleGroup = GameObjectHelpers.FindGameObject(gameObject, "ToggleGroup").EnsureComponent<FCSToggleGroup>();
                _orderWindow = GameObjectHelpers.FindGameObject(gameObject, "OrderWindow").EnsureComponent<OrderWindowDialog>();
                _orderWindow.Initialize(_mono);
                _toggleGroup.OnToggleButtonAction += OnToggleButtonAction;
                _foodSectionLabel = GameObjectHelpers.FindGameObject(gameObject, "CookedFoodLabel").GetComponent<Text>();
                _pullFromDataStorage = GameObjectHelpers.FindGameObject(gameObject, "DataStorageToggle").EnsureComponent<FCSToggleButton>();
                _sendToSeaBreeze = GameObjectHelpers.FindGameObject(gameObject, "SeaBreezeExport").EnsureComponent<FCSToggleButton>();
                _listBTN = GameObjectHelpers.FindGameObject(gameObject, "ListBTN").EnsureComponent<FoodQueueList>();
                _listBTN.Initialize(_orderWindow);
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

            LoadKnownCookedItems();
        }

        public void UpdatePercentage(float process)
        {
            _cookingPercentageRing.fillAmount = process;
        }

        public void UpdateCookingTime(float cookingTime)
        {
            _cookingTime.text = TimeConverters.SecondsToMS(cookingTime);
        }

        public void AddToOrder(CookerItemController cookerItemDialog)
        {
            _orderWindow.AddItemToList(cookerItemDialog);
        }
    }
}
