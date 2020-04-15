using System;
using System.Collections.Generic;
using System.Linq;
using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Display;
using ARS_SeaBreezeFCS32.Model;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezeDisplay : AIDisplay
    {
        private ARSolutionsSeaBreezeController _mono;
        private ColorManager _colorPage;
        private readonly Color _startColor = Color.white;
        private readonly Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private Text _itemCounter_LBL;
        private Text _seaBreeze_LBL;
        private GridHelper _foodPage;
        private GridHelper _waterPage;
        private GridHelper _trashPage;
        private Color colorEmpty = new Color(1f, 0f, 0f, 1f);
        private Color colorHalf = new Color(1f, 1f, 0f, 1f);
        private Color colorFull = new Color(0f, 1f, 0f, 1f);
        private Text _batteryPercent;
        private Image _batteryFill;

        internal void Setup(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;
            _colorPage = mono.ColorManager;

            if (FindAllComponents())
            {
                _mono.AnimationManager.SetIntHash(_mono.PageStateHash,1);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PPBtn":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 1);
                    _mono.ToggleBreaker();
                    break;

                case "HPPBtn":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 3);
                    _mono.ToggleBreaker();
                    break;

                case "RenameBTN":
                    _mono.NameController.Show();
                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColor((Color) tag);
                    break;
                case "HomeBTN":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 2);
                    break;
                case "FoodCBTN":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 7);
                    break;
                case "WaterCBTN":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 5);
                    break;
                case "TrashBTN":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 6);
                    break;
                case "DumpBTN":
                    _mono.FridgeDumpContainer.OpenStorage();
                    break;
                case "ColorBTN":
                    _mono.AnimationManager.SetIntHash(_mono.PageStateHash, 4);
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
                    QuickLogger.Error("Canvas not found.");
                    return false;
                }

                #endregion

                #region Home
                var home = InterfaceHelpers.FindGameObject(canvasGameObject, "HomeScreen");
                #endregion

                #region Battery

                var homeBattery = InterfaceHelpers.FindGameObject(home, "Battery");
                _batteryPercent = InterfaceHelpers.FindGameObject(homeBattery, "Text").GetComponent<Text>();
                _batteryFill = InterfaceHelpers.FindGameObject(homeBattery, "Fill").GetComponent<Image>();

                #endregion

                #region Food
                var food = InterfaceHelpers.FindGameObject(canvasGameObject, "FoodScreen");
                #endregion

                #region Drinks
                var drinks = InterfaceHelpers.FindGameObject(canvasGameObject, "DrinksScreen");
                #endregion

                #region Trash
                var trash = InterfaceHelpers.FindGameObject(canvasGameObject, "TrashScreen");
                #endregion

                #region Color Picker
                var colorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPicker");
                #endregion

                #region PowerOff
                var powerOff = InterfaceHelpers.FindGameObject(canvasGameObject, "PoweredOffScreen");
                #endregion

                #region PowerButton
                var powerBtn = InterfaceHelpers.FindGameObject(home, "Power_BTN");

                InterfaceHelpers.CreateButton(powerBtn, "HPPBtn", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.PowerBTNMessage());
                #endregion

                #region PowerOFf PowerButton
                var ppowerBtn = InterfaceHelpers.FindGameObject(powerOff, "Power_BTN");

                InterfaceHelpers.CreateButton(ppowerBtn, "PPBtn", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.PowerBTNMessage());
                #endregion

                #region PowerOFf PowerButton
                var powerOffLbl = InterfaceHelpers.FindGameObject(powerOff, "Powered_Off_LBL");
                powerOffLbl.GetComponent<Text>().text = ARSSeaBreezeFCS32Buildable.NoPower();
                #endregion

                #region DumpBTNButton
                var dumpBtn = InterfaceHelpers.FindGameObject(home, "DumpBTN");

                InterfaceHelpers.CreateButton(dumpBtn, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.DumpButton(), ARSSeaBreezeFCS32Buildable.DumpMessage());
                #endregion

                #region FoodCButton
                var foodContainterBtn = InterfaceHelpers.FindGameObject(home, "FoodCBTN");

                InterfaceHelpers.CreateButton(foodContainterBtn, "FoodCBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.FoodCButton());
                #endregion

                #region WaterCButton
                var WaterContainterBtn = InterfaceHelpers.FindGameObject(home, "WaterCBTN");

                InterfaceHelpers.CreateButton(WaterContainterBtn, "WaterCBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.WaterCButton());
                #endregion

                #region Rename Button
                var RenameBtn = InterfaceHelpers.FindGameObject(home, "RenameBTN");

                InterfaceHelpers.CreateButton(RenameBtn, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.RenameButton());
                #endregion

                #region Trash Button
                var TrashBtn = InterfaceHelpers.FindGameObject(home, "TrashCBTN");

                InterfaceHelpers.CreateButton(TrashBtn, "TrashBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.TrashButton(),ARSSeaBreezeFCS32Buildable.TrashMessage());
                #endregion

                #region ColorBTN Button
                var colorBtn = InterfaceHelpers.FindGameObject(home, "ColorPickerBTN");

                InterfaceHelpers.CreateButton(colorBtn, "ColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, ARSSeaBreezeFCS32Buildable.ColorPicker());
                #endregion

                #region ColorPage
                _colorPage.SetupGrid(48, ARSSeaBreezeFCS32Buildable.ColorItemPrefab, colorPicker, OnButtonClick, _startColor,_hoverColor);
                #endregion

                #region Food Page

                _foodPage = _mono.gameObject.AddComponent<GridHelper>();
                _foodPage.OnLoadDisplay += OnLoadFoodDisplay;
                _foodPage.Setup(17, ARSSeaBreezeFCS32Buildable.ItemPrefab,food,_startColor,_hoverColor,OnButtonClick);

                #endregion

                #region Drink Page

                _waterPage = _mono.gameObject.AddComponent<GridHelper>();
                _waterPage.OnLoadDisplay += OnLoadWaterDisplay;
                _waterPage.Setup(17, ARSSeaBreezeFCS32Buildable.ItemPrefab, drinks, _startColor, _hoverColor, OnButtonClick);

                #endregion

                #region Trash Page

                _trashPage = _mono.gameObject.AddComponent<GridHelper>();
                _trashPage.OnLoadDisplay += OnLoadTrashDisplay;
                _trashPage.Setup(17, ARSSeaBreezeFCS32Buildable.ItemPrefab, trash, _startColor, _hoverColor, OnButtonClick);

                #endregion

                #region StorageAmount

                _itemCounter_LBL = InterfaceHelpers.FindGameObject(home,"ItemCounter_LBL").GetComponent<Text>();

                #endregion

                #region Unit Name

                _seaBreeze_LBL = InterfaceHelpers.FindGameObject(home, "SeaBreeze_LBL").GetComponent<Text>();

                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}:\n{e.StackTrace}");
                return false;
            }

            return true;
        }

        private void OnLoadFoodDisplay(DisplayData data)
        {
            OnLoadDisplay( EatableType.Food, data.ItemsPrefab, data.ItemsGrid, data.StartPosition, data.EndPosition);
        }

        private void OnLoadTrashDisplay(DisplayData data)
        {
            OnLoadDisplay(EatableType.Rotten, data.ItemsPrefab, data.ItemsGrid, data.StartPosition, data.EndPosition);
        }

        private void OnLoadWaterDisplay(DisplayData data)
        {
            OnLoadDisplay(EatableType.Drink, data.ItemsPrefab, data.ItemsGrid, data.StartPosition, data.EndPosition);
        }

        private void OnLoadDisplay(EatableType eatableType, GameObject itemPrefab, GameObject itemsGrid, int stPos, int endPos)
        {
            switch (eatableType)
            {
                case EatableType.Rotten:
                    var rottenList = _mono.FridgeComponent.FridgeItems.Where(x => x.IsRotten()).ToList();
                    _trashPage.ClearPage();
                    CreateFoodItem(itemPrefab, itemsGrid, stPos, endPos, rottenList, _trashPage, EatableType.Rotten);
                    break;

                case EatableType.Food:
                    var freshList = _mono.FridgeComponent.FridgeItems.Where(x => x.GetFoodValue() > 0 && !x.IsRotten()).ToList();
                    _foodPage.ClearPage();
                    CreateFoodItem(itemPrefab, itemsGrid, stPos, endPos, freshList, _foodPage, EatableType.Food);
                    break;

                case EatableType.Drink:
                    var drinkList = _mono.FridgeComponent.FridgeItems.Where(x => x.GetFoodValue() <= 0 && x.GetWaterValue() > 0).ToList();
                    _waterPage.ClearPage();
                    CreateFoodItem(itemPrefab, itemsGrid, stPos, endPos, drinkList, _waterPage, EatableType.Drink);
                    break;
            }
        }
        
        private void CreateFoodItem(GameObject itemPrefab, GameObject itemsGrid, int stPos, int endPos,List<EatableEntities> list, GridHelper page, EatableType eatableType)
        {
            var grouped = list.GroupBy(x => x.TechType).Select(x => x.First()).ToList();

            if (endPos > grouped.Count)
            {
                endPos = grouped.Count;
            }

            for (int i = stPos; i < endPos; i++)
            {
                var techType = grouped[i].TechType;

                GameObject foodIcon = Instantiate(itemPrefab);

                if (foodIcon == null || itemsGrid == null)
                {
                    if (foodIcon != null)
                    {
                        Destroy(foodIcon);
                    }
                    return;
                }

                foodIcon.transform.SetParent(itemsGrid.transform, false);

                foodIcon.GetComponentInChildren<Text>().text = "x" + list.Count(x => x.TechType == techType);

                var itemButton = foodIcon.AddComponent<ItemButton>();
                itemButton.Type = techType;
                itemButton.EatableType = eatableType;
                itemButton.OnButtonClick = _mono.FridgeComponent.RemoveItem;

                uGUI_Icon icon = InterfaceHelpers.FindGameObject(foodIcon, "ItemImage").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(techType);
            }

            page.UpdaterPaginator(grouped.Count);
        }
        
        internal void UpdateScreenLabel(string name, NameController nameController)
        {
            QuickLogger.Debug($"Label set to {name}", true);
            _seaBreeze_LBL.text = name;
        }

        internal void OnContainerUpdate(int numberofItems, int storageLimit)
        {
            _itemCounter_LBL.text = $"{numberofItems}/{storageLimit} {ARSSeaBreezeFCS32Buildable.Items()}";
            UpdateContainers();
        }

        internal void UpdateVisuals(PowercellData data)
        {
            var charge = data.GetCharge() < 1 ? 0f : data.GetCapacity();

            float percent = charge / data.GetCapacity();

            QuickLogger.Debug($"Percent: {percent}");
            
            if (_batteryPercent != null)
            {
                _batteryPercent.text = ((data.GetCharge() < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
            }

            if (_batteryFill != null)
            {
                if (data.GetCharge() >= 0f)
                {
                    Color value = (percent >= 0.5f) ? Color.Lerp(this.colorHalf, this.colorFull, 2f * percent - 1f) : Color.Lerp(this.colorEmpty, this.colorHalf, 2f * percent);
                    _batteryFill.color = value;
                    _batteryFill.fillAmount = percent;
                }
                else
                {
                    _batteryFill.color = colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }
            }
        }

        internal void EmptyBatteryVisual()
        {
            if (_batteryPercent != null)
            {
                _batteryPercent.text = Language.main.Get("ChargerSlotEmpty");
            }

            if (_batteryFill != null)
            {
                _batteryFill.color = colorEmpty;
                _batteryFill.fillAmount = 0f;
            }
        }

        private void UpdateContainers()
        {
            _foodPage.DrawPage();
            _waterPage.DrawPage();
            _trashPage.DrawPage();
        }
    }
}