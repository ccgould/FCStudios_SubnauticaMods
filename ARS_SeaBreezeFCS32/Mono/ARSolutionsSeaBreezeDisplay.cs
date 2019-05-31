using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Display;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezeDisplay : AIDisplay
    {
        private ARSolutionsSeaBreezeController _mono;
        private int _amount;
        private List<EatableEntities> _container;
        private GameObject _canvasGameObject;
        private GameObject _model;
        private GameObject _homeScreen;
        private GameObject _filterBTN;
        private GameObject _itemsGrid;
        private GameObject _homeScreenPowerBTN;
        private GameObject _paginator;
        private GameObject _background;
        private GameObject _blackOut;
        private GameObject _poweredOffScreen;
        private GameObject _poweredScreenPowerBTN;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _pageCounter;
        private Text _pageCounterText;
        private int _pageStateHash;
        private readonly float BOOTING_ANIMATION_TIME = 6f;
        private readonly float WELCOME_SCREEN_TIME = 2f;
        private Dictionary<TechType, int> TrackedItems = new Dictionary<TechType, int>();


        internal void Setup(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                PowerOffDisplay();
                return;
            }

            ITEMS_PER_PAGE = 16;

            _pageStateHash = UnityEngine.Animator.StringToHash("PageState");

            StartCoroutine(PowerOn());

            DrawPage(1);
        }

        public override void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                var item = _itemsGrid.transform.GetChild(i).gameObject;
                Destroy(item);
            }
            TrackedItems.Clear();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PPBtn":
                    _mono.PowerManager.TogglePower();
                    StartCoroutine(PowerOn());
                    break;

                case "HPPBtn":
                    _mono.PowerManager.TogglePower();
                    StartCoroutine(PowerOff());
                    break;

                case "FilterBtn":
                    _mono.OpenFilterContainer();
                    break;
                default:
                    break;
            }
        }

        public override void ItemModified<T>(T item)
        {
            DrawPage(1);
        }

        public override bool FindAllComponents()
        {
            QuickLogger.Debug("Find All Components");

            #region Canvas

            _canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (_canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }

            #endregion

            // == Canvas Elements == //

            #region Model

            _model = gameObject.FindChild("model")?.gameObject;

            if (_model == null)
            {
                QuickLogger.Error("Screen: Home Screen not found.");
                return false;
            }
            #endregion

            #region Home Screen
            _homeScreen = _canvasGameObject.transform.Find("HomeScreen")?.gameObject;
            if (_homeScreen == null)
            {
                QuickLogger.Error("Screen: Home Screen not found.");
                return false;
            }
            #endregion

            #region Home Screen Timer
            _filterBTN = _homeScreen.transform.Find("FilterTimer_LBL")?.gameObject;

            if (_filterBTN == null)
            {
                QuickLogger.Error("Screen: Filter label not found.");
                return false;
            }
            #endregion

            #region Home Screen Power BTN
            _homeScreenPowerBTN = _homeScreen.transform.Find("Power_BTN")?.gameObject;

            if (_homeScreenPowerBTN == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }


            var homeScreenPowerBTN = _homeScreenPowerBTN.AddComponent<InterfaceButton>();
            homeScreenPowerBTN.OnButtonClick = OnButtonClick;
            homeScreenPowerBTN.ButtonMode = InterfaceButtonMode.Background;
            homeScreenPowerBTN.BtnName = "HPPBtn";
            homeScreenPowerBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Paginator
            _paginator = _homeScreen.FindChild("Paginator")?.gameObject;
            if (_paginator == null)
            {
                QuickLogger.Error("Screen: Paginator not found.");
                return false;
            }
            #endregion

            #region Filter
            _filterBTN = _homeScreen.transform.Find("Filter_BTN")?.gameObject;

            if (_filterBTN == null)
            {
                QuickLogger.Error("Screen: Filter Button not found.");
                return false;
            }

            var filterrBTN = _filterBTN.AddComponent<InterfaceButton>();
            filterrBTN.OnButtonClick = OnButtonClick;
            filterrBTN.ButtonMode = InterfaceButtonMode.Background;
            filterrBTN.BtnName = "FilterBtn";
            filterrBTN.TextLineOne = "Open Filter Drive";
            filterrBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Items Grid
            _itemsGrid = _canvasGameObject.transform.Find("HomeScreen").Find("ItemsGrid")?.gameObject;
            if (_itemsGrid == null)
            {
                QuickLogger.Error("Screen: Item Grid not found.");
                return false;
            }
            #endregion

            #region Background
            _background = _canvasGameObject.transform.Find("Background")?.gameObject;
            if (_background == null)
            {
                QuickLogger.Error("Screen: Background not found.");
                return false;
            }
            #endregion

            #region BlackOut
            _blackOut = _canvasGameObject.transform.Find("BlackOut")?.gameObject;
            if (_blackOut == null)
            {
                QuickLogger.Error("Screen: BlackOut not found.");
                return false;
            }
            #endregion

            #region PowerOff
            _poweredOffScreen = _canvasGameObject.transform.Find("PoweredOffScreen")?.gameObject;
            if (_poweredOffScreen == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen not found.");
                return false;
            }
            #endregion

            #region Power BTN
            _poweredScreenPowerBTN = _poweredOffScreen.transform.Find("Power_BTN")?.gameObject;
            if (_poweredScreenPowerBTN == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }

            var poweredOffScreenBTN = _poweredScreenPowerBTN.AddComponent<InterfaceButton>();
            poweredOffScreenBTN.OnButtonClick = OnButtonClick;
            poweredOffScreenBTN.BtnName = "PPBtn";
            poweredOffScreenBTN.ButtonMode = InterfaceButtonMode.Background;
            poweredOffScreenBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Prev Page BTN
            _previousPageGameObject = _paginator.FindChild("PreviousPage")?.gameObject;
            if (_previousPageGameObject == null)
            {
                QuickLogger.Error("Screen: PreviousPage not found.");
                return false;
            }

            var prevPageBTN = _previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.OnChangePageBy = ChangePageBy;
            prevPageBTN.AmountToChangePageBy = -1;
            prevPageBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Next Page BTN
            _nextPageGameObject = _paginator.FindChild("NextPage")?.gameObject;
            if (_nextPageGameObject == null)
            {
                QuickLogger.Error("Screen: NextPage not found.");
                return false;
            }

            var nextPageBTN = _nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.OnChangePageBy = ChangePageBy;
            nextPageBTN.AmountToChangePageBy = 1;
            nextPageBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Color Picker Page Counter
            _pageCounter = _paginator.FindChild("PageNumber")?.gameObject;
            if (_pageCounter == null)
            {
                QuickLogger.Error("Screen: PageNumber not found.");
                return false;
            }

            _pageCounterText = _pageCounter.GetComponent<Text>();
            if (_pageCounterText == null)
            {
                QuickLogger.Error("Screen: _pageCounterText not found.");
                return false;
            }
            #endregion
            return true;
        }

        public override IEnumerator PowerOff()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetFloatHash(_pageStateHash, 4);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator PowerOn()
        {
            QuickLogger.Debug("In Power On", true);
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 1);
            yield return new WaitForSeconds(BOOTING_ANIMATION_TIME);
            _mono.AnimationManager.SetIntHash(_pageStateHash, 2);
            yield return new WaitForSeconds(WELCOME_SCREEN_TIME);
            _mono.AnimationManager.SetIntHash(_pageStateHash, 3);
            yield return new WaitForEndOfFrame();
            QuickLogger.Debug("In Power On Done", true);
        }

        public override IEnumerator ShutDown()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetFloatHash(_pageStateHash, 0);
            yield return new WaitForEndOfFrame();
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

            QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);
            QuickLogger.Debug($"Number Of Items: {_mono.NumberOfItems}");
            _container = _mono.FridgeItems;


            if (endingPosition > _container.Count)
            {
                endingPosition = _container.Count;
            }

            QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);

            ClearPage();

            QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var element = _container.ElementAt(i);
                var techType = element.TechType;

                QuickLogger.Debug($"Element: {element} || TechType : {techType}");

                if (TrackedItems.ContainsKey(techType))
                {
                    TrackedItems[techType] = TrackedItems[techType] + 1;
                }
                else
                {
                    TrackedItems.Add(techType, 1);
                }
            }

            foreach (var storageItem in TrackedItems)
            {
                LoadFridgeDisplay(storageItem.Key, storageItem.Value);
            }

            UpdatePaginator();
        }

        private bool CheckIfItemExist(TechType techType)
        {
            foreach (Transform child in _itemsGrid.transform)
            {
                var itemButton = child.GetComponent<ItemButton>();

                if (itemButton == null) continue;

                if (itemButton.Type == techType)
                {
                    QuickLogger.Debug("Item Found", true);
                    itemButton.Amount = _mono.GetTechTypeAmount(techType);
                    return true;
                }
            }

            return false;
        }

        public override void UpdatePaginator()
        {
            base.UpdatePaginator();

            CalculateNewMaxPages();
            _pageCounter.SetActive(_mono.NumberOfItems != 0);
            _pageCounterText.text = $"{CurrentPage} / {MaxPage}";
            _previousPageGameObject.SetActive(CurrentPage != 1);
            _nextPageGameObject.SetActive(CurrentPage != MaxPage);
        }

        private void CalculateNewMaxPages()
        {
            MaxPage = Mathf.CeilToInt((_mono.NumberOfItems - 1) / ITEMS_PER_PAGE) + 1;
            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }
        }

        private void LoadFridgeDisplay(TechType type, int amount)
        {
            QuickLogger.Debug("Load Fridge Display");

            GameObject itemDisplay = Instantiate(ARSSeaBreezeFCS32Buildable.ItemPrefab);

            itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            itemDisplay.GetComponentInChildren<Text>().text = "x" + amount;

            ItemButton itemButton = itemDisplay.AddComponent<ItemButton>();
            itemButton.Type = type;
            itemButton.Amount = amount;

            uGUI_Icon icon = itemDisplay.transform.Find("ItemImage").gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(type);
        }

    }
}
