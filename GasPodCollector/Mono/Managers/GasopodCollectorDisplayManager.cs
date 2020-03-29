using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Managers;
using GasPodCollector.Buildables;
using GasPodCollector.Models;
using UnityEngine;
using UnityEngine.UI;

namespace GasPodCollector.Mono.Managers
{
    internal class GasopodCollectorDisplayManager : AIDisplay
    {
        private GaspodCollectorController _mono;
        private Text _amountOfPodsCount;
        private readonly Color _startColor = Color.white;
        private readonly Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private readonly Color _fireBrickColor = new Color(0.6980392f, 0.1333333f, 0.1333333f, 1f);
        private readonly Color  _greenColor = new Color(0.03424335f, 1f, 0f, 1f);
        private ColorManager _colorPage;
        private Image _b1Fill;
        private Image _b2Fill;
        private Text _b1Amount;
        private Text _b2Amount;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private readonly string _empty = Language.main.Get("ChargerSlotEmpty");
        internal void Setup(GaspodCollectorController mono)
        {
            _mono = mono;
            _colorPage = mono.ColorManager;
            if (FindAllComponents())
            {
                _mono.GaspodCollectorStorage.OnAmountChanged += OnStorageAmountChange;
                _amountOfPodsCount.text = $"0/{QPatch.Configuration.Config.StorageLimit}";
                UpdateBatteries(null);
            }
        }

        internal void OnStorageAmountChange(int amount)
        {
            _amountOfPodsCount.text = $"{amount}/{QPatch.Configuration.Config.StorageLimit}";
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "GasPodBTN":
                    _mono.GaspodCollectorStorage.RemoveGaspod();
                    break;
                case "ColorBTN":
                    _mono.ChangePage(2);
                    break;
                case "HomeBTN":
                    _mono.ChangePage(1);
                    break;
                case "BatteryBTN":
                    _mono.PowerManager.OpenEquipment();
                    break;
                case "DumpBTN":
                    _mono.GaspodCollectorStorage.DumpToPlayer();
                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColor((Color)tag);
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
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Home
                var home = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region Amount
                _amountOfPodsCount = InterfaceHelpers.FindGameObject(home, "AmountOfPodsCount")?.GetComponent<Text>();
                #endregion

                #region GasPodButton
                var gasPodButtonObj = InterfaceHelpers.FindGameObject(home, "GasPodButton");

                InterfaceHelpers.CreateButton(gasPodButtonObj, "GasPodBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, GaspodCollectorBuildable.TakeGaspod());

                uGUI_Icon icon = gasPodButtonObj.transform.Find("Image").gameObject.AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(TechType.GasPod);
                #endregion

                #region DumpBTNButton
                var dumpBTNButtonObj = InterfaceHelpers.FindGameObject(home, "DumpBTN");

                InterfaceHelpers.CreateButton(dumpBTNButtonObj, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE,GaspodCollectorBuildable.DumpPull(), GaspodCollectorBuildable.DumpMessage());
                #endregion

                #region Messages
                InterfaceHelpers.FindGameObject(home, "AmountOfPods").GetComponent<Text>().text = GaspodCollectorBuildable.AmountOfPodsMessage();
                InterfaceHelpers.FindGameObject(home, "ClickToTake").GetComponent<Text>().text = GaspodCollectorBuildable.InstructionsMessage();
                InterfaceHelpers.FindGameObject(home, "Battery (1)").FindChild("Battery Label").GetComponent<Text>().text = $"{GaspodCollectorBuildable.Battery()} 1";
                InterfaceHelpers.FindGameObject(home, "Battery (2)").FindChild("Battery Label").GetComponent<Text>().text = $"{GaspodCollectorBuildable.Battery()} 2";
                #endregion

                #region ColorPicker Button

                var colorBTN = InterfaceHelpers.FindGameObject(home, "ColorBTN");

                InterfaceHelpers.CreateButton(colorBTN, "ColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, GaspodCollectorBuildable.ColorPicker());

                #endregion

                #region Battery Button

                var batteryBTN = InterfaceHelpers.FindGameObject(home, "BatteryBTN").FindChild("Fill");

                InterfaceHelpers.CreateButton(batteryBTN, "BatteryBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _greenColor, _fireBrickColor, MAX_INTERACTION_DISTANCE, GaspodCollectorBuildable.BatteryReceptacle());

                #endregion

                #region ColorPicker

                var colorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPicker");
                #endregion

                #region Color Paginator
                var _colorPaginator = InterfaceHelpers.FindGameObject(colorPicker, "Paginator");
                #endregion

                #region Color Grid
                var _colorGrid = InterfaceHelpers.FindGameObject(colorPicker, "Grid");
                #endregion

                #region ColorPage
                _colorPage.SetupGrid(66, GaspodCollectorBuildable.ColorItemPrefab, _colorGrid, _colorPaginator.GetComponent<Text>(), OnButtonClick);
                #endregion

                #region Prev Color Button
                var prevColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "PrevBTN");

                InterfaceHelpers.CreatePaginator(prevColorBtn, -1, _colorPage.ChangeColorPageBy, _startColor, _hoverColor);
                #endregion

                #region Next Color Button
                var nextColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "NextBTN");

                InterfaceHelpers.CreatePaginator(nextColorBtn, 1, _colorPage.ChangeColorPageBy, _startColor, _hoverColor);
                #endregion

                #region HomeButton
                var homeBTN = InterfaceHelpers.FindGameObject(colorPicker, "HomeBTN");

                InterfaceHelpers.CreateButton(homeBTN, "HomeBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, GaspodCollectorBuildable.GoHome());

                #endregion

                #region Batteries

                var b1 = InterfaceHelpers.FindGameObject(home, "Battery (1)");
                var b2 = InterfaceHelpers.FindGameObject(home, "Battery (2)");

                _b1Fill = InterfaceHelpers.FindGameObject(b1, "Fill")?.GetComponent<Image>();
                _b2Fill = InterfaceHelpers.FindGameObject(b2, "Fill")?.GetComponent<Image>();

                _b1Amount = InterfaceHelpers.FindGameObject(b1, "Amount")?.GetComponent<Text>();
                _b2Amount = InterfaceHelpers.FindGameObject(b2, "Amount")?.GetComponent<Text>();

                #endregion

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error<GasopodCollectorDisplayManager>($"{e.Message}\n{e.StackTrace}");
                return false;
            }
        }


        internal void UpdateBatteries(Dictionary<int, BatteryInfo> batteries)
        {
            //Set Percentages
            if (batteries == null)
            {

                _b1Amount.text = _empty;
                _b2Amount.text = _empty;

                _b1Fill.fillAmount = 0f;
                _b2Fill.fillAmount = 0f;
                return;
            }

            ManageBattery(_b1Fill, batteries[0], _b1Amount);
            ManageBattery(_b2Fill, batteries[1], _b2Amount);
        }

        private string FloatToPercent(BatteryInfo batteryInfo)
        {
            if (batteryInfo == null) return "0%";

            var charge = batteryInfo.BatteryCharge < 1 ? 0f : batteryInfo.BatteryCharge;

            float percent = charge / batteryInfo.BatteryCapacity;

            return ((batteryInfo.BatteryCharge < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.RoundToInt(percent * 100)}%");
        }

        private void ManageBattery(Image bar, BatteryInfo batteryInfo, Text amount)
        {
            if (batteryInfo == null && bar != null)
            {
                amount.text = _empty;
                bar.color = _colorEmpty;
                bar.fillAmount = 0f;
                return;
            }

            amount.text = FloatToPercent(batteryInfo);

            if (bar == null)
            {
                QuickLogger.Error("Bar is null");
                return;
            }

            var charge = batteryInfo.BatteryCharge < 1 ? 0f : batteryInfo.BatteryCharge;

            float percent = Mathf.RoundToInt(charge / batteryInfo.BatteryCapacity);

            if (batteryInfo.BatteryCharge >= 0f)
            {
                Color value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
                bar.color = value;
                bar.fillAmount = percent;
            }
            else
            {
                bar.color = _colorEmpty;
                bar.fillAmount = 0f;
            }

        }
    }
}