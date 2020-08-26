using FCSAlterraShipping.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Display;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCSCommon.Abstract;
using FCSCommon.Extensions;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace FCSPowerStorage.Mono
{
    /**
* Component that contains the controller to the view. Majority of the view is set up already on the prefab inside of the unity editor.
* Handles such things as the paginator, drawing all the items that are on the "current page"
* Handles the idle screen saver.
* Handles the welcome animations.
*/
    internal class FCSPowerStorageDisplay : AIDisplay
    {
        private FCSPowerStorageController _mono;
        private List<ColorVec4> _serializedColors;
        private GameObject _batteryGrid;
        private Text _batteryMonitorAmountLbl;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _pageCounter;
        private Text _pageCounterText;
        private GameObject _colorPicker;
        private Text _autoActivateTextB;
        private Text _baseDrainLimitTextB;

        public override void ClearPage()
        {
            for (int i = 0; i < _colorPicker.transform.childCount; i++)
            {
                Destroy(_colorPicker.transform.GetChild(i).gameObject);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "HomeBTN":
                    _mono.AnimationManager.SetIntHash(_mono.StateHash, 1);
                    break;

                case "SettingsBTN":
                    _mono.AnimationManager.SetIntHash(_mono.StateHash, 2);
                    break;

                case "PowerBTN":
                    var currentState = _mono.AnimationManager.GetIntHash(_mono.StateHash);
                    _mono.PowerManager.SetPowerState(currentState == 4 ? FCSPowerStates.Powered : FCSPowerStates.Unpowered);
                    break;

                case "ColorPickerBTN":
                    _mono.AnimationManager.SetIntHash(_mono.StateHash, 3);
                    break;

                case "TrickleModeBTN":
                    if (_mono.GetAutoActivate())
                    {
                        QuickLogger.Info(LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoDischargeEnabledMessageKey), true);
                        break;
                    }
                    _mono.PowerManager.SetChargeMode(PowerToggleStates.TrickleMode);
                    break;

                case "ChargeModeBTN":
                    if (_mono.GetAutoActivate())
                    {
                        QuickLogger.Info(LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoDischargeEnabledMessageKey), true);
                        break;
                    }
                    _mono.PowerManager.SetChargeMode(PowerToggleStates.ChargeMode);
                    break;

                case "AutoActivateBTN":
                    _mono.SetAutoActivate(!_mono.GetAutoActivate());
                    break;

                case "BaseDrainBTN":
                    UpdateActivateBaseDrain();
                    break;

                case "SyncBTN":
                    _mono.SyncAll();
                    break;

                case "ColorItem":
                    var color = (Color)tag;
                    _mono.SetCurrentBodyColor(color);
                    break;
            }
        }

        internal void UpdateActivateBaseDrain()
        {
            QuickLogger.Debug("Updating Activate Base Drain", true);

            var value = !_mono.GetBaseDrainProtection();

            _mono.Manager.UpdateBaseDrain(value);

        }

        internal void UpdateTextBoxes(int autoActivateValue, int baseDrainValue)
        {
            _autoActivateTextB.text = autoActivateValue.ToString();
            _mono.SetAutoActivateAt(autoActivateValue);
            _baseDrainLimitTextB.text = baseDrainValue.ToString();
            _mono.SetBasePowerProtectionGoal(baseDrainValue);
        }

        public override void ItemModified(TechType item, int newAmount)
        {
            throw new System.NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            #region Canvas

            var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }

            #endregion

            // == Canvas Elements == //

            #region Navigation Dock
            var navigationDock = canvasGameObject.transform.Find("Navigation_Dock")?.gameObject;
            if (navigationDock == null)
            {
                QuickLogger.Error("Dock: Navigation_Dock not found.");
                return false;
            }
            #endregion

            // == Navigation Button Elements == //

            #region Screen Holder

            GameObject screenHolder = canvasGameObject.transform.Find("Screens")?.gameObject;

            if (screenHolder == null)
            {
                QuickLogger.Error("Screen Holder GameObject not found.");
                return false;
            }

            #endregion

            // == Screen Holder Elements == //

            #region Welcome Screen

            var welcomeScreen = screenHolder.FindChild("WelcomePage")?.gameObject;
            if (welcomeScreen == null)
            {
                QuickLogger.Error("Screen: WelcomeScreen not found.");
                return false;
            }

            #endregion

            #region Version Label
            var versionLbl = welcomeScreen.FindChild("Logo_Intro").FindChild("Version_Text")?.gameObject;

            if (versionLbl == null)
            {
                QuickLogger.Error("Cannot find Version_Text Game Object");
            }

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            versionLbl.GetComponent<Text>().text = $"V{assemblyVersion}";
            #endregion

            #region Settings Screen

            var settingsScreen = screenHolder.FindChild("SettingsPage")?.gameObject;
            if (settingsScreen == null)
            {
                QuickLogger.Error("Screen: SettingsPage not found.");
                return false;
            }

            #endregion

            #region Battery Monitor Page
            var batteryMonitorPage = screenHolder.FindChild("BatteryMonitorPage")?.gameObject;
            if (batteryMonitorPage == null)
            {
                QuickLogger.Error("Screen: BatteryMonitorPage not found.");
                return false;
            }

            QuickLogger.Info("Finding Meters");
            #endregion

            #region Battery Grid
            _batteryGrid = batteryMonitorPage.FindChild("Grid")?.gameObject;
            if (_batteryGrid == null)
            {
                QuickLogger.Error("Screen: BatteryMonitorPage not found.");
                return false;
            }

            QuickLogger.Debug($"Meter Count {_batteryGrid.transform.childCount}");


            for (int i = 0; i < _mono.BatteryCount; i++)
            {
                QuickLogger.Info($"Meter {i}");
                var powercell = _mono.PowerManager.GetPowerCell(i);
                QuickLogger.Debug($"Battery {powercell.GetName()}");
                powercell.SetMeter(_batteryGrid.transform.GetChild(i).gameObject);
            }
            #endregion

            #region Power Off Page
            var powerOffPage = screenHolder.FindChild("PowerOffPage")?.gameObject;
            if (powerOffPage == null)
            {
                QuickLogger.Error("Screen: PowerOffPage not found.");
                return false;
            }
            #endregion

            #region Boot Page
            var bootingPage = screenHolder.FindChild("BootingPage")?.gameObject;
            if (bootingPage == null)
            {
                QuickLogger.Error("Screen: BootingPage not found.");
                return false;
            }
            #endregion

            // == Powered off Elements
            #region PoweredOff LBL

            var poweredOffLbl = powerOffPage.FindChild("Powered_Off_LBL").GetComponent<Text>();
            if (poweredOffLbl == null)
            {
                QuickLogger.Error("Screen: Powered_Off_LBL  not found.");
                return false;
            }
            poweredOffLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.PoweredOffKey);

            #endregion


            // == Battery MonitorPage Elements

            #region Battery Monitor Power Amount Label
            _batteryMonitorAmountLbl = batteryMonitorPage.FindChild("Battery_Monitor_Amount_LBL").GetComponent<Text>();
            if (_batteryMonitorAmountLbl == null)
            {
                QuickLogger.Error("Screen: Battery_Monitor_Amount_LBL not found.");
                return false;
            }
            #endregion

            #region Battery Monitor Label

            var batteryMonitorLbl = batteryMonitorPage.FindChild("Battery_Monitor_LBL").GetComponent<Text>();
            if (batteryMonitorLbl == null)
            {
                QuickLogger.Error("Screen: Battery_Monitor_LBL not found.");
                return false;
            }
            batteryMonitorLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BatteryMetersKey);

            #endregion

            #region Trickle Mode BTN
            var trickleModeBtn = batteryMonitorPage.FindChild("Trickle_Mode")?.gameObject;
            if (trickleModeBtn == null)
            {
                QuickLogger.Error("Screen: Trickle_Mode not found.");
                return false;
            }

            InterfaceButton trickleBTN = trickleModeBtn.AddComponent<InterfaceButton>();
            trickleBTN.OnButtonClick += OnButtonClick;
            trickleBTN.ButtonMode = InterfaceButtonMode.None;
            trickleBTN.BtnName = "TrickleModeBTN";

            #endregion

            #region Charge Mode BTN
            var chargeModeBtn = batteryMonitorPage.FindChild("Charge_Mode")?.gameObject;
            if (chargeModeBtn == null)
            {
                QuickLogger.Error("Screen: Charge_Mode not found.");
                return false;
            }

            InterfaceButton chargeBTN = chargeModeBtn.AddComponent<InterfaceButton>();
            chargeBTN.ButtonMode = InterfaceButtonMode.None;
            chargeBTN.OnButtonClick += OnButtonClick;
            chargeBTN.BtnName = "ChargeModeBTN";
            #endregion


            // == Boot Page Elements == //

            #region Booting LBL

            var bootingLbl = bootingPage.FindChild("Booting_TXT").GetComponent<Text>();
            if (bootingLbl == null)
            {
                QuickLogger.Error("Screen: _bootingLBL  not found.");
                return false;
            }
            bootingLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BootingKey);

            #endregion

            // == Settings Page Elements == //
            #region Color Picker
            var colorPicker = settingsScreen.FindChild("Color_Picker")?.gameObject;
            if (colorPicker == null)
            {
                QuickLogger.Error("Screen: _color_Picker not found.");
                return false;
            }

            InterfaceButton colorPickerBTN = colorPicker.AddComponent<InterfaceButton>();
            colorPickerBTN.OnButtonClick += OnButtonClick;
            colorPickerBTN.ButtonMode = InterfaceButtonMode.None;
            colorPickerBTN.BtnName = "ColorPickerBTN";
            #endregion

            #region Color Picker LBL

            var colorPickerLbl = colorPicker.FindChild("Label").GetComponent<Text>();
            if (colorPickerLbl == null)
            {
                QuickLogger.Error("Screen: Color Picker Label not found.");
                return false;
            }
            colorPickerLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.ColorPickerKey);

            #endregion

            #region Settings LBL

            var settingsLbl = settingsScreen.FindChild("Setting_LBL").GetComponent<Text>();
            if (settingsLbl == null)
            {
                QuickLogger.Error("Screen: Settings Page Label not found.");
                return false;
            }
            settingsLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.SettingsKey);

            #endregion

            #region System Settings LBL

            var storageModeLbl = settingsScreen.FindChild("System_Settings_LBL").GetComponent<Text>();
            if (storageModeLbl == null)
            {
                QuickLogger.Error("Screen: Storage Mode Label not found.");
                return false;
            }
            storageModeLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.SystemSettingsLblKey);

            #endregion

            #region Grid
            var grid = settingsScreen.FindChild("Grid")?.gameObject;
            if (grid == null)
            {
                QuickLogger.Error("Screen: Grid not found.");
                return false;
            }
            #endregion

            #region Auto Activate BTN
            var autoActivateBtn = grid.FindChild("Auto_Activate")?.gameObject;
            if (autoActivateBtn == null)
            {
                QuickLogger.Error("Screen: Auto_Activate not found.");
                return false;
            }

            var autoActivateTxt = autoActivateBtn.FindChild("Label").GetComponent<Text>();

            if (autoActivateTxt == null)
            {
                QuickLogger.Error("Screen: Auto_Activate text not found.");
                return false;
            }

            autoActivateTxt.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoActivateKey);

            InterfaceButton autoABTN = autoActivateBtn.AddComponent<InterfaceButton>();
            autoABTN.ButtonMode = InterfaceButtonMode.None;
            autoABTN.OnButtonClick += OnButtonClick;
            autoABTN.BtnName = "AutoActivateBTN";

            #endregion

            #region Sync BTN
            var syncBtn = grid.FindChild("Sync")?.gameObject;
            if (syncBtn == null)
            {
                QuickLogger.Error("Screen: Sync not found.");
                return false;
            }

            var syncAllBtnTxt = syncBtn.FindChild("Label").GetComponent<Text>();

            if (syncAllBtnTxt == null)
            {
                QuickLogger.Error("Screen: Sync All text not found.");
                return false;
            }

            syncAllBtnTxt.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.SyncAllKey);

            InterfaceButton syncBTN = syncBtn.AddComponent<InterfaceButton>();
            syncBTN.ButtonMode = InterfaceButtonMode.None;
            syncBTN.OnButtonClick += OnButtonClick;
            syncBTN.BtnName = "SyncBTN";

            #endregion

            #region Auto Activate Limit 
            var autoActivateLimitBtn = grid.FindChild("Auto_Activate_Limit")?.gameObject;
            if (autoActivateLimitBtn == null)
            {
                QuickLogger.Error("Screen: Auto_Activate_Limit not found.");
                return false;
            }

            var autoActivateLimitTxt = autoActivateLimitBtn.FindChild("Label").GetComponent<Text>();

            if (autoActivateLimitTxt == null)
            {
                QuickLogger.Error("Screen: Auto_Activate_limit text not found.");
                return false;
            }

            autoActivateLimitTxt.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoActivateAtKey);

            var autoActivateLimitTextBox = autoActivateLimitBtn.FindChild("Background").FindChild("Text")?.gameObject;

            if (autoActivateLimitTextBox == null)
            {
                QuickLogger.Error("Screen:btnName Text not found.");
                return false;
            }

            _autoActivateTextB = autoActivateLimitTextBox.GetComponent<Text>();
            _autoActivateTextB.text = _mono.GetAutoActivateAt().ToString();

            var autoActivateResult = CreateSystemButton(autoActivateLimitTextBox,
                LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoActivateDescKey), LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoActivateOnHoverKey),
                _mono.GetAutoActivateAt, _mono.ValidateAutoConfigUnits);

            if (!autoActivateResult)
            {
                return false;
            }

            #endregion

            #region Base Drain Protection BTN
            var baseDrainProtectionBtn = grid.FindChild("Base_Drain_Protection")?.gameObject;
            if (baseDrainProtectionBtn == null)
            {
                QuickLogger.Error("Screen: Base_Drain_Protection not found.");
                return false;
            }

            var baseDrianProtectionTxt = baseDrainProtectionBtn.FindChild("Label").GetComponent<Text>();

            if (baseDrianProtectionTxt == null)
            {
                QuickLogger.Error("Screen: Base_Drain_protection text not found.");
                return false;
            }

            baseDrianProtectionTxt.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BaseDrainProtectionKey);

            InterfaceButton bDPBTN = baseDrainProtectionBtn.AddComponent<InterfaceButton>();
            bDPBTN.ButtonMode = InterfaceButtonMode.None;
            bDPBTN.OnButtonClick += OnButtonClick;
            bDPBTN.BtnName = "BaseDrainBTN";
            #endregion

            #region Base Drain Limit 
            var baseDrainlimitBtn = grid.FindChild("Base_Drain_Limit")?.gameObject;

            if (baseDrainlimitBtn == null)
            {
                QuickLogger.Error("Screen: Base_Drain_Limit not found.");
                return false;
            }

            var baseDrianLimitTxt = baseDrainlimitBtn.FindChild("Label").GetComponent<Text>();

            if (baseDrianLimitTxt == null)
            {
                QuickLogger.Error("Screen: Base_Drain_Limit  text not found.");
                return false;
            }

            baseDrianLimitTxt.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BaseDrainLimitKey);

            var baseDrainlimitTextBox = baseDrainlimitBtn.FindChild("Background").FindChild("Text")?.gameObject;

            if (baseDrainlimitTextBox == null)
            {
                QuickLogger.Error("Screen:baseDrainlimitTextBox Text not found.");
                return false;
            }

            _baseDrainLimitTextB = baseDrainlimitTextBox.GetComponent<Text>();
            _baseDrainLimitTextB.text = _mono.GetBasePowerProtectionGoal().ToString();

            var baseDrainResult = CreateSystemButton(baseDrainlimitTextBox,
                LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BaseDrainLimitDescKey), LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BaseDrainLimitOnHoverKey),
                _mono.GetBasePowerProtectionGoal, _mono.ValidateBaseProtectionUnits);

            if (!baseDrainResult)
            {
                return false;
            }
            #endregion


            //TODO Add langugae for btns

            #region Discharge Mode LBL
            var trickleModeLbl = trickleModeBtn.FindChild("Label").GetComponent<Text>();
            if (trickleModeLbl == null)
            {
                QuickLogger.Error("Screen: TrickleModeLabel not found.");
                return false;
            }
            trickleModeLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.DischargeKey);

            #endregion

            #region Charge Mode LBL

            var chargeModeLbl = chargeModeBtn.FindChild("Label").GetComponent<Text>();
            if (chargeModeLbl == null)
            {
                QuickLogger.Error("Screen: Charge Mode LBL not found.");
                return false;
            }
            chargeModeLbl.text = LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.ChargeKey);

            #endregion

            // == Color Picker Elements == //

            #region Color Picker Page
            var colorPickerPage = screenHolder.FindChild("ColorPickerPage")?.gameObject;
            if (colorPickerPage == null)
            {
                QuickLogger.Error("Screen: ColorPicker not found.");
                return false;
            }
            #endregion

            _colorPicker = colorPickerPage.FindChild("ColorPicker")?.gameObject;
            if (_colorPicker == null)
            {
                QuickLogger.Error("GameObject: ColorPicker not found.");
                return false;
            }

            #region Color Picker Previous Page BTN
            _previousPageGameObject = colorPickerPage.FindChild("Back_Arrow_BTN")?.gameObject;
            if (_previousPageGameObject == null)
            {
                QuickLogger.Error("Screen: Back_Arrow_BTN not found.");
                return false;
            }

            var prevPageBTN = _previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.AmountToChangePageBy = -1;
            prevPageBTN.ChangePageBy = ChangePageBy;
            #endregion

            #region Color Picker Next Page BTN
            _nextPageGameObject = colorPickerPage.FindChild("Forward_Arrow_BTN")?.gameObject;
            if (_nextPageGameObject == null)
            {
                QuickLogger.Error("Screen: Forward_Arrow_BTN not found.");
                return false;
            }

            var nextPageBTN = _nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.ChangePageBy = ChangePageBy;
            nextPageBTN.AmountToChangePageBy = 1;
            #endregion

            #region Color Picker Page Counter
            _pageCounter = colorPickerPage.FindChild("Page_Number")?.gameObject;
            if (_pageCounter == null)
            {
                QuickLogger.Error("Screen: Page_Number not found.");
                return false;
            }

            _pageCounterText = _pageCounter.GetComponent<Text>();
            if (_pageCounterText == null)
            {
                QuickLogger.Error("Screen: _pageCounterText not found.");
                return false;
            }
            #endregion

            // == Navigation Dock Elements == //

            #region Settings Button
            var settingButton = navigationDock.transform.Find("Settings_BTN")?.gameObject;
            if (settingButton == null)
            {
                QuickLogger.Error("Dock: Settings_BTN not found.");
                return false;
            }


            InterfaceButton settingsBTN = settingButton.AddComponent<InterfaceButton>();
            settingsBTN.OnButtonClick = OnButtonClick;
            settingsBTN.BtnName = "SettingsBTN";
            settingsBTN.ButtonMode = InterfaceButtonMode.Background;
            #endregion

            #region Home Button
            var homeButton = navigationDock.transform.Find("Home_BTN")?.gameObject;
            if (homeButton == null)
            {
                QuickLogger.Error("Dock: Home_BTN not found.");
                return false;
            }

            InterfaceButton home = homeButton.AddComponent<InterfaceButton>();
            home.OnButtonClick = OnButtonClick;
            home.BtnName = "HomeBTN";
            home.ButtonMode = InterfaceButtonMode.Background;
            #endregion

            #region Power Button
            var powerButton = navigationDock.transform.Find("Power_BTN")?.gameObject;
            if (powerButton == null)
            {
                QuickLogger.Error("Dock: Power_BTN not found.");
                return false;
            }

            InterfaceButton power = powerButton.AddComponent<InterfaceButton>();
            power.OnButtonClick = OnButtonClick;
            power.BtnName = "PowerBTN";
            power.ButtonMode = InterfaceButtonMode.Background;

            #endregion

            return true;
        }

        private bool CreateSystemButton(GameObject go, string desc, string onhover, Func<int> getValue, Action<int> setValue)
        {

            var textBoxController = go.AddComponent<FCSTextBoxController>();
            textBoxController.OnLabelChanged += OnLabelChanged;
            FCSTextBoxController.Create(desc, onhover, go, getValue, setValue);

            return true;
        }

        private void OnLabelChanged()
        {
            _mono.Manager.UpdateTextBoxes(_mono.GetAutoActivateAt(), _mono.GetBasePowerProtectionGoal());
        }

        public override IEnumerator PowerOff()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_mono.StateHash, 4);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_mono.StateHash, 1);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator ShutDown()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_mono.StateHash, 0);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator CompleteSetup()
        {
            yield return new WaitForEndOfFrame();
            PowerOnDisplay();
            yield return new WaitForEndOfFrame();
        }

        public override void DrawPage(int page)
        {
            CurrentPage = page;

            if (CurrentPage <= 0)
            {
                CurrentPage = 1;
            }
            else if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }

            int startingPosition = (CurrentPage - 1) * ITEMS_PER_PAGE;
            int endingPosition = startingPosition + ITEMS_PER_PAGE;

            if (endingPosition > _serializedColors.Count)
            {
                endingPosition = _serializedColors.Count;
            }

            ClearPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var colorID = _serializedColors.ElementAt(i);
                LoadColorPicker(colorID);
            }

            UpdatePaginator();
        }

        public override void UpdatePaginator()
        {
            CalculateNewMaxPages();
            _pageCounter.SetActive(_serializedColors.Count != 0);
            _pageCounterText.text = $"{CurrentPage} / {MaxPage}";
            _previousPageGameObject.SetActive(CurrentPage != 1);
            _nextPageGameObject.SetActive(CurrentPage != MaxPage);
        }

        private void CalculateNewMaxPages()
        {
            MaxPage = Mathf.CeilToInt((_serializedColors.Count - 1) / ITEMS_PER_PAGE) + 1;
            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }
        }

        private void LoadColorPicker(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(FCSPowerStorageBuildable.ColorItemPefab);
            itemDisplay.transform.SetParent(_colorPicker.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.Vector4ToColor();
        }

        internal void Setup(FCSPowerStorageController mono)
        {
            _mono = mono;

            if (FindAllComponents() == false)
            {
                ShutDownDisplay();
                return;
            }

            //var colors = File.ReadAllText(Path.Combine(Information.GetAssetPath(), "colors.json"));
            _serializedColors = ColorList.Colors; //JsonConvert.DeserializeObject<List<SerializableColor>>(colors);

            InvokeRepeating("UpdateBatteryMonitor", 1, 0.1f);
            InvokeRepeating("UpdatePowerInfo", 1, 0.1f);

            ITEMS_PER_PAGE = 28;

            DrawPage(1);

            StartCoroutine(CompleteSetup());

        }

        private void UpdatePowerInfo()
        {
            _batteryMonitorAmountLbl.text = $"{Mathf.RoundToInt(_mono.PowerManager.GetPowerSum())}/{LoadData.BatteryConfiguration.Capacity}";
        }

        private void UpdateBatteryMonitor()
        {
            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Unpowered) return;

            for (int i = 0; i < _mono.BatteryCount; i++)
            {
                _mono.PowerManager.GetPowerCell(i).UpdateBatteryMeter();
            }
        }
    }
}
