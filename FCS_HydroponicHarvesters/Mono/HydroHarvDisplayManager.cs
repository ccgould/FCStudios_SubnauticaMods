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
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvDisplayManager : AIDisplay
    {
        private HydroHarvController _mono;
        private readonly Color _startColor = Color.white;
        private readonly Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private readonly Color _fireBrickColor = new Color(0.6980392f, 0.1333333f, 0.1333333f, 1f);
        private readonly Color _greenColor = new Color(0.03424335f, 1f, 0f, 1f);
        private bool _initialized;
        private ColorManager _colorPage;
        private int _page;
        private Atlas.Sprite _melonIconSprite;
        private Atlas.Sprite _kooshIconSprite;
        private uGUI_Icon _modeIcon;
        private Text _powerLevelText;
        private GridHelper _dnaGrid;

        internal Atlas.Sprite MelonIconSprite => _melonIconSprite ?? (_melonIconSprite = SpriteManager.Get(TechType.Melon));

        internal Atlas.Sprite KooshIconSprite => _kooshIconSprite ?? (_kooshIconSprite = SpriteManager.Get(TechType.SmallKoosh));
        
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
                    QuickLogger.Debug($"Clicked on {btnName}", true);
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
            }
        }

        private void UpdateSpeedModeText()
        {
            switch (_mono.CurrentSpeedMode)
            {
                case SpeedModes.Off:
                    _powerLevelText.text = HydroponicHarvestersBuidable.Off();
                    break;
                case SpeedModes.Max:
                    _powerLevelText.text = HydroponicHarvestersBuidable.Max();
                    break;
                case SpeedModes.High:
                    _powerLevelText.text = HydroponicHarvestersBuidable.High();
                    break;
                case SpeedModes.Low:
                    _powerLevelText.text = HydroponicHarvestersBuidable.Low();
                    break;
                case SpeedModes.Min:
                    _powerLevelText.text = HydroponicHarvestersBuidable.Min();
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

        public void Setup(HydroHarvController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");
            _colorPage = mono.ColorManager;

            if (FindAllComponents())
            {
                _initialized = true;
                PowerOnDisplay();
                _mono.HydroHarvContainer.OnContainerUpdate += OnContainerUpdate;
                _dnaGrid.DrawPage(1);
            }
        }

        private void OnContainerUpdate()
        {
            UpdateDna();
        }

        public override void PowerOnDisplay()
        {
           _mono.AnimationManager.SetIntHash(_page,1);
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

                #region Controls
                var controls = InterfaceHelpers.FindGameObject(home, "Controls");
                #endregion

                #region CleanerButton
                var cleanerButtonObj = InterfaceHelpers.FindGameObject(controls, "CleanerBTN");

                InterfaceHelpers.CreateButton(cleanerButtonObj, "CleanerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, "");
                #endregion

                #region DumpBTNButton
                var dumpBTNButtonObj = InterfaceHelpers.FindGameObject(controls, "DumpBTN");

                InterfaceHelpers.CreateButton(dumpBTNButtonObj, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE, "");
                #endregion

                #region Messages
                //InterfaceHelpers.FindGameObject(home, "AmountOfPods").GetComponent<Text>().text = GaspodCollectorBuildable.AmountOfPodsMessage();
                //InterfaceHelpers.FindGameObject(home, "ClickToTake").GetComponent<Text>().text = GaspodCollectorBuildable.InstructionsMessage();
                //InterfaceHelpers.FindGameObject(home, "Battery (1)").FindChild("Battery Label").GetComponent<Text>().text = $"{GaspodCollectorBuildable.Battery()} 1";
                //InterfaceHelpers.FindGameObject(home, "Battery (2)").FindChild("Battery Label").GetComponent<Text>().text = $"{GaspodCollectorBuildable.Battery()} 2";
                #endregion

                #region ColorPicker Button

                var colorBTN = InterfaceHelpers.FindGameObject(controls, "ColorBTN");

                InterfaceHelpers.CreateButton(colorBTN, "ColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "");

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
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "");

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
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, "");

                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void OnLoadDnaGrid(GameObject itemPrefab, GameObject itemsGrid, int stPos, int endPos)
        {
            var grouped = _mono.HydroHarvContainer.Items.ToList();

            if (endPos > grouped.Count)
            {
                endPos = grouped.Count;
            }

            for (int i = stPos; i < endPos; i++)
            {
                var techType = grouped[i].Key;

                GameObject buttonPrefab = Instantiate(itemPrefab);

                if (buttonPrefab == null || itemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                buttonPrefab.transform.SetParent(itemsGrid.transform, false);

                buttonPrefab.GetComponentInChildren<Text>().text = grouped[i].Value.ToString();
                var mainButton = buttonPrefab.FindChild("MainButton");
                var mainBTN = mainButton.AddComponent<ItemButton>();
                mainBTN.Type = techType;
                mainBTN.OnButtonClick = _mono.HydroHarvContainer.RemoveItemFromContainer;
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(mainButton, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(techType);

                var deleteButton = buttonPrefab.FindChild("DeleteButton");
                var deleteBTN = deleteButton.AddComponent<ItemButton>();
                deleteBTN.Type = techType;
                deleteBTN.OnButtonClick = _mono.HydroHarvContainer.DeleteItemFromContainer;
                uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(mainButton, "Icon").AddComponent<uGUI_Icon>();
                trashIcon.sprite = SpriteManager.Get(TechType.Trashcans);
            }
        }

        internal void UpdateDna()
        {
            _dnaGrid.DrawPage();   
        }

    }
}
