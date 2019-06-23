using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Buildables;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Display;
using Oculus.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace FCSPowerStorage.Model.Components
{
    /**
* Component that contains the controller to the view. Majority of the view is set up already on the prefab inside of the unity editor.
* Handles such things as the paginator, drawing all the items that are on the "current page"
* Handles the idle screen saver.
* Handles the welcome animations.
*/
    internal class FCSPowerStorageDisplay : AIDisplay
    {
        private CustomBatteryController _mono;
        private List<SerializableColor> _serializedColors;
        private GameObject _batteryGrid;
        private GameObject _batteryMonitorAmountLBL;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _pageCounter;
        private Text _pageCounterText;
        private GameObject _colorPicker;
        private int _stateHash;

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

            }
        }

        public override void ItemModified<T>(T item)
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

            for (int i = 0; i < batteryMonitorPage.transform.childCount; i++)
            {
                var powercell = _mono.PowerManager.GetPowerCell(i);
                powercell.SetMeter(batteryMonitorPage.transform.GetChild(i).gameObject);
            }

            #endregion

            #region Battery Grid
            _batteryGrid = batteryMonitorPage.FindChild("Grid")?.gameObject;
            if (_batteryGrid == null)
            {
                QuickLogger.Error("Screen: BatteryMonitorPage not found.");
                return false;
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

            // == Battery MonitorPage Elements

            #region Battery Monitor Power Amount Label
            _batteryMonitorAmountLBL = batteryMonitorPage.FindChild("Battery_Monitor_Amount_LBL")?.gameObject;
            if (_batteryMonitorAmountLBL == null)
            {
                QuickLogger.Error("Screen: Battery_Monitor_Amount_LBL not found.");
                return false;
            }
            #endregion

            #region Battery Monitor Label
            var batteryMonitorLbl = batteryMonitorPage.FindChild("Battery_Monitor_LBL")?.gameObject;
            if (batteryMonitorLbl == null)
            {
                QuickLogger.Error("Screen: Battery_Monitor_LBL not found.");
                return false;
            }
            #endregion
            // == Boot Page Elements == //

            #region Booting LBL

            var bootingLbl = bootingPage.FindChild("Booting_TXT")?.gameObject;
            if (bootingLbl == null)
            {
                QuickLogger.Error("Screen: _bootingLBL  not found.");
                return false;
            }

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

            var colorPickerLbl = colorPicker.FindChild("Label")?.gameObject;
            if (colorPickerLbl == null)
            {
                QuickLogger.Error("Screen: Color Picker Label not found.");
                return false;
            }

            #endregion

            #region Settings LBL

            var settingsLbl = settingsScreen.FindChild("Setting_LBL")?.gameObject;
            if (settingsLbl == null)
            {
                QuickLogger.Error("Screen: Settings Page Label not found.");
                return false;
            }

            #endregion

            #region Storage Mode LBL

            var storageModeLbl = settingsScreen.FindChild("Storage_Mode_LBL")?.gameObject;
            if (storageModeLbl == null)
            {
                QuickLogger.Error("Screen: Storage Mode Label not found.");
                return false;
            }

            #endregion

            #region Trickle Mode BTN
            var trickleModeBtn = settingsScreen.FindChild("Trickle_Mode")?.gameObject;
            if (trickleModeBtn == null)
            {
                QuickLogger.Error("Screen: Trickle_Mode not found.");
                return false;
            }

            InterfaceButton trickleBTN = trickleModeBtn.AddComponent<InterfaceButton>();
            trickleBTN.OnButtonClick += OnButtonClick;
            trickleBTN.ButtonMode = InterfaceButtonMode.None;
            trickleBTN.BtnName = "TrickleModeBTN";

            var trickleModeCheckBox = trickleModeBtn.FindChild("Background").FindChild("Checkmark")?.gameObject;
            if (trickleModeCheckBox == null)
            {
                QuickLogger.Error("Screen: Trickle_Mode =>Checkmark not found.");
                return false;
            }
            #endregion

            #region Trickle Mode LBL
            var trickleModeLbl = trickleModeBtn.FindChild("Label")?.gameObject;
            if (trickleModeLbl == null)
            {
                QuickLogger.Error("Screen: TrickleModeLabel not found.");
                return false;
            }
            #endregion

            #region Charge Mode BTN
            var chargeModeBtn = settingsScreen.FindChild("Charge_Mode")?.gameObject;
            if (chargeModeBtn == null)
            {
                QuickLogger.Error("Screen: Charge_Mode not found.");
                return false;
            }

            InterfaceButton chargeBTN = chargeModeBtn.AddComponent<InterfaceButton>();
            chargeBTN.ButtonMode = InterfaceButtonMode.None;
            chargeBTN.OnButtonClick += OnButtonClick;
            chargeBTN.BtnName = "ChargeModeBTN";


            var chargeModeCheckBox = chargeModeBtn.FindChild("Background").FindChild("Checkmark")?.gameObject;
            if (chargeModeCheckBox == null)
            {
                QuickLogger.Error("Screen: Charge_Mode =>Checkmark not found.");
                return false;
            }
            #endregion

            #region Charge Mode LBL
            var chargeModeLbl = chargeModeBtn.FindChild("Label")?.gameObject;
            if (chargeModeLbl == null)
            {
                QuickLogger.Error("Screen: Charge Mode LBL not found.");
                return false;
            }
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
            prevPageBTN.ChangePageBy = ChangePageBy;
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

        public override IEnumerator PowerOff()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_stateHash, 4);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_stateHash, 1);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator ShutDown()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_stateHash, 0);
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
        private void LoadColorPicker(SerializableColor color)
        {
            GameObject itemDisplay = Instantiate(FCSPowerStorageBuildable.ColorItemPefab);
            itemDisplay.transform.SetParent(_colorPicker.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.ToColor();
        }

        internal void Setup(CustomBatteryController mono)
        {
            _mono = mono;

            _stateHash = Animator.StringToHash("state");

            if (FindAllComponents() == false)
            {
                ShutDownDisplay();
                return;
            }

            var colors = File.ReadAllText(Path.Combine(Information.ASSETSFOLDER, "colors.json"));
            _serializedColors = JsonConvert.DeserializeObject<List<SerializableColor>>(colors);

            InvokeRepeating("UpdateBatteryMonitor", 1, 1);

            StartCoroutine(CompleteSetup());

        }

        private void UpdateBatteryMonitor()
        {
            if (_mono.PowerManager.PowerState != FCSPowerStates.Unpowered)
            {
                for (int i = 0; i < _mono.BatteryCount; i++)
                {
                    _mono.PowerManager.GetPowerCell(i).UpdateBatteryMeter();
                }
            }
        }
    }
}
