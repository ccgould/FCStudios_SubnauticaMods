using System;
using System.IO;
using System.Linq;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Display;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvDisplayManager : AIDisplay
    {
        private HydroHarvController _mono;
        private readonly Color _startColor = Color.black;
        private readonly Color _hoverColor = Color.white;
        private ColorManager _colorPage;
        private int _page;
        private Atlas.Sprite _melonIconSprite;
        private Atlas.Sprite _kooshIconSprite;
        private uGUI_Icon _modeIcon;
        private Text _powerLevelText;
        private GridHelper _dnaGrid;
        private Text _dnaCounter;
        private Text _powerUsage;
        private Text _itemsCount;
        private Text _timeLeft;


        internal Atlas.Sprite MelonIconSprite => _melonIconSprite ?? (_melonIconSprite = SpriteManager.Get(TechType.Melon));

        internal Atlas.Sprite KooshIconSprite => _kooshIconSprite ?? (_kooshIconSprite = SpriteManager.Get(TechType.SmallKoosh));

        private void UpdateSpeedModeText()
        {
            switch (_mono.CurrentSpeedMode)
            {
                case SpeedModes.Off:
                    _powerLevelText.text = HydroponicHarvestersBuildable.Off();
                    break;
                case SpeedModes.Max:
                    _powerLevelText.text = HydroponicHarvestersBuildable.Max();
                    break;
                case SpeedModes.High:
                    _powerLevelText.text = HydroponicHarvestersBuildable.High();
                    break;
                case SpeedModes.Low:
                    _powerLevelText.text = HydroponicHarvestersBuildable.Low();
                    break;
                case SpeedModes.Min:
                    _powerLevelText.text = HydroponicHarvestersBuildable.Min();
                    break;
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "PowerLevelBTN":
                    switch (_mono.CurrentSpeedMode)
                    {
                        case SpeedModes.High:
                            _mono.CurrentSpeedMode = SpeedModes.Max;
                            break;
                        case SpeedModes.Low:
                            _mono.CurrentSpeedMode = SpeedModes.High;
                            break;
                        case SpeedModes.Min:
                            _mono.CurrentSpeedMode = SpeedModes.Low;
                            break;
                        case SpeedModes.Off:
                            _mono.CurrentSpeedMode = SpeedModes.Min;
                            break;
                        case SpeedModes.Max:
                            _mono.CurrentSpeedMode = SpeedModes.Off;
                            break;
                    }
                    UpdateSpeedModeText();
                    break;
                case "ColorBTN":
                    _mono.AnimationManager.SetIntHash(_page,2);
                    break;
                case "HomeBTN":
                    _mono.AnimationManager.SetIntHash(_page,1);
                    break;
                case "CleanerBTN":
                    _mono.CleanerDumpContainer.OpenStorage();
                    break;
                case "DumpBTN":
                    _mono.DumpContainer.OpenStorage();
                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColorMask((Color)tag);
                    break;
                case "ModeBTN":
                    _mono.ToggleMode();
                    break;
                case "LightBTN":
                    _mono.LightManager.ToggleLight();
                    break;
            }
        }

        internal void ToggleMode(FCSEnvironment mode)
        {
            switch (mode)
            {
                case FCSEnvironment.Air:
                    _modeIcon.sprite = MelonIconSprite;
                    break;
                case FCSEnvironment.Water:
                    _modeIcon.sprite = KooshIconSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal void Setup(HydroHarvController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");
            _colorPage = mono.ColorManager;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                _mono.HydroHarvContainer.OnContainerUpdate += OnContainerUpdate;
                _dnaGrid.DrawPage(1);
            }
        }

        internal void OnContainerUpdate(int arg1,int arg2)
        {
            UpdateDna();
            _itemsCount.text = string.Format(HydroponicHarvestersBuildable.AmountOfItems(),
                $"<color=aqua>{_mono.HydroHarvContainer.GetTotal()}/{_mono.HydroHarvContainer.StorageLimit}</color>");
        }

        public override void PowerOnDisplay()
        {
            QuickLogger.Debug("Power On Display",true);

            _mono.AnimationManager.SetIntHash(_page, _mono.IsConnectedToBase ? 1 : -1);
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

                #region NotOnBase
                var notOnbase = InterfaceHelpers.FindGameObject(canvasGameObject, "NotOnBase")?.GetComponentInChildren<Text>();
                if (notOnbase != null) notOnbase.text = HydroponicHarvestersBuildable.NotOnBaseMessage();
                #endregion

                #region Controls
                var controls = InterfaceHelpers.FindGameObject(home, "Controls");
                #endregion

                #region LightButton
                var lightBTN = InterfaceHelpers.FindGameObject(controls, "LightBTN");

                InterfaceHelpers.CreateButton(lightBTN, "LightBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.ToggleLightMessage());

                var ligtIcon = InterfaceHelpers.FindGameObject(lightBTN, "Icon").AddComponent<uGUI_Icon>();
                ligtIcon.sprite = SpriteManager.Get(TechType.Flashlight);
                #endregion

                #region DNA Counter
                _dnaCounter = InterfaceHelpers.FindGameObject(home, "Limit")?.GetComponent<Text>();
                #endregion

                #region Power Usage
                _powerUsage = InterfaceHelpers.FindGameObject(home, "PowerUsage")?.GetComponent<Text>();
                #endregion

                #region ItemsCount
                _itemsCount = InterfaceHelpers.FindGameObject(home, "ItemsCount")?.GetComponent<Text>();
                #endregion

                #region TimeLeft
                _timeLeft = InterfaceHelpers.FindGameObject(home, "TimeLeft")?.GetComponent<Text>();
                #endregion

                #region CleanerButton
                var cleanerButtonObj = InterfaceHelpers.FindGameObject(controls, "CleanerBTN");

                var cleanerIcon = InterfaceHelpers.FindGameObject(cleanerButtonObj, "Icon").AddComponent<uGUI_Icon>();
                cleanerIcon.sprite = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "FloraKleen.png"));

                InterfaceHelpers.CreateButton(cleanerButtonObj, "CleanerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.CleanerBTNMessage());
                #endregion
                
                #region DumpBTNButton
                var dumpBTNButtonObj = InterfaceHelpers.FindGameObject(controls, "DumpBTN");

                InterfaceHelpers.CreateButton(dumpBTNButtonObj, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.DumpBTNMessage());
                #endregion

                #region ColorPicker Button

                var colorBTN = InterfaceHelpers.FindGameObject(controls, "ColorBTN");

                InterfaceHelpers.CreateButton(colorBTN, "ColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.ColorPickerBTNMessage());

                #endregion

                #region ColorPicker

                var colorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPicker");
                #endregion
                
                #region ColorPage
                _colorPage.SetupGrid(50, HydroponicHarvestersModelPrefab.ColorItemPrefab, colorPicker, OnButtonClick,_startColor,_hoverColor);
                #endregion

                #region PowerLevelButton
                var powerLevelBTN = InterfaceHelpers.FindGameObject(controls, "PowerLevelBTN");
                _powerLevelText = InterfaceHelpers.FindGameObject(powerLevelBTN, "Text").GetComponent<Text>();

                InterfaceHelpers.CreateButton(powerLevelBTN, "PowerLevelBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.PowerLevelBTNMessage());

                #endregion

                #region DNA

                _dnaGrid = _mono.gameObject.AddComponent<GridHelper>();
                _dnaGrid.OnLoadDisplay += OnLoadDnaGrid;
                _dnaGrid.Setup(4, HydroponicHarvestersModelPrefab.ItemPrefab, home, _startColor, _hoverColor, OnButtonClick,5,string.Empty,string.Empty,"Slots", string.Empty, string.Empty);
                #endregion
                
                #region ModeButton
                var modeBTN = InterfaceHelpers.FindGameObject(controls, "ModeBTN");
                
                _modeIcon = InterfaceHelpers.FindGameObject(modeBTN, "Icon").AddComponent<uGUI_Icon>();
                _modeIcon.sprite = MelonIconSprite;

                InterfaceHelpers.CreateButton(modeBTN, "ModeBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, HydroponicHarvestersBuildable.ModeBTNMessage());

                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void OnLoadDnaGrid(DisplayData info)
        {
            _dnaGrid.ClearPage();

            var grouped = _mono.HydroHarvContainer.Items.ToList();

            if (info.EndPosition > grouped.Count)
            {
                info.EndPosition = grouped.Count;
            }

            for (int i = info.StartPosition; i < info.EndPosition; i++)
            {
                var techType = grouped[i].Key;

                GameObject buttonPrefab = Instantiate(info.ItemsPrefab);

                if (buttonPrefab == null || info.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                buttonPrefab.transform.SetParent(info.ItemsGrid.transform, false);
                buttonPrefab.GetComponentInChildren<Text>().text = grouped[i].Value.ToString();

                var mainButton = buttonPrefab.FindChild("MainButton");
                
                var mainBTN = mainButton.AddComponent<ItemButton>();
                mainBTN.Mode = ButtonMode.Take;
                mainBTN.Type = techType;
                mainBTN.ButtonMode = InterfaceButtonMode.Background;
                mainBTN.STARTING_COLOR = _startColor;
                mainBTN.HOVER_COLOR = _hoverColor;
                mainBTN.OnButtonClick = _mono.HydroHarvContainer.RemoveItemFromContainer;
                
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(mainButton, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(techType);

                var deleteButton = buttonPrefab.FindChild("DeleteBTN");

                var deleteBTN = deleteButton.AddComponent<ItemButton>();
                deleteBTN.ButtonMode = InterfaceButtonMode.Background;
                deleteBTN.Mode = ButtonMode.Delete;
                deleteBTN.Type = techType;
                deleteBTN.STARTING_COLOR = _startColor;
                deleteBTN.HOVER_COLOR = _hoverColor;
                deleteBTN.OnButtonClick = _mono.HydroHarvContainer.DeleteItemFromContainer;

                uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(deleteButton, "Icon").AddComponent<uGUI_Icon>();
                trashIcon.sprite = SpriteManager.Get(TechType.Trashcans);
            }
        }

        internal void UpdateDnaCounter()
        {
            if (_dnaCounter == null) return;
            _dnaCounter.text = $"{_mono.HydroHarvGrowBed.GetDNASamplesTotal()}/{_mono.HydroHarvGrowBed.Slots.Length}";
        }

        internal void UpdatePowerUsagePerSecond()
        {
            if (_powerUsage == null) return;
            _powerUsage.text = $"<size=100><color=Green>{Mathf.RoundToInt(_mono.PowerManager.EnergyConsumptionPerSecond)}</color>{Environment.NewLine}Unit Per Second</size>";
        }

        internal void UpdateDna()
        {
            if (_dnaGrid == null) return;
            _dnaGrid.DrawPage();   
        }

        internal void Load()
        {
           UpdateSpeedModeText();
           UpdateDnaCounter();
        }

        internal void UpdateTimeLeft(string hms)
        {
            if (!QPatch.Configuration.Config.GetsDirtyOverTime && _timeLeft.text.Equals(HydroponicHarvestersBuildable.NotAvailable())) return;
            _timeLeft.text = QPatch.Configuration.Config.GetsDirtyOverTime ? string.Format(HydroponicHarvestersBuildable.HMSTime(), $"<color=aqua>{hms}</color>") : HydroponicHarvestersBuildable.NotAvailable();
        }

        internal void RefreshModeBTN(FCSEnvironment savedDataBedType)
        {
            switch (savedDataBedType)
            {
                case FCSEnvironment.Air:
                    _modeIcon.sprite = MelonIconSprite;
                    break;
                case FCSEnvironment.Water:
                    _modeIcon.sprite = KooshIconSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(savedDataBedType), savedDataBedType, null);
            }
        }
    }
}
