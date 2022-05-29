using System;
using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerHUD : uGUI_InputGroup, uGUI_IButtonReceiver
    {
        private GameObject _grid;
        public static DeepDrillerHUD Main;
        private bool _isOpen;
        private DrillSystem _sender;
        private Toggle _isStandbyToggle;
        private BatteryMeterController _batteryMeter;
        private Text _unitID;
        private BatteryMeterController _lubeMeter;
        private Text _oresPerDayAmount;
        private Text _powerConsumptionAmount;
        private DeepDrillerGUIInventoryPage _inventoryPage;
        private GameObject _homePage;
        private DeepDrillerGUISettingsPage _settingsPage;
        private DeepDrillerGUIFilterPage _filterPage;
        private DeepDrillerGUISignalPage _signalPage;
        private DeepDrillerGUIOilPage _lubricantPage;
        private FCSMessageBox _messageBox;
        private Text _filterPageInformation;
        private Text _biomeLbl;

        public override void Update()
        {
            base.Update();

            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }

            UpdateScreen();
        }

        public override void Awake()
        {
            base.Awake();

            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }

            _batteryMeter = InterfaceHelpers.FindGameObject(gameObject, "BatteryMeter")?.AddComponent<BatteryMeterController>();
            _batteryMeter?.Initialize(QPatch.Configuration.DDInternalBatteryCapacity);

            _biomeLbl = InterfaceHelpers.FindGameObject(gameObject, "Biome").GetComponent<Text>();


            _lubeMeter = InterfaceHelpers.FindGameObject(gameObject, "LubeMeter")?.AddComponent<BatteryMeterController>();
            _lubeMeter?.Initialize();
            _unitID = InterfaceHelpers.FindGameObject(gameObject, "UnitID")?.GetComponent<Text>();
            _oresPerDayAmount = InterfaceHelpers.FindGameObject(gameObject, "OresPerDayAmount")?.GetComponent<Text>();
            _powerConsumptionAmount = InterfaceHelpers.FindGameObject(gameObject, "PowerConsumptionAmount")?.GetComponent<Text>();

            _inventoryPage = InterfaceHelpers.FindGameObject(gameObject, "InventoryPage").AddComponent<DeepDrillerGUIInventoryPage>();
            _inventoryPage.Hud = this;

            _settingsPage = InterfaceHelpers.FindGameObject(gameObject, "Settings").AddComponent<DeepDrillerGUISettingsPage>();
            _settingsPage.Hud = this;

            _homePage = InterfaceHelpers.FindGameObject(gameObject, "Home");
            
            _filterPage = InterfaceHelpers.FindGameObject(gameObject, "FilterPage").AddComponent<DeepDrillerGUIFilterPage>();
            _filterPage.Hud = this;

            _signalPage = InterfaceHelpers.FindGameObject(gameObject, "SignalPage").AddComponent<DeepDrillerGUISignalPage>();
            _signalPage.Hud = this;

            _lubricantPage = InterfaceHelpers.FindGameObject(gameObject, "OilPage").AddComponent <DeepDrillerGUIOilPage>();
            _lubricantPage.Hud = this;

            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener((Hide));

            var inventoryBTN = GameObjectHelpers.FindGameObject(gameObject, "StorageBTN").GetComponent<Button>();
            inventoryBTN.onClick.AddListener((() =>
            {
                GoToPage(DeepDrillerHudPages.Inventory);
            }));

            var settingsBTN = GameObjectHelpers.FindGameObject(gameObject, "SettingsBTN").GetComponent<Button>();
            settingsBTN.onClick.AddListener((() =>
            {
                GoToPage(DeepDrillerHudPages.Settings);
            }));

            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();

        }

        internal void ShowMessage(string message)
        {
            _messageBox.Show(message,FCSMessageButton.OK,null);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void UpdateScreen()
        {
            if (_sender == null || !_isOpen || _isStandbyToggle == null) return;
        }

        private void OnLoadDisplay()
        {
            try
            {
                _unitID.text = _sender.UnitID;
                _biomeLbl.text = FCSDeepDrillerBuildable.BiomeFormat(_sender.CurrentBiome);
                OnBatteryLevelChange(_sender.GetBatteryPowerData());
                OnOilLevelChange(_sender.GetOilPercentage());
                _oresPerDayAmount.text = _sender.GetOresPerDayCount();
                _powerConsumptionAmount.text = _sender.GetPowerUsageAmount();
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }

        }
        
        internal void Show(DrillSystem sender)
        {
            try
            {
                if (Time.timeSinceLevelLoad < 1f)
                {
                    return;
                }
                
                if (!gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                    Select();
                }
                GoToPage(DeepDrillerHudPages.Home);

                _sender = sender;

                if (_sender != null)
                {
                    sender.OnBatteryLevelChange += OnBatteryLevelChange;
                    sender.OnOilLevelChange += OnOilLevelChange;
                    sender.DeepDrillerContainer.OnContainerUpdate += RefreshInventory;
                }
                
                OnLoadDisplay();
                
                _isOpen = true;
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message);
                QuickLogger.Debug(e.StackTrace);
                _isOpen = false;
                Hide();
            }
        }

        private void RefreshInventory(int i, int i1)
        {
            _inventoryPage.Refresh();
        }

        private void OnOilLevelChange(float obj)
        {
            _lubeMeter.UpdateStateByPercentage(obj);
            _lubricantPage.LubeMeter?.UpdateStateByPercentage(obj);
        }

        private void OnBatteryLevelChange(PowercellData obj)
        {
            _batteryMeter.UpdateBatteryStatus(obj);  
        }

        public void Hide()
        {
            Deselect();
            Clear();
        }

        private void Clear()
        {
            if (_sender != null)
            {
                _sender.OnBatteryLevelChange -= OnBatteryLevelChange;
                _sender.OnOilLevelChange -= OnOilLevelChange;
                _sender.DeepDrillerContainer.OnContainerUpdate -= RefreshInventory;

                //_sender = null; //Not cleared due to the renaming of the drill
            }
        }

        public bool OnButtonDown(GameInput.Button button)
        {
            if (button == GameInput.Button.UICancel || button == GameInput.Button.PDA)
            {
                Deselect();
                return true;
            }
            return false;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            Clear();
            gameObject.SetActive(false);
            _isOpen = false;
            if (_sender != null)
            {
                //_sender.OnProcessingCompleted -= OnLoadDisplay;
                //_sender = null;//Not cleared due to the renaming of the drill
            }
        }

        public override void OnReselect(bool lockMovement)
        {
            base.OnReselect(true);
        }

        public void GoToPage(DeepDrillerHudPages home)
        {
            ResetPages();

            switch (home)
            {
                case DeepDrillerHudPages.Home:
                    _homePage?.SetActive(true);
                    break;
                case DeepDrillerHudPages.Settings:
                    _settingsPage?.Show();
                    break;
                case DeepDrillerHudPages.Oil:
                    _lubricantPage?.Show();
                    break;
                case DeepDrillerHudPages.Beacon:
                    _signalPage?.Show();
                    break;
                case DeepDrillerHudPages.Power:

                    break;
                case DeepDrillerHudPages.Filter:
                    _filterPage?.Show();
                    break;
                case DeepDrillerHudPages.Inventory:
                    _inventoryPage?.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(home), home, null);
            }

        }

        private void ResetPages()
        {
            _inventoryPage?.Hide();
            _homePage.gameObject?.SetActive(false);
            _settingsPage.Hide();
            _filterPage.Hide();
            _signalPage.Hide();
            _lubricantPage.Hide();
        }

        public int GetContainerTotal()
        {
            return _sender.DeepDrillerContainer.GetContainerTotal();
        }

        public int GetStorageSize()
        {
            return _sender.DeepDrillerContainer.GetContainerCapacity();
        }

        public Dictionary<TechType,int> GetItemsWithin()
        {
            return _sender.DeepDrillerContainer.GetItemsWithin();
        }

        public FCSDeepDrillerContainer GetContainer()
        {
            return _sender.DeepDrillerContainer;
        }

        internal DrillSystem GetSender()
        {
            return _sender;
        }
    }
}