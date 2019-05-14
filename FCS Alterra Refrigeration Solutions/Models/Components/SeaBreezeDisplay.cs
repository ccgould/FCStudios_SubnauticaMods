using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Models.Base;
using FCS_Alterra_Refrigeration_Solutions.Models.Interfaces;
using FCSPowerStorage.Model.Components;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace FCS_Alterra_Refrigeration_Solutions.Models.Components
{
    public class SeaBreezeDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IARSolutionDisplay<ARSolutionsSeaBreezeController>
    {
        #region Private Members
        private GameObject _seaBreeze;
        private GameObject _background;
        private GameObject SEA_BREEZE_ITEM;
        private GameObject _blackOut;
        private readonly float BOOTING_ANIMATION_TIME = 6f;
        private readonly float WELCOME_SCREEN_TIME = 2f;
        private readonly float RESET_TIMER = 0.5f;
        private int _currentPage = 1;
        private Text _pageCounterText;
        private int _maxPage = 1;
        private readonly int ITEMS_PER_PAGE = 16;
        private ItemsContainer _container;
        private GameObject _pageCounter;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _itemsGrid;
        private GameObject _poweredOffScreen;
        private GameObject _poweredScreenPowerBTN;
        private GameObject _homeScreen;
        private GameObject _homeScreenPowerBTN;
        private readonly float HOME_SCREEN_TIME = 2f;
        private readonly float ON_OFF_TIMER = 2f;
        private GameObject _filterBTN;
        private readonly bool _driveState = true;
        private GameObject _model;
        private GameObject _paginator;
        private readonly GameObject _prevPageGameObject;

        #endregion

        #region Public Properties
        public GameObject CanvasGameObject { get; set; }
        public Animator Animator { get; private set; }
        public float MAX_INTERACTION_DISTANCE { get; set; } = 2.5f;
        public ARSolutionsSeaBreezeController Controller { get; set; }
        public void ChangePageBy(int amount)
        {
            DrawPage(_currentPage + amount);
        }

        public GameObject FilterLBL { get; set; }
        //public GameObject LongTermFilter { get; set; }
        //public GameObject ShortTermFilter { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// The entry point that sets up the class 
        /// </summary>
        /// <param name="arscontroller"></param>
        public void Setup(ARSolutionsSeaBreezeController arscontroller)
        {
            SEA_BREEZE_ITEM = LoadItems.ASSETBUNDLE.LoadAsset<GameObject>("ARSItem");
            if (arscontroller.IsBeingDeleted) return;
            Controller = arscontroller;

            Log.Info($"Controller Storage Items Count {Controller.StorageItems.Count}");

            if (FindAllComponents() == false)
            {
                Log.Error("// ============== Error getting all Components ============== //");
                TurnDisplayOff();
                return;
            }

            //StartCoroutine(CompleteSetup());
        }

        public void TurnDisplayOff()
        {
            Animator.SetBool("Reboot", true);
        }

        public void ResetIdleTimer()
        {

        }

        public void OnButtonClick(string btnName, GameObject page = null)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PPBtn":
                    Controller.HasBreakerTripped = false;
                    StartCoroutine(PowerOn());
                    break;

                case "HPPBtn":
                    Controller.HasBreakerTripped = true;
                    StartCoroutine(PowerOff());
                    break;

                case "FilterBtn":
                    StartCoroutine(OpenDrive());
                    Controller.OnUse();
                    break;
                default:
                    break;
            }
        }

        public void CloseFilterDrive()
        {
            StartCoroutine(CloseDrive());
        }

        public void BlackOutMode()
        {
            StartCoroutine(BlackOut());
        }

        public void PowerOnMode()
        {
            StartCoroutine(PowerOn());
        }

        public void PowerOffMode()
        {
            StartCoroutine(PowerOff());
        }

        public void ItemModified()
        {
            DrawPage(_currentPage);
        }
        #endregion

        #region Private Methods
        private bool FindAllComponents()
        {
            Log.Info("Find All Components");

            #region Sea Breeze

            _seaBreeze = LoadItems.FcsARSolutionPrefab;
            if (_seaBreeze == null)
            {
                Log.Error("Screen: SeaBreeze not found.");
                return false;
            }
            #endregion

            #region Canvas

            CanvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (CanvasGameObject == null)
            {
                Log.Error("Canvas not found.");
                return false;
            }

            #endregion


            // == Canvas Elements == //

            #region Model

            _model = _seaBreeze.FindChild("model")?.gameObject;

            if (_model == null)
            {
                Log.Error("Screen: Home Screen not found.");
                return false;
            }
            #endregion

            //#region LongTermFilter

            //LongTermFilter = _model.FindChild("FilterHolderDoor").FindChild("LongTerm_Filter");

            //if (LongTermFilter == null)
            //{
            //    Log.Error("Object: Long Term Filter not found.");
            //    return false;
            //}
            //#endregion

            //#region ShortTermFilter

            //ShortTermFilter = _model.FindChild("FilterHolderDoor").FindChild("ShortTerm_Filter");

            //if (ShortTermFilter == null)
            //{
            //    Log.Error("Object: Short Term Filter not found.");
            //    return false;
            //}
            //#endregion

            #region Home Screen
            _homeScreen = CanvasGameObject.transform.Find("HomeScreen")?.gameObject;
            if (_homeScreen == null)
            {
                Log.Error("Screen: Home Screen not found.");
                return false;
            }
            #endregion

            #region Home Screen Timer
            FilterLBL = _homeScreen.transform.Find("FilterTimer_LBL")?.gameObject;

            if (FilterLBL == null)
            {
                Log.Error("Screen: Filter label not found.");
                return false;
            }
            #endregion

            #region Home Screen Power BTN
            _homeScreenPowerBTN = _homeScreen.transform.Find("Power_BTN")?.gameObject;

            if (_homeScreenPowerBTN == null)
            {
                Log.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }


            var homeScreenPowerBTN = _homeScreenPowerBTN.AddComponent<InterfaceButton>();
            homeScreenPowerBTN.Display = this;
            homeScreenPowerBTN.BtnName = "HPPBtn";
            #endregion

            #region Paginator
            _paginator = _homeScreen.FindChild("Paginator")?.gameObject;
            if (_paginator == null)
            {
                Log.Error("Screen: Paginator not found.");
                return false;
            }
            #endregion

            #region Filter
            _filterBTN = _homeScreen.transform.Find("Filter_BTN")?.gameObject;

            if (_filterBTN == null)
            {
                Log.Error("Screen: Filter Button not found.");
                return false;
            }

            var filterrBTN = _filterBTN.AddComponent<InterfaceButton>();
            filterrBTN.Display = this;
            filterrBTN.BtnName = "FilterBtn";
            filterrBTN.HoverTextLineOne = "Open Filter Drive";
            #endregion

            #region Items Grid
            _itemsGrid = CanvasGameObject.transform.Find("HomeScreen").Find("ItemsGrid")?.gameObject;
            if (_itemsGrid == null)
            {
                Log.Error("Screen: Item Grid not found.");
                return false;
            }
            #endregion

            #region Background
            _background = CanvasGameObject.transform.Find("Background")?.gameObject;
            if (_background == null)
            {
                Log.Error("Screen: Background not found.");
                return false;
            }
            #endregion

            #region BlackOut
            _blackOut = CanvasGameObject.transform.Find("BlackOut")?.gameObject;
            if (_blackOut == null)
            {
                Log.Error("Screen: BlackOut not found.");
                return false;
            }
            #endregion

            #region PowerOff
            _poweredOffScreen = CanvasGameObject.transform.Find("PoweredOffScreen")?.gameObject;
            if (_poweredOffScreen == null)
            {
                Log.Error("Screen: Powered Off Screen not found.");
                return false;
            }
            #endregion

            #region Power BTN
            _poweredScreenPowerBTN = _poweredOffScreen.transform.Find("Power_BTN")?.gameObject;
            if (_poweredScreenPowerBTN == null)
            {
                Log.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }

            var poweredOffScreenBTN = _poweredScreenPowerBTN.AddComponent<InterfaceButton>();
            poweredOffScreenBTN.Display = this;
            poweredOffScreenBTN.BtnName = "PPBtn";
            #endregion

            #region Animator

            Animator = gameObject.GetComponentInChildren<Animator>();

            if (Animator == null)
            {
                Log.Error("Animator not found.");
                return false;
            }

            #endregion

            #region Prev Page BTN
            _previousPageGameObject = _paginator.FindChild("PreviousPage")?.gameObject;
            if (_previousPageGameObject == null)
            {
                Log.Error("Screen: PreviousPage not found.");
                return false;
            }

            var prevPageBTN = _previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.Display = this;
            prevPageBTN.AmountToChangePageBy = -1;
            #endregion

            #region Next Page BTN
            _nextPageGameObject = _paginator.FindChild("NextPage")?.gameObject;
            if (_nextPageGameObject == null)
            {
                Log.Error("Screen: NextPage not found.");
                return false;
            }

            var nextPageBTN = _nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.Display = this;
            nextPageBTN.AmountToChangePageBy = 1;
            #endregion

            #region Color Picker Page Counter
            _pageCounter = _paginator.FindChild("PageNumber")?.gameObject;
            if (_pageCounter == null)
            {
                Log.Error("Screen: PageNumber not found.");
                return false;
            }

            _pageCounterText = _pageCounter.GetComponent<Text>();
            if (_pageCounterText == null)
            {
                Log.Error("Screen: _pageCounterText not found.");
                return false;
            }
            #endregion
            return true;
        }

        internal void ShutDown()
        {
            Log.Info($"Shutting Down");
        }

        public void DrawPage(int page)
        {
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

            //====================================================================//
            //Log.Info($"//=================== StartPosition | {startingPosition} ============================== //");
            //Log.Info($"//=================== StartPosition | {endingPosition} ============================== //");
            //Log.Info($"//=================== SerializedColors Count| {_serializedColors.Count} ============================== //");
            //====================================================================//

            _container = Controller.Container.container;

            if (endingPosition > _container.count)
            {
                endingPosition = _container.count;
            }

            ClearPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var element = _container.ElementAt(i);
                var techType = element.item.GetTechType();

                Log.Info($"Element: {element} || TechType : {techType}");

                if (Controller.StorageItems.ContainsKey(techType))
                {
                    Controller.StorageItems[techType] = Controller.StorageItems[techType] + 1;
                }
                else
                {
                    Controller.StorageItems.Add(techType, 1);
                }
            }

            foreach (var storageItem in Controller.StorageItems)
            {
                LoadFridgeDisplay(storageItem.Key, storageItem.Value);

            }

            Log.Info($"Dictionary Count: {Controller.StorageItems.Count}");
            UpdatePaginator();
        }

        private void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                var item = _itemsGrid.transform.GetChild(i).gameObject;
                //Log.Info($"Removed Item : {item.name} from screen");
                Destroy(item);
            }

            Controller.StorageItems.Clear();
        }

        private void LoadFridgeDisplay(TechType type, int amount)
        {
            Log.Info("Load Fridge Display");

            GameObject itemDisplay = Instantiate(LoadItems.FcsARSolutionItemPrefab);

            itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            itemDisplay.GetComponentInChildren<Text>().text = "x" + amount;

            ItemButton itemButton = itemDisplay.AddComponent<ItemButton>();
            itemButton.Type = type;
            itemButton.Amount = amount;
            itemButton.Display = this;

            uGUI_Icon icon = itemDisplay.transform.Find("ItemImage").gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(type);

        }

        private void UpdatePaginator()
        {
            CalculateNewMaxPages();
            _pageCounter.SetActive(_container.count != 0);
            _pageCounterText.text = $"{_currentPage} / {_maxPage}";
            _previousPageGameObject.SetActive(_currentPage != 1);
            _nextPageGameObject.SetActive(_currentPage != _maxPage);
            Controller.Container.enabled = true;
        }

        private IEnumerator CompleteSetup()
        {
            Log.Info("InComplete Setup");
            Log.Info($"HasBreakerTripped || {Controller.HasBreakerTripped}");
            if (!Controller.HasBreakerTripped || Controller.PowerAvaliable)
            {
                Animator.enabled = true;
                Log.Info($"Animator Enabled : {Animator.enabled}");

                yield return new WaitForEndOfFrame();
                if (Controller.IsBeingDeleted) yield break;

                var bootMode = Animator.GetBool("BootScreen");
                Log.Info($"Starting Boot Screen || {bootMode}");


                Animator.SetBool("BootScreen", !bootMode);

                if (Controller.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(BOOTING_ANIMATION_TIME);
                if (Controller.IsBeingDeleted) yield break;


                Log.Info($"Starting Welcome Screen");

                Animator.SetBool("WelcomeScreen", true);
                if (Controller.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(WELCOME_SCREEN_TIME);
                if (Controller.IsBeingDeleted) yield break;

                Log.Info($"Starting Home Screen");

                Animator.SetBool("HomeScreen", true);

                if (Controller.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(HOME_SCREEN_TIME);
                if (Controller.IsBeingDeleted) yield break;


                Animator.SetBool("BootScreen", false);
                Animator.SetBool("WelcomeScreen", false);
                DrawPage(1);

                if (Controller.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(RESET_TIMER);
                if (Controller.IsBeingDeleted) yield break;

                ResetAnimation();
            }

        }

        private IEnumerator PowerOff()
        {
            Animator.enabled = true;
            Log.Info($"Animator Enabled : {Animator.enabled}");

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Log.Info($"Rebooting");
            Animator.SetBool("Reboot", true);

            Log.Info($"Powering Off");
            Animator.SetBool("PowerOff", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(ON_OFF_TIMER);
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            PoweredOffMode = true;

            ResetAnimation();
        }

        public bool PoweredOffMode { get; set; }

        private IEnumerator BlackOut()
        {
            Animator.enabled = true;
            Log.Info($"Animator Enabled : {Animator.enabled}");

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Log.Info($"Rebooting");

            if (PoweredOffMode)
            {
                Animator.SetBool("PowerOn", true);
            }

            Animator.SetBool("Reboot", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(ON_OFF_TIMER);
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            ResetAnimation();
        }

        private IEnumerator PowerOn()
        {
            Animator.enabled = true;
            Log.Info($"Animator Enabled : {Animator.enabled}");

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Log.Info($"Powering On");
            Animator.SetBool("PowerOn", true);

            Log.Info($"Rebooting");
            Animator.SetBool("Reboot", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(ON_OFF_TIMER);
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            ResetAnimation();

            PoweredOffMode = false;

            StartCoroutine(CompleteSetup());
        }

        private IEnumerator OpenDrive()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Animator.SetBool("DriveState", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(2);
            if (Controller.IsBeingDeleted) yield break;

        }

        private IEnumerator CloseDrive()
        {
            Animator.enabled = true;

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(2);
            if (Controller.IsBeingDeleted) yield break;

            Animator.SetBool("DriveState", false);

        }

        private void ResetAnimation()
        {
            foreach (var parameter in Animator.parameters)
            {
                Animator.SetBool(parameter.name, false);
                Log.Info($"Animator Parameter {parameter.name} was reset to false");
            }
            //Animator.SetBool("BootScreen", false);
        }

        private void CalculateNewMaxPages()
        {
            _maxPage = Mathf.CeilToInt((_container.count - 1) / ITEMS_PER_PAGE) + 1;
            if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }
        #endregion

        #region Event Handlers
        public void OnPointerClick(PointerEventData eventData)
        {
            //throw new NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //throw new NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
