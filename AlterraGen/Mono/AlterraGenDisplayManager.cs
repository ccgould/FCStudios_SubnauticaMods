using System;
using System.Linq;
using AlterraGen.Buildables;
using AlterraGen.Enumerators;
using AlterraGen.Extensions;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.UI;

namespace AlterraGen.Mono
{
    internal class AlterraGenDisplayManager : AIDisplay
    {
        private AlterraGenController _mono;
        private Image _batteryFill;
        private Text _powerStateValue;
        private Text _powerUnitValue;
        private Text _batteryPercentageValue;
        private Image _breakerStatusLight;
        private ColorManager _colorPage;
        private int _pageHash;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private GridHelper _grid;
        private bool _isBeingDestroyed;
        private int _isOperational;

        internal void Setup(AlterraGenController mono)
        {
            _mono = mono;
            _colorPage = mono.ColorManager;
            _isOperational = Animator.StringToHash("IsOperational");
            mono.PowerManager.OnContainerUpdate += OnContainerUpdate;

            if (FindAllComponents())
            {
                _pageHash = Animator.StringToHash("Page");
                _mono.PowerManager.OnPowerUpdateCycle += OnPowerUpdateCycle;
                _grid.DrawPage(1);
                GotoPage(AlterraGenPages.HomePage);
                OnPowerUpdateCycle(_mono.PowerManager);
                InvokeRepeating(nameof(UpdateScreenOnLoad), 1f,1f);
            }
        }

        private void UpdateScreenOnLoad()
        {
            if (_mono.IsFromSave && GetCurrentPage() != AlterraGenPages.PoweredOffPage)
            {
                if (_mono.PowerManager.PowerState == FCSPowerStates.Tripped)
                {
                    GotoPage(AlterraGenPages.PoweredOffPage);
                }
            }

            if (GetCurrentPage() == AlterraGenPages.PoweredOffPage)
            {
                QuickLogger.Debug("Canceling Invoke Repeating",true);
                CancelInvoke(nameof(UpdateScreenOnLoad));
            }
        }

        private void OnContainerUpdate(int arg1, int arg2)
        {
            _grid.DrawPage();
        }

        private void OnPowerUpdateCycle(AlterraGenPowerManager obj)
        {
            if (obj == null || _breakerStatusLight == null || _powerStateValue == null || _powerUnitValue == null || _grid == null) return;
            //Update the Breaker Status Light
            _breakerStatusLight.color = obj.ProducingPower ? Color.green : Color.red;
                
            //Update the Power State
            _powerStateValue.text = obj.ProducingPower ? AlterraGenBuildable.Active() : AlterraGenBuildable.InActive();

            //Update the Power Amount Stored
            _powerUnitValue.text = obj.GetTotalPowerString();

            //Update Battery Fill
            UpdateBattery(obj.GetBatteryData());

            if (obj.ProducingPower)
            {
                _mono.SetXBubbles(true);
                if (_mono.AnimationManager.GetBoolHash(_isOperational)) return;
                _mono.AnimationManager.SetBoolHash(_isOperational, true);
                QuickLogger.Debug("Starting Animation", true);
            }
            else
            {
                _mono.SetXBubbles(false);
                if (!_mono.AnimationManager.GetBoolHash(_isOperational)) return;
                _mono.AnimationManager.SetBoolHash(_isOperational, false);
                QuickLogger.Debug("Stopping Animation", true);

            }
        }

        private void UpdateBattery(PowerSource data)
        {
            //var charge = data.power < 1 ? 0f : data.maxPower;

            if(data == null) return; 

            var percent = data.power / data.maxPower;

            if (_batteryFill != null)
            {
                if (data.power >= 0f)
                {
                    var value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
                    _batteryFill.color = value;
                    _batteryFill.fillAmount = percent;
                }
                else
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }
            }

            _batteryPercentageValue.text = ((data.power < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "PowerBTN":
                    _mono.PowerManager.TogglePowerState();
                    break;

                case "ColorPickerBTN":
                    QuickLogger.Debug("Color Picker", true);
                    GotoPage(AlterraGenPages.ColorPage);
                    break;

                case "DumpBTN":
                    QuickLogger.Debug("Opening oil dump container", true);
                    _mono.DumpContainer.OpenStorage();
                    break;

                case "ColorItem":
                    QuickLogger.Debug("Trying to change color",true);
                    _mono.ColorManager.ChangeColorMask((Color)tag);
                    break;
                case "HomeBTN":
                    GotoPage(AlterraGenPages.HomePage);
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas

                var canvas = GameObjectHelpers.FindGameObject(gameObject, "Canvas");

                #endregion

                #region Home Page

                var homePage = GameObjectHelpers.FindGameObject(canvas, "HomePage");
                
                //Battery Fill
                _batteryFill = GameObjectHelpers.FindGameObject(homePage, "BatteryFrameFill")?.GetComponent<Image>();

                //Power State
                GameObjectHelpers.FindGameObject(homePage, "PowerState_LBL").GetComponent<Text>().text = AlterraGenBuildable.PowerStateLBL();
                _powerStateValue = GameObjectHelpers.FindGameObject(homePage, "PoweredState_Value")?.GetComponent<Text>();

                //Power Unit
                GameObjectHelpers.FindGameObject(homePage, "PowerUnit_LBL").GetComponent<Text>().text = AlterraGenBuildable.PowerUnitLBL();
                _powerUnitValue = GameObjectHelpers.FindGameObject(homePage, "PowerUnit_Value")?.GetComponent<Text>();

                //Battery Percentage
                GameObjectHelpers.FindGameObject(homePage, "BatteryPercentage_LBL").GetComponent<Text>().text = AlterraGenBuildable.BatteryPercentageLBL();
                _batteryPercentageValue = GameObjectHelpers.FindGameObject(homePage, "BatteryPercentage_Percentage")?.GetComponent<Text>();

                //Breaker State
                GameObjectHelpers.FindGameObject(homePage, "BreakerState_LBL").GetComponent<Text>().text = AlterraGenBuildable.BreakerStateLBL();
                _breakerStatusLight = GameObjectHelpers.FindGameObject(homePage, "BreakerState_StatusLight")?.GetComponent<Image>();

                //Power Button
                var powerBTNObject = GameObjectHelpers.FindGameObject(homePage, "PowerBTN");
                InterfaceHelpers.CreateButton(powerBTNObject, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, Color.cyan, MAX_INTERACTION_DISTANCE);

                //Dump Button
                var dumpBTN = GameObjectHelpers.FindGameObject(homePage, "DumpBTN");
                InterfaceHelpers.CreateButton(dumpBTN, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.gray, Color.white, MAX_INTERACTION_DISTANCE);

                //Color Picker BTN
                var colorPickerBTN = GameObjectHelpers.FindGameObject(homePage, "ColorPickerBTN");
                InterfaceHelpers.CreateButton(colorPickerBTN, "ColorPickerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.gray, Color.white, MAX_INTERACTION_DISTANCE);

                _grid = _mono.gameObject.AddComponent<GridHelper>();
                _grid.OnLoadDisplay += OnLoadItemsGrid;
                _grid.Setup(3, AlterraGenBuildable.ItemPrefab, homePage, Color.gray, Color.gray, OnButtonClick);

                #endregion

                #region PowerOff Page

                var poweroffPage = GameObjectHelpers.FindGameObject(canvas, "PowerOff");

                var powerBTNObject2 = GameObjectHelpers.FindGameObject(poweroffPage, "PowerBTN");
                InterfaceHelpers.CreateButton(powerBTNObject2, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, Color.cyan, MAX_INTERACTION_DISTANCE);

                #endregion

                #region Color Page

                var colorPage = GameObjectHelpers.FindGameObject(canvas, "ColorPage");
                _colorPage.SetupGrid(35, AlterraGenBuildable.ColorItemPrefab, colorPage, OnButtonClick, Color.white, Color.cyan);

                #endregion

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadItemsGrid : {data.ItemsGrid}", true);

                _grid.ClearPage();
                
                var grouped = _mono.PowerManager.GetItemsWithin().ToList();
                
                if (data.EndPosition > grouped.Count())
                {
                    data.EndPosition = grouped.Count();
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
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(grouped.ElementAt(i));
                }
                _grid.UpdaterPaginator(grouped.Count());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal void GotoPage(AlterraGenPages page)
        {
            _mono.AnimationManager.SetIntHash(_pageHash, (int)page);
        }

        internal AlterraGenPages GetCurrentPage()
        {
            return (AlterraGenPages)_mono.AnimationManager.GetIntHash(_pageHash);
        }
        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }
    }
}
