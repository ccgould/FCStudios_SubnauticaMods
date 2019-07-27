using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Display;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private bool _initialized = true;

        public Color colorEmpty = new Color(1f, 0f, 0f, 1f);

        public Color colorHalf = new Color(1f, 1f, 0f, 1f);

        public Color colorFull = new Color(0f, 1f, 0f, 1f);
        private Text _s1Percent;
        private Image _s1Fill;
        private Text _s2Percent;
        private Image _s2Fill;
        private Text _s3Percent;
        private Image _s3Fill;
        private Text _s4Percent;
        private Image _s4Fill;
        private GameObject _grid;
        private Text _pageNumber;
        private List<InterfaceButton> OreButtons = new List<InterfaceButton>();
        private Text _focusBtnText;


        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            if (!FindAllComponents())
            {
                QuickLogger.Error("Unable to find all components");
                _initialized = false;
            }
            ITEMS_PER_PAGE = 5;
            DrawPage(1);
        }

        public override void ClearPage()
        {
            for (int i = 0; i < _grid.transform.childCount; i++)
            {
                Destroy(_grid.transform.GetChild(i).gameObject);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (!_initialized)
            {
                QuickLogger.Error("Deep Driller failed to initialize. All button events are disabled.");
                return;
            }

            switch (btnName)
            {
                case "Open_BTN":
                    _mono.DeepDrillerContainer.OpenStorage();
                    break;

                case "PowerBTN":
                    switch (_mono.PowerManager.GetPowerState())
                    {
                        case FCSPowerStates.Powered:
                            _mono.PowerOffDrill();
                            break;
                        case FCSPowerStates.None:
                            _mono.PowerOnDrill();
                            break;
                        case FCSPowerStates.Tripped:
                            _mono.PowerOnDrill();
                            break;
                        case FCSPowerStates.Unpowered:
                            _mono.PowerOnDrill();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case "Module_Door":
                    _mono.DeepDrillerModuleContainer.OpenModulesDoor();
                    break;
                case "ListItem":
                    _mono.SetOreFocus((TechType)tag);
                    UpdateListItems((TechType)tag);
                    break;
                case "Focus":
                    _mono.ToggleFocus();
                    UpdateFocusStates();
                    break;
            }
        }

        private void UpdateFocusStates()
        {
            if (_mono.GetFocusedState())
            {
                _focusBtnText.text = FCSDeepDrillerBuildable.Focusing();

            }
            else
            {
                _focusBtnText.text = FCSDeepDrillerBuildable.Focus();
                RemoveFocusOnItems();
            }
        }

        private void RemoveFocusOnItems()
        {
            foreach (InterfaceButton interfaceButton in OreButtons)
            {
                interfaceButton.RemoveFocus();
            }
        }

        public override void ItemModified<T>(T item)
        {
            QuickLogger.Debug("In ItemModified");
            DrawPage(1);
        }

        public override bool FindAllComponents()
        {
            QuickLogger.Debug("Find All Components");

            #region Canvas

            var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }

            #endregion

            #region Power Button

            var powerBTN = canvasGameObject.FindChild("PowerBTN")?.gameObject;

            if (powerBTN == null)
            {
                QuickLogger.Error("Power Button not found.");
                return false;
            }

            var powerBtn = powerBTN.AddComponent<InterfaceButton>();
            powerBtn.OnButtonClick = OnButtonClick;
            powerBtn.BtnName = "PowerBTN";
            powerBtn.ButtonMode = InterfaceButtonMode.Background;
            powerBtn.TextLineOne = $"Toggle {Mod.ModFriendlyName} Power";
            #endregion

            #region Open Storage Button

            var openStorageBTN = canvasGameObject.FindChild("Open_BTN").FindChild("OPEN_LBL")?.gameObject;

            if (openStorageBTN == null)
            {
                QuickLogger.Error("Open Storage Button not found.");
                return false;
            }

            var openStorageBtn = openStorageBTN.AddComponent<InterfaceButton>();
            openStorageBtn.OnButtonClick = OnButtonClick;
            openStorageBtn.BtnName = "Open_BTN";
            openStorageBtn.ButtonMode = InterfaceButtonMode.TextColor;
            openStorageBtn.TextComponent = openStorageBtn.GetComponent<Text>();
            openStorageBtn.TextLineOne = $"Open {Mod.ModFriendlyName} Storage";

            #endregion

            #region Open Modules Button

            var openModuleDoor = canvasGameObject.FindChild("Module_BTN").FindChild("OPEN_LBL")?.gameObject;

            if (openModuleDoor == null)
            {
                QuickLogger.Error("Open module door not found.");
                return false;
            }

            var moduleDoor = openModuleDoor.AddComponent<InterfaceButton>();
            moduleDoor.OnButtonClick = OnButtonClick;
            moduleDoor.BtnName = "Module_Door";
            moduleDoor.ButtonMode = InterfaceButtonMode.TextColor;
            moduleDoor.TextComponent = openModuleDoor.GetComponent<Text>();
            moduleDoor.TextLineOne = $"Open {Mod.ModFriendlyName} Modular";
            #endregion

            var main = gameObject.FindChild("model").FindChild("Scanner_Screen_Attachment").FindChild("Canvas").FindChild("Home")?.gameObject;

            #region Slot1
            var slot1 = main.FindChild("Battery_1")?.gameObject;
            if (slot1 == null)
            {
                QuickLogger.Error("Battery_1 cannot be found");
                return false;
            }

            _s1Percent = slot1.FindChild("percentage").GetComponent<Text>();
            _s1Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s1Fill = slot1.FindChild("Fill").GetComponent<Image>();
            _s1Fill.color = colorEmpty;
            _s1Fill.fillAmount = 0f;
            #endregion

            #region Slot2
            var slot2 = main.FindChild("Battery_2")?.gameObject;
            if (slot2 == null)
            {
                QuickLogger.Error("Battery_2 cannot be found");
                return false;
            }

            _s2Percent = slot2.FindChild("percentage").GetComponent<Text>();
            _s2Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s2Fill = slot2.FindChild("Fill").GetComponent<Image>();
            _s2Fill.color = colorEmpty;
            _s2Fill.fillAmount = 0f;
            #endregion

            #region Slot3
            var slot3 = main.FindChild("Battery_3")?.gameObject;
            if (slot3 == null)
            {
                QuickLogger.Error("Battery_3 cannot be found");
                return false;
            }

            _s3Percent = slot3.FindChild("percentage").GetComponent<Text>();
            _s3Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s3Fill = slot3.FindChild("Fill").GetComponent<Image>();
            _s3Fill.color = colorEmpty;
            _s3Fill.fillAmount = 0f;
            #endregion

            #region Slot4
            var slot4 = main.FindChild("Battery_4")?.gameObject;
            if (slot4 == null)
            {
                QuickLogger.Error("Battery_4 cannot be found");
                return false;
            }

            _s4Percent = slot4.FindChild("percentage").GetComponent<Text>();
            _s4Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s4Fill = slot4.FindChild("Fill").GetComponent<Image>();
            _s4Fill.color = colorEmpty;
            _s4Fill.fillAmount = 0f;
            #endregion

            _grid = main.FindChild("Grid")?.gameObject;

            if (_grid == null)
            {
                QuickLogger.Error("Could not find the grid");
                return false;
            }

            var previousPageGameObject = main.FindChild("Arrow_Up")?.gameObject;

            if (previousPageGameObject == null)
            {
                QuickLogger.Error("Could not find the Arrow_Up");
                return false;

            }

            var prevPageBTN = previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.OnChangePageBy = ChangePageBy;
            prevPageBTN.AmountToChangePageBy = -1;
            prevPageBTN.HoverTextLineTwo = FCSDeepDrillerBuildable.PrevPage();


            var nextPageGameObject = main.FindChild("Arrow_Down")?.gameObject;

            if (nextPageGameObject == null)
            {
                QuickLogger.Error("Could not find the Arrow_Down");
                return false;

            }

            var nextPageBTN = nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.OnChangePageBy = ChangePageBy;
            nextPageBTN.AmountToChangePageBy = 1;
            nextPageBTN.HoverTextLineTwo = FCSDeepDrillerBuildable.NextPage();

            _pageNumber = main.FindChild("Paginator").GetComponent<Text>();

            if (_pageNumber == null)
            {
                QuickLogger.Error("Could not find the Paginator");
                return false;
            }

            var focusBtn = main.FindChild("Button")?.gameObject;

            if (focusBtn == null)
            {
                QuickLogger.Error("Could not find the Button");
                return false;
            }

            var focusBTN = focusBtn.AddComponent<InterfaceButton>();
            focusBTN.BtnName = "Focus";
            focusBTN.OnButtonClick = OnButtonClick;
            focusBTN.ButtonMode = InterfaceButtonMode.TextColor;
            focusBTN.TextComponent = focusBtn.FindChild("Text").GetComponent<Text>();

            _focusBtnText = focusBtn.gameObject.GetComponentInChildren<Text>();
            return true;
        }

        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            throw new NotImplementedException();
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


            if (endingPosition > _mono.GetBiomeData().Count)
            {
                endingPosition = _mono.GetBiomeData().Count;
            }

            ClearPage();

            OreButtons.Clear();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var techType = _mono.GetBiomeData().ElementAt(i);
                LoadDisplay(techType);
            }

            UpdatePaginator();

            UpdateListItems(_mono.GetFocusedOre());
        }

        private void LoadDisplay(TechType techType)
        {
            GameObject itemDisplay = Instantiate(FCSDeepDrillerBuildable.ItemPrefab);
            itemDisplay.transform.SetParent(_grid.transform, false);

            var itemButton = itemDisplay.AddComponent<InterfaceButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ListItem";
            itemButton.Tag = techType;
            itemButton.ButtonMode = InterfaceButtonMode.HoverImage;
            itemButton.HoverItemName = "MouseOver";

            var icon = itemDisplay.gameObject.FindChild("Icon")?.gameObject;
            var text = itemDisplay.GetComponentInChildren<Text>();
            text.text = techType.ToString();

            if (icon == null)
            {
                QuickLogger.Error("Cannot find gameobject Icon");
                return;
            }

            var uiIcon = icon.AddComponent<uGUI_Icon>();
            uiIcon.sprite = SpriteManager.Get(techType);

            OreButtons.Add(itemButton);
        }

        public override void UpdatePaginator()
        {
            CalculateNewMaxPages();
            _pageNumber.text = $"{CurrentPage.ToString()} | {MaxPage}";
        }

        private void CalculateNewMaxPages()
        {
            MaxPage = Mathf.CeilToInt((_mono.GetBiomeData().Count - 1) / ITEMS_PER_PAGE) + 1;
            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }
        }

        internal void UpdateVisuals(PowerUnitData data)
        {
            Text text = null;
            Image bar = null;

            var charge = data.Battery.charge < 1 ? 0f : data.Battery.charge;

            float percent = charge / data.Battery.capacity;

            QuickLogger.Debug($"P: {percent} | S: {data.Slot} | BDC {data.Battery.charge}");

            if (data.Slot == EquipmentConfiguration.SlotIDs[0])
            {
                text = _s1Percent;
                bar = _s1Fill;
            }
            else if (data.Slot == EquipmentConfiguration.SlotIDs[1])
            {
                text = _s2Percent;
                bar = _s2Fill;
            }
            else if (data.Slot == EquipmentConfiguration.SlotIDs[2])
            {
                text = _s3Percent;
                bar = _s3Fill;
            }
            else if (data.Slot == EquipmentConfiguration.SlotIDs[3])
            {
                text = _s4Percent;
                bar = _s4Fill;
            }

            if (text != null)
            {
                text.text = ((data.Battery.charge < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
            }

            if (bar != null)
            {
                if (data.Battery.charge >= 0f)
                {
                    Color value = (percent >= 0.5f) ? Color.Lerp(this.colorHalf, this.colorFull, 2f * percent - 1f) : Color.Lerp(this.colorEmpty, this.colorHalf, 2f * percent);
                    bar.color = value;
                    bar.fillAmount = percent;
                }
                else
                {
                    bar.color = colorEmpty;
                    bar.fillAmount = 0f;
                }
            }
        }

        internal void EmptyBatteryVisual(string slot)
        {
            Text text = null;
            Image bar = null;

            if (slot == EquipmentConfiguration.SlotIDs[0])
            {
                text = _s1Percent;
                bar = _s1Fill;
            }
            else if (slot == EquipmentConfiguration.SlotIDs[1])
            {
                text = _s2Percent;
                bar = _s2Fill;
            }
            else if (slot == EquipmentConfiguration.SlotIDs[2])
            {
                text = _s3Percent;
                bar = _s3Fill;
            }
            else if (slot == EquipmentConfiguration.SlotIDs[3])
            {
                text = _s4Percent;
                bar = _s4Fill;
            }

            if (text != null)
            {
                text.text = Language.main.Get("ChargerSlotEmpty");
            }

            if (bar != null)
            {
                bar.color = colorEmpty;
                bar.fillAmount = 0f;
            }
        }

        internal void UpdateListItems(TechType techType)
        {
            foreach (InterfaceButton button in OreButtons)
            {
                if ((TechType)button.Tag == techType)
                {
                    button.Focus();
                    QuickLogger.Debug($"Match found for TechType {techType}", true);
                }
                else
                {
                    button.RemoveFocus();
                }
            }

            UpdateFocusStates();
        }
    }
}
