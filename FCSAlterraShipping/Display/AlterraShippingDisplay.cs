using FCSAlterraShipping.Buildable;
using FCSAlterraShipping.Display.Patching;
using FCSAlterraShipping.Enums;
using FCSAlterraShipping.Models;
using FCSAlterraShipping.Mono;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCSCommon.Extensions;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace FCSAlterraShipping.Display
{
    /// <summary>
    /// A component that controls the screens UI input functions
    /// </summary>
    internal class AlterraShippingDisplay : MonoBehaviour
    {
        #region Private Members
        private GameObject _canvasGameObject;
        private bool _coroutineStarted;
        private AlterraShippingTarget _mono;
        private AlterraShippingAnimator _animatorController;
        private bool _initialized;
        private Text _message;
        private GameObject _itemsGrid;
        private Text _messagePag2;
        private Text _pageTopNumber;
        private Text _pageBottomNumber;
        private int _currentPage = 1;
        private int _maxPage = 1;
        private List<AlterraShippingTarget> _container;
        private Text _timeLeftTXT;
        private Text _shippingLabel;
        private bool _sender;
        private Text _pageName;
        private GameObject _colorPickerPage;
        private const int ITEMS_PER_PAGE = 7;
        private const int MaxContainerSpaces = AlterraShippingContainer.MaxContainerSlots;
        private const float DelayedStartTime = 0.5f;
        private const float RepeatingUpdateInterval = 1f;
        private const int Main = 1;
        private const int BaseSelect = 2;
        private const int Shipping = 3;
        private const int ColorPicker = 4;
        private const int BlackOut = 0;
        private List<ColorVec4> _serializedColors;
        private int COLORS_PER_PAGE = 48;
        private int _maxColorPage = 1;
        private int _currentColorPage = 1;
        private Text _colorPageBottomNumber;
        private Text _colorPageTopNumber;
        private GameObject _colorPageContainer;

        #endregion

        #region Public Properties
        public bool ShowBootScreen { get; set; } = true;
        public int BootTime { get; set; } = 3;
        #endregion

        #region Unity Methods
        internal void Setup(AlterraShippingTarget mono)
        {
            if (!_coroutineStarted)
                base.InvokeRepeating(nameof(UpdateDisplay), DelayedStartTime * 3f, RepeatingUpdateInterval);

            DisplayLanguagePatching.AdditionPatching();


            _mono = mono;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                return;
            }


            if (_mono == null)
            {
                QuickLogger.Error("CubeGeneratorMono component not found on the GameObject.");
                return;
            }

            if (_mono.GetTransferHandler() == null)
            {
                QuickLogger.Error($"Transfer Handler is returning null.");
                return;
            }

            _animatorController = _mono.AnimatorController;

            if (_mono != null) _mono.OnReceivingTransfer += OnReceivingTransfer;
            if (_mono != null) _mono.OnTimerChanged += OnTimerChanged;
            if (_mono != null) _mono.OnItemSent += OnItemSent;
            _initialized = true;

            _serializedColors = ColorList.Colors;


            CheckCurrentPage();

            DrawPage(1);

            DrawColorPage(1);

            ShippingTargetManager.GlobalChanged += GlobalChanged;

            InvokeRepeating("UpdateStatus", 1f, 0.5f);
        }

        private void Start()
        {
            
        }

        #endregion

        #region Internal Methods
        internal void OnButtonClick(string btnName, object additionalObject)
        {
            switch (btnName)
            {
                case "SentPackages":

                    if (!_mono.HasItems())
                    {
                        QuickLogger.Info($"There are no items to ship canceling shipment", true);
                        return;
                    }

                    TargetScreen();
                    break;

                case "OpenContainer":
                    _mono.OpenStorage();
                    _animatorController.OpenDoors();
                    break;

                case "CancelBTN":
                    MainScreen();
                    break;

                case "ColorItem":
                    var color = (Color)additionalObject;
                    ColorHandler.ChangeBodyColor("AlterraShipping_BaseColor", color, _mono.gameObject);
                    QuickLogger.Debug($"{_mono.gameObject.name} Color Changed to {color.ToString()}");
                    _mono.SetCurrentBodyColor(color);
                    break;

                case "ColorPickerBTN":
                    ColorPickerScreen();
                    break;

                case "ColorCancelBTN":
                    MainScreen();
                    break;

                case "ShippingContainer":
                    var target = additionalObject as AlterraShippingTarget;

                    if (target.IsReceivingTransfer || !target.CanFit())
                    {
                        QuickLogger.Debug($"Target Inventory doesn't have enough free slots or is receiving a shipment", true);
                        return;
                    }

                    ShippingScreen(target);
                    break;
            }
        }

        internal void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
        }

        internal void DrawPage(int page)
        {
            if (_mono == null) return;

            _currentPage = page;

            if (_currentPage <= 0)
            {
                _currentPage = 1;
            }
            else if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }

            int startingPosition = (_currentPage - 1) * ITEMS_PER_PAGE;

            int endingPosition = startingPosition + ITEMS_PER_PAGE;

            if (_mono.Manager == null) return;

            var targetCount = _mono.Manager.GetShippingTargetCount();

            QuickLogger.Debug($"Target Count: {targetCount}");

            _container = ShippingTargetManager.GlobalShippingTargets;

            if (_container == null) return;

            if (endingPosition > targetCount)
            {
                endingPosition = targetCount;
            }

            ClearPage();


            for (int i = startingPosition; i < endingPosition; i++)
            {
                LoadShippingDisplay(_container[i]);
            }

            UpdatePaginator();
        }

        internal void ChangePageBy(int amount)
        {
            DrawPage(_currentPage + amount);
        }
        #endregion

        #region Private Methods

        private bool FindAllComponents()
        {
            #region Canvas
            _canvasGameObject = this.gameObject.GetComponentInChildren<Canvas>()?.gameObject;
            if (_canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }
            #endregion

            #region Top
            var top = _canvasGameObject.FindChild("Top")?.gameObject;
            if (top == null)
            {
                QuickLogger.Error("Top not found.");
                return false;
            }
            #endregion

            #region Overlay
            var overlay = _canvasGameObject.FindChild("Overlay")?.gameObject;
            if (overlay == null)
            {
                QuickLogger.Error("Overlay not found.");
                return false;
            }
            #endregion

            #region Page_Name

            _pageName = overlay.FindChild("Page_Name").GetComponent<Text>();
            if (_pageName == null)
            {
                QuickLogger.Error("Page_Name not found.");
                return false;
            }
            #endregion

            #region Message
            _message = top.FindChild("Message").GetComponent<Text>();
            if (_message == null)
            {
                QuickLogger.Error("Message found.");
                return false;
            }
            #endregion

            #region Main
            var main = _canvasGameObject.FindChild("Main")?.gameObject;
            if (main == null)
            {
                QuickLogger.Error("Main not found.");
                return false;
            }
            #endregion

            #region Page_1
            var page1 = main.FindChild("Page_1")?.gameObject;
            if (page1 == null)
            {
                QuickLogger.Error("Page_1 not found.");
                return false;
            }
            #endregion

            #region Page_2
            var page2 = main.FindChild("Page_2")?.gameObject;
            if (page2 == null)
            {
                QuickLogger.Error("Page_2 not found.");
                return false;
            }
            #endregion

            #region Page_3
            var page3 = main.FindChild("Page_3")?.gameObject;
            if (page3 == null)
            {
                QuickLogger.Error("Page_3 not found.");
                return false;
            }
            #endregion

            #region Color Picker Page
            var colorPickerPage = main.FindChild("Color_Picker")?.gameObject;
            if (colorPickerPage == null)
            {
                QuickLogger.Error("Color_Picker not found.");
                return false;
            }

            _colorPickerPage = colorPickerPage;
            #endregion

            #region Sent_Package Button
            var sentPackages = page1.FindChild("Sent_Package")?.gameObject;
            if (sentPackages == null)
            {
                QuickLogger.Error("Sent_Package not found.");
                return false;
            }

            InterfaceButton sentPackages_BTN = sentPackages.AddComponent<InterfaceButton>();
            sentPackages_BTN.OnButtonClick = OnButtonClick;
            sentPackages_BTN.BtnName = "SentPackages";
            sentPackages_BTN.ButtonMode = InterfaceButtonMode.Background;
            sentPackages_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.SendPackageKey);
            sentPackages_BTN.Tag = this;
            #endregion

            #region Open_Container Button
            var openContainer = page1.FindChild("Open_Container")?.gameObject;
            if (openContainer == null)
            {
                QuickLogger.Error("Open_Container not found.");
                return false;
            }

            InterfaceButton openContainer_BTN = openContainer.AddComponent<InterfaceButton>();
            openContainer_BTN.OnButtonClick = OnButtonClick;
            openContainer_BTN.BtnName = "OpenContainer";
            openContainer_BTN.ButtonMode = InterfaceButtonMode.Background;
            openContainer_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.OpenShippingContainerKey);
            openContainer_BTN.Tag = this;
            #endregion

            #region Color_Picker Button
            var colorPicker = page1.FindChild("Color_Picker")?.gameObject;

            if (colorPicker == null)
            {
                QuickLogger.Error("Color_Picker not found.");
                return false;
            }

            InterfaceButton colorPicker_BTN = colorPicker.AddComponent<InterfaceButton>();
            colorPicker_BTN.OnButtonClick = OnButtonClick;
            colorPicker_BTN.BtnName = "ColorPickerBTN";
            colorPicker_BTN.ButtonMode = InterfaceButtonMode.Background;
            colorPicker_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.ColorPickerKey);
            colorPicker_BTN.Tag = this;
            #endregion

            #region Prev Button
            var prevBTN = page2.FindChild("Prev_BTN")?.gameObject;
            if (prevBTN == null)
            {
                QuickLogger.Error("Prev_BTN not found.");
                return false;
            }

            PaginatorButton prev_BTN = prevBTN.AddComponent<PaginatorButton>();
            prev_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.PrevPageKey);
            prev_BTN.ChangePageBy = ChangePageBy;
            prev_BTN.AmountToChangePageBy = -1;
            #endregion

            #region Next Button
            var nextBTN = page2.FindChild("Next_BTN")?.gameObject;
            if (nextBTN == null)
            {
                QuickLogger.Error("Next_BTN not found.");
                return false;
            }

            PaginatorButton next_BTN = nextBTN.AddComponent<PaginatorButton>();
            next_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.NextPageKey);
            next_BTN.ChangePageBy = ChangePageBy;
            next_BTN.AmountToChangePageBy = 1;
            #endregion

            #region Prev Button
            var colorPrevBTN = colorPickerPage.FindChild("Prev_BTN")?.gameObject;
            if (colorPrevBTN == null)
            {
                QuickLogger.Error("Color Prev_BTN not found.");
                return false;
            }

            PaginatorButton colorPrev_BTN = colorPrevBTN.AddComponent<PaginatorButton>();
            colorPrev_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.PrevPageKey);
            colorPrev_BTN.ChangePageBy = ChangeColorPageBy;
            colorPrev_BTN.AmountToChangePageBy = -1;
            #endregion

            #region Next Button
            var colorNextBTN = colorPickerPage.FindChild("Next_BTN")?.gameObject;
            if (colorNextBTN == null)
            {
                QuickLogger.Error("Color Next_BTN not found.");
                return false;
            }

            PaginatorButton colorNext_BTN = colorNextBTN.AddComponent<PaginatorButton>();
            colorNext_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.NextPageKey);
            colorNext_BTN.ChangePageBy = ChangeColorPageBy;
            colorNext_BTN.AmountToChangePageBy = 1;
            #endregion

            #region Cancel Button Text
            var cancelBtntxt = page2.FindChild("Cancel_BTN").GetComponent<Text>();
            if (cancelBtntxt == null)
            {
                QuickLogger.Error("Cancel_BTN Text not found.");
                return false;
            }

            cancelBtntxt.text = GetLanguage(DisplayLanguagePatching.CancelKey);
            #endregion

            #region Cancel Button
            var cancelBTN = page2.FindChild("Cancel_BTN")?.gameObject;
            if (cancelBTN == null)
            {
                QuickLogger.Error("Cancel_BTN not found.");
                return false;
            }

            InterfaceButton cancel_BTN = cancelBTN.AddComponent<InterfaceButton>();
            cancel_BTN.OnButtonClick = OnButtonClick;
            cancel_BTN.BtnName = "CancelBTN";
            cancel_BTN.ButtonMode = InterfaceButtonMode.TextColor;
            cancel_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.CancelKey);
            cancel_BTN.TextComponent = cancelBtntxt;
            cancel_BTN.Tag = this;
            #endregion

            #region Color Cancel Button Text
            var colorCancelBtntxt = colorPickerPage.FindChild("Cancel_BTN").GetComponent<Text>();
            if (colorCancelBtntxt == null)
            {
                QuickLogger.Error("Cancel_BTN Text not found.");
                return false;
            }

            colorCancelBtntxt.text = GetLanguage(DisplayLanguagePatching.ColorBackKey);
            #endregion

            #region Color Cancel Button
            var colorCancelBTN = colorPickerPage.FindChild("Cancel_BTN")?.gameObject;
            if (colorCancelBTN == null)
            {
                QuickLogger.Error("Color Cancel_BTN not found.");
                return false;
            }

            InterfaceButton colorCancel_BTN = colorCancelBTN.AddComponent<InterfaceButton>();
            colorCancel_BTN.OnButtonClick = OnButtonClick;
            colorCancel_BTN.BtnName = "ColorCancelBTN";
            colorCancel_BTN.ButtonMode = InterfaceButtonMode.TextColor;
            colorCancel_BTN.TextLineOne = GetLanguage(DisplayLanguagePatching.ColorBackKey);
            colorCancel_BTN.TextComponent = colorCancelBtntxt;
            colorCancel_BTN.Tag = this;
            #endregion

            #region Container
            _itemsGrid = page2.FindChild("Container")?.gameObject;
            if (_itemsGrid == null)
            {
                QuickLogger.Error("Container not found.");
                return false;
            }
            #endregion

            #region Color Page Container
            _colorPageContainer = colorPickerPage.FindChild("Container")?.gameObject;
            if (_colorPageContainer == null)
            {
                QuickLogger.Error("Color Page Container not found.");
                return false;
            }
            #endregion

            #region Message Page 2
            var messagePag2 = page2.FindChild("Message").GetComponent<Text>();
            if (messagePag2 == null)
            {
                QuickLogger.Error("Message Page 2 not found.");
                return false;
            }

            messagePag2.text = GetLanguage(DisplayLanguagePatching.BasePageDescriptionKey);
            #endregion

            #region Message Color Page
            var messageColorPage = colorPickerPage.FindChild("Message").GetComponent<Text>();
            if (messageColorPage == null)
            {
                QuickLogger.Error("Message Color Page not found.");
                return false;
            }

            messageColorPage.text = GetLanguage(DisplayLanguagePatching.ColorPageDescriptionKey);
            #endregion

            #region Page_Top Text
            _pageTopNumber = page2.FindChild("TopNumber").GetComponent<Text>();
            if (_pageTopNumber == null)
            {
                QuickLogger.Error("TopNumber Text not found.");
                return false;
            }
            #endregion

            #region Page_Bottom Text
            _pageBottomNumber = page2.FindChild("BottomNumber").GetComponent<Text>();
            if (_pageBottomNumber == null)
            {
                QuickLogger.Error("BottomNumber Text not found.");
                return false;
            }
            #endregion

            #region Color Page_Top Text
            _colorPageTopNumber = colorPickerPage.FindChild("TopNumber").GetComponent<Text>();
            if (_colorPageTopNumber == null)
            {
                QuickLogger.Error("Color TopNumber Text not found.");
                return false;
            }
            #endregion

            #region Color Page_Bottom Text
            _colorPageBottomNumber = colorPickerPage.FindChild("BottomNumber").GetComponent<Text>();
            if (_colorPageBottomNumber == null)
            {
                QuickLogger.Error("Color BottomNumber Text not found.");
                return false;
            }
            #endregion

            #region Shipping Text
            _shippingLabel = page3.FindChild("Shipping_LBL").GetComponent<Text>();
            if (_shippingLabel == null)
            {
                QuickLogger.Error("Shipping_LBL Text not found.");
                return false;
            }

            _shippingLabel.text = GetLanguage(DisplayLanguagePatching.ShippingKey);
            #endregion

            #region Shipping Container Label
            var shippingContainerLabel = page1.FindChild("ShippingContainerLabel")?.gameObject;

            if (shippingContainerLabel == null)
            {
                QuickLogger.Error("ShippingContainerLabel  not found.");
                return false;
            }

            shippingContainerLabel.GetComponent<Text>().text = _mono.Name;
            var alterraShippingNameController = shippingContainerLabel.AddComponent<AlterraShippingNameController>();
            alterraShippingNameController.OnLabelChanged += OnLabelChanged;
            AlterraShippingNameController.Create(_mono, shippingContainerLabel);
            #endregion

            #region Time Left Text
            var timeLeftTXT = page3.FindChild("TimeLeft_LBL").GetComponent<Text>();
            if (timeLeftTXT == null)
            {
                QuickLogger.Error("TimeLeft_LBL Text not found.");
                return false;
            }

            _timeLeftTXT = timeLeftTXT;
            #endregion

            return true;
        }

        private void OnLabelChanged()
        {
            _mono.Manager.UpdateGlobalTargets();
            DrawPage(_currentPage);
        }

        private string GetLanguage(string key)
        {
            return Language.main.Get(key);
        }

        private void UpdateDisplay()
        {
            if (!_initialized)
                return;

            _coroutineStarted = true;
        }

        private void BootScreen()
        {
            StartCoroutine(BootScreenEnu());
        }

        private void ColorPickerScreen()
        {
            StartCoroutine(ColorPickerScreenEnu());
        }

        private void ShippingScreen(AlterraShippingTarget target)
        {
            StartCoroutine(ShippingScreenEnu(target));
        }

        private void MainScreen()
        {
            StartCoroutine(MainScreenEnu());
        }

        private void TargetScreen()
        {
            StartCoroutine(TargetScreenEnu());
        }

        private void CheckCurrentPage()
        {
            if (_animatorController.GetIntHash(_mono.PageHash) == BlackOut)
            {
                _animatorController.SetIntHash(_mono.PageHash, Main);
            }
        }

        private void OnReceivingTransfer()
        {
            ShippingScreen(null);
            _mono.ContainerMode = ShippingContainerStates.Receiving;
            _animatorController.CloseDoors();
        }

        private void OnItemSent()
        {
            BootScreen();

            _mono.ContainerMode = _sender ? ShippingContainerStates.Waiting : ShippingContainerStates.PickUpAvaliable;

            if (_mono.HasItems())
            {
                _animatorController.OpenDoors();
            }
        }

        private void UpdateStatus()
        {
            switch (_mono.ContainerMode)
            {
                case ShippingContainerStates.Shipping:
                    _shippingLabel.text = GetLanguage(DisplayLanguagePatching.ShippingKey);
                    _message.text = GetLanguage(DisplayLanguagePatching.ShippingKey);
                    break;
                case ShippingContainerStates.Receiving:
                    _message.text = GetLanguage(DisplayLanguagePatching.ReceivingKey);
                    _shippingLabel.text = GetLanguage(DisplayLanguagePatching.ReceivingKey);
                    break;
                case ShippingContainerStates.Waiting:
                    _message.text = GetLanguage(DisplayLanguagePatching.WaitingKey);
                    break;
                case ShippingContainerStates.PickUpAvaliable:

                    if (_mono.NumberOfItems == 0)
                    {
                        _mono.ContainerMode = ShippingContainerStates.Waiting;
                    }
                    _message.text = GetLanguage(DisplayLanguagePatching.PickUpAvailable);
                    break;
            }
        }

        private void OnTimerChanged(string timeAsString)
        {
            _timeLeftTXT.text = $"{GetLanguage(DisplayLanguagePatching.TimeLeftKey)} {timeAsString}";
        }

        private void GlobalChanged()
        {
            DrawPage(_currentPage);
        }

        private void LoadShippingDisplay(AlterraShippingTarget storageItemName)
        {
            if (storageItemName.GetInstanceID() == _mono.GetInstanceID()) return;

            QuickLogger.Debug("Load Shipping Display");
            if (storageItemName == null) return;
            GameObject itemDisplay = Instantiate(AlterraShippingBuildable.ItemPrefab);

            itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            var text = itemDisplay.transform.Find("Location_LBL").GetComponent<Text>();
            text.text = storageItemName.Name;

            var itemButton = itemDisplay.AddComponent<InterfaceButton>();
            itemButton.ButtonMode = InterfaceButtonMode.TextColor;
            itemButton.Tag = storageItemName;
            itemButton.TextComponent = text;
            itemButton.OnButtonClick += OnButtonClick;
            itemButton.BtnName = "ShippingContainer";
        }

        private void UpdatePaginator()
        {
            CalculateNewMaxPages();
            _pageTopNumber.text = _currentPage.ToString();
            _pageBottomNumber.text = _maxPage.ToString();
        }

        private void CalculateNewMaxPages()
        {
            _maxPage = Mathf.CeilToInt((_container.Count - 1) / ITEMS_PER_PAGE) + 1;
            if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }

        private void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                var item = _itemsGrid.transform.GetChild(i).gameObject;
                Destroy(item);
            }
        }

        private void CalculateNewMaxColorPages()
        {
            _maxColorPage = Mathf.CeilToInt((_serializedColors.Count - 1) / COLORS_PER_PAGE) + 1;
            if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }
        }

        private void DrawColorPage(int page)
        {
            _currentColorPage = page;

            if (_currentColorPage <= 0)
            {
                _currentColorPage = 1;
            }
            else if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }

            int startingPosition = (_currentColorPage - 1) * COLORS_PER_PAGE;
            int endingPosition = startingPosition + COLORS_PER_PAGE;

            //QuickLogger.Debug($"_currentColorPage: {_currentColorPage} || startingPosition: {startingPosition} || endingPosition: {endingPosition} || COLORS_PER_PAGE {COLORS_PER_PAGE}", true);

            if (endingPosition > _serializedColors.Count)
            {
                endingPosition = _serializedColors.Count;
            }

            ClearColorPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                //QuickLogger.Debug($"Found: {_serializedColors.Count} colors || Element:{i}", true);
                var colorID = _serializedColors.ElementAt(i);
                LoadColorPicker(colorID);
            }

            UpdateColorPaginator();
        }

        private void LoadColorPicker(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(AlterraShippingBuildable.ColorItemPrefab);
            itemDisplay.transform.SetParent(_colorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.Vector4ToColor();
        }

        private void UpdateColorPaginator()
        {
            CalculateNewMaxColorPages();
            _colorPageTopNumber.text = _currentColorPage.ToString();
            _colorPageBottomNumber.text = _maxColorPage.ToString();
        }

        private void ClearColorPage()
        {
            for (int i = 0; i < _colorPageContainer.transform.childCount; i++)
            {
                Destroy(_colorPageContainer.transform.GetChild(i).gameObject);
            }
        }
        #endregion

        #region IEnumerators
        private IEnumerator MainScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _pageName.text = GetLanguage(DisplayLanguagePatching.MainKey);
            _animatorController.SetIntHash(_mono.PageHash, Main);

            if (_mono.HasItems())
            {
                _animatorController.OpenDoors();
            }
        }

        private IEnumerator ShippingScreenEnu(AlterraShippingTarget target)
        {
            yield return new WaitForEndOfFrame();

            if (_animatorController.GetIntHash(_mono.PageHash) == Shipping) yield break;

            _animatorController.SetIntHash(_mono.PageHash, Shipping);

            if (target == null) yield break;
            _sender = true;
            _mono.ContainerMode = ShippingContainerStates.Shipping;
            _mono.TransferItems(target);
        }

        private IEnumerator BootScreenEnu()
        {
            yield return new WaitForEndOfFrame();

            if (ShowBootScreen)
            {
                _pageName.text = GetLanguage(DisplayLanguagePatching.MainKey);
                _animatorController.SetIntHash(_mono.PageHash, Main);
                yield return new WaitForSeconds(BootTime);
            }

            MainScreen();
        }

        private IEnumerator TargetScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _pageName.text = GetLanguage(DisplayLanguagePatching.ShippingKey);
            _animatorController.CloseDoors();
            _animatorController.SetIntHash(_mono.PageHash, BaseSelect);
        }

        private IEnumerator ColorPickerScreenEnu()
        {
            yield return new WaitForEndOfFrame();
            _pageName.text = GetLanguage(DisplayLanguagePatching.ColorPickerKey);
            _animatorController.SetIntHash(_mono.PageHash, ColorPicker);
        }
        #endregion
    }
}
