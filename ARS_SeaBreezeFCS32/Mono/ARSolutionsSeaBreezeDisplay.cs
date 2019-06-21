using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Display;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezeDisplay : AIDisplay
    {
        #region Private Members
        private ARSolutionsSeaBreezeController _mono;
        private List<EatableEntities> _container;
        private GameObject _canvasGameObject;
        private GameObject _model;
        private GameObject _homeScreen;
        private GameObject _filterBtn;
        private GameObject _itemsGrid;
        private GameObject _homeScreenPowerBtn;
        private GameObject _pagination;
        private GameObject _background;
        private GameObject _blackOut;
        private GameObject _poweredOffScreen;
        private GameObject _poweredScreenPowerBtn;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _pageCounter;
        private Text _pageCounterText;
        private int _pageStateHash;
        private readonly float BOOTING_ANIMATION_TIME = 6f;
        private readonly float WELCOME_SCREEN_TIME = 2f;
        private readonly Dictionary<TechType, int> TrackedItems = new Dictionary<TechType, int>();
        private bool _completeSetup;

        #endregion

        #region Internal Methods    
        internal void Setup(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                QuickLogger.Debug("Turning off display from find components", true);
                PowerOffDisplay();
                return;
            }

            ITEMS_PER_PAGE = 16;

            _pageStateHash = UnityEngine.Animator.StringToHash("PageState");

            _mono.PowerManager.OnPowerOutage += OnPowerOutage;
            _mono.PowerManager.OnPowerResume += OnPowerResume;

            StartCoroutine(CompleteSetup());

            DrawPage(1);

            InvokeRepeating("UpdateScreenState", 1, 0.5f);
        }

        private void OnPowerResume()
        {
            QuickLogger.Debug("Power Restored", true);
            if (_mono.PowerManager.GetHasBreakerTripped()) return;
            StartCoroutine(CompleteSetup());
        }

        private void OnPowerOutage()
        {
            QuickLogger.Debug("Power Outage", true);
            ShutDownDisplay();
        }

        private void UpdateScreenState()
        {
            if (!_mono.PowerManager.GetHasBreakerTripped() || !_mono.PowerManager.GetIsPowerAvailable()) return;
            PowerOffDisplay();
        }

        internal void UpdateTimer(string time)
        {
            _filterLBL.text = $"Filter Change in: {time}";
        }
        #endregion

        #region Public Methods
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
                    //_mono.UpdateFridgeCooler();
                    _mono.ResetOnInterceButton();
                    StartCoroutine(CompleteSetup());
                    break;

                case "HPPBtn":
                    _mono.PowerManager.TogglePower();
                    // _mono.UpdateFridgeCooler();
                    _mono.ResetOnInterceButton();
                    PowerOffDisplay();
                    break;

                case "FilterBtn":
                    _mono.OpenFilterContainer();
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
            _filterLBL = _homeScreen.transform.Find("FilterTimer_LBL").GetComponent<Text>();

            if (_filterLBL == null)
            {
                QuickLogger.Error("Screen: Filter label not found.");
                return false;
            }
            #endregion

            #region Home Screen Power BTN
            _homeScreenPowerBtn = _homeScreen.transform.Find("Power_BTN")?.gameObject;

            if (_homeScreenPowerBtn == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }


            var homeScreenPowerBTN = _homeScreenPowerBtn.AddComponent<InterfaceButton>();
            homeScreenPowerBTN.OnButtonClick = OnButtonClick;
            homeScreenPowerBTN.ButtonMode = InterfaceButtonMode.Background;
            homeScreenPowerBTN.BtnName = "HPPBtn";
            homeScreenPowerBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Paginator
            _pagination = _homeScreen.FindChild("Paginator")?.gameObject;
            if (_pagination == null)
            {
                QuickLogger.Error("Screen: Paginator not found.");
                return false;
            }
            #endregion

            #region Filter
            _filterBtn = _homeScreen.transform.Find("Filter_BTN")?.gameObject;

            if (_filterBtn == null)
            {
                QuickLogger.Error("Screen: Filter Button not found.");
                return false;
            }

            var filterrBTN = _filterBtn.AddComponent<InterfaceButton>();
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
            _poweredScreenPowerBtn = _poweredOffScreen.transform.Find("Power_BTN")?.gameObject;
            if (_poweredScreenPowerBtn == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }

            var poweredOffScreenBTN = _poweredScreenPowerBtn.AddComponent<InterfaceButton>();
            poweredOffScreenBTN.OnButtonClick = OnButtonClick;
            poweredOffScreenBTN.BtnName = "PPBtn";
            poweredOffScreenBTN.ButtonMode = InterfaceButtonMode.Background;
            poweredOffScreenBTN.OnInterfaceButton = _mono.OnInterfaceButton;
            #endregion

            #region Prev Page BTN
            _previousPageGameObject = _pagination.FindChild("PreviousPage")?.gameObject;
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
            _nextPageGameObject = _pagination.FindChild("NextPage")?.gameObject;
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
            _pageCounter = _pagination.FindChild("PageNumber")?.gameObject;
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

        public Text _filterLBL { get; set; }

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

        public override void UpdatePaginator()
        {
            base.UpdatePaginator();

            CalculateNewMaxPages();
            _pageCounter.SetActive(_mono.NumberOfItems != 0);
            _pageCounterText.text = $"{CurrentPage} / {MaxPage}";
            _previousPageGameObject.SetActive(CurrentPage != 1);
            _nextPageGameObject.SetActive(CurrentPage != MaxPage);
        }
        #endregion

        #region IEnumerators    
        public override IEnumerator PowerOff()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 3);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator PowerOn()
        {
            // QuickLogger.Debug("In Power On", true);
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 2);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator ShutDown()
        {
            //QuickLogger.Debug("In Shut Down", true);
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 0);
            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator CompleteSetup()
        {
            QuickLogger.Debug("In Complete Setup", true);
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 1);
            _completeSetup = true;
        }
        #endregion

        #region Private Methods
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
        #endregion
    }
}
