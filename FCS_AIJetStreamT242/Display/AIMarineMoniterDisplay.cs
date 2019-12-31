using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Display.Patching;
using FCS_AIMarineTurbine.Mono;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Abstract;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AIMarineTurbine.Display
{
    public class AIMarineMoniterDisplay : AIDisplay
    {
        #region Private Members


        private readonly float RESET_TIMER = 1;
        private Animator Animator;
        private GameObject _canvasGameObject;
        private GameObject _home;
        private GameObject _innerFrame;
        private GameObject _backFrame;
        private GameObject _menuFrame;
        private GameObject _damagedContainer;
        private GameObject _poweredOffContainer;
        private GameObject _healthyContainer;
        private Dictionary<string, AIJetStreamT242Controller> _container;
        private GameObject _paginator;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _itemsGrid;
        private GameObject _pageCounterBottom;
        private GameObject _pageCounterTop;
        private AIMarineMonitorController _mono;
        private bool _componentsFound;

        #endregion

        #region Private Methods

        private void UpdateLanguage()
        {
            _menuFrame.FindChild("Legend").GetComponent<Text>().text = $"{LanguageHelpers.GetLanguage(DisplayLanguagePatching.LegendKey)}:";

            _menuFrame.FindChild("StatusOverviewLBL").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.StatusOverviewKey);

            _menuFrame.FindChild("HealthyContainer").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.WorkingKey);

            _menuFrame.FindChild("DamagedContainer").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.FailedKey);

            _menuFrame.FindChild("PoweredOffContainer").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.PoweredOffKey);

            _menuFrame.FindChild("Healthy_Legend").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.HealthyLegendKey);

            _menuFrame.FindChild("MidlyDamaged_Legend").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.MidlyDamagedLegendKey);

            _menuFrame.FindChild("Damaged_Legend").FindChild("Status").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.FailedLegendKey);
        }

        private void UpdateMenu()
        {
            if (!_componentsFound) return;

            var working =
                _mono.Turbines.Where(x => x.Value.IsInitialized && x.Value.HealthManager.GetHealth() <= 100 && x.Value.HealthManager.GetHealth() > 0).ToList();

            var damaged =
                _mono.Turbines.Where(x => x.Value.IsInitialized && x.Value.HealthManager.GetHealth() <= 0).ToList();

            var poweredOff =
                _mono.Turbines.Where(x => x.Value.IsInitialized &&  x.Value.PowerManager.GetHasBreakerTripped()).ToList();

            _healthyContainer.FindChild("Value").GetComponent<Text>().text = working.Count.ToString();
            _damagedContainer.FindChild("Value").GetComponent<Text>().text = damaged.Count.ToString();
            _poweredOffContainer.FindChild("Value").GetComponent<Text>().text = poweredOff.Count.ToString();
        }

        private void CalculateNewMaxPages()
        {
            MaxPage = Mathf.CeilToInt((_container.Count - 1) / ITEMS_PER_PAGE) + 1;
            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }
        }

        private void UpdatePaginator()
        {
            CalculateNewMaxPages();
            _pageCounterTop.SetActive(_container.Count != 0);
            _pageCounterBottom.SetActive(_container.Count != 0);
            _pageCounterTop.GetComponent<Text>().text = $"{CurrentPage}";
            _pageCounterBottom.GetComponent<Text>().text = $"{MaxPage}";
            _previousPageGameObject.SetActive(CurrentPage != 1);
            _nextPageGameObject.SetActive(CurrentPage != MaxPage);
        }

        private void ResetAnimation()
        {
            foreach (var parameter in Animator.parameters)
            {
                Animator.SetBool(parameter.name, false);
                QuickLogger.Debug($"Animator Parameter {parameter.name} was reset to false");
            }
        }

        private void LoadDisplay(AIJetStreamT242Controller turbine)
        {
            QuickLogger.Debug("Load Monitor Display");

            GameObject itemDisplay = Instantiate(AIMarineMonitorBuildable.TurbineItemPrefab);
            QuickLogger.Debug("TurbineItemPrefab instantiated");

            itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            QuickLogger.Debug("itemDisplay parent set");

            TurbineItem turbineItem = itemDisplay.EnsureComponent<TurbineItem>();
            turbineItem.Turbine = turbine;
            turbineItem.Setup(this);

            QuickLogger.Debug($"Display: {turbineItem.Display}");

            QuickLogger.Debug("Added Turbine Item Component");
        }

        #endregion

        #region Public Methods

        public override bool FindAllComponents()
        {
            QuickLogger.Debug("Find All Components");

            #region Animator

            Animator = gameObject.GetComponentInChildren<Animator>();

            if (Animator == null)
            {
                QuickLogger.Error("Animator not found.");
                return false;
            }

            #endregion

            #region Canvas

            _canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (_canvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }

            #endregion

            #region Home

            _home = _canvasGameObject.FindChild("Home")?.gameObject;

            if (_home == null)
            {
                QuickLogger.Error("Home not found.");
                return false;
            }

            #endregion

            #region InnerFrame
            _innerFrame = _home.FindChild("InnerFrame")?.gameObject;

            if (_innerFrame == null)
            {
                QuickLogger.Error("InnerFrame not found.");
                return false;
            }
            #endregion

            #region BackFrame
            _backFrame = _home.FindChild("BackFrame")?.gameObject;

            if (_backFrame == null)
            {
                QuickLogger.Error("BackFrame not found.");
                return false;
            }
            #endregion

            #region MenuFrame
            _menuFrame = _backFrame.FindChild("MenuFrame")?.gameObject;

            if (_menuFrame == null)
            {
                QuickLogger.Error("MenuFrame not found.");
                return false;
            }
            #endregion

            #region DamagedContainer
            _damagedContainer = _menuFrame.FindChild("DamagedContainer")?.gameObject;

            if (_damagedContainer == null)
            {
                QuickLogger.Error("DamagedContainer not found.");
                return false;
            }
            #endregion

            #region PoweredOffContainer
            _poweredOffContainer = _menuFrame.FindChild("PoweredOffContainer")?.gameObject;

            if (_poweredOffContainer == null)
            {
                QuickLogger.Error("PoweredOffContainer not found.");
                return false;
            }
            #endregion

            #region PoweredOffContainer
            _healthyContainer = _menuFrame.FindChild("HealthyContainer")?.gameObject;

            if (_healthyContainer == null)
            {
                QuickLogger.Error("HealthyContainer not found.");
                return false;
            }
            #endregion

            #region Paginator
            _paginator = _home.FindChild("Paginator")?.gameObject;
            if (_paginator == null)
            {
                QuickLogger.Error("Screen: Paginator not found.");
                return false;
            }
            #endregion

            #region Prev Page BTN
            _previousPageGameObject = _paginator.FindChild("PrevBTN")?.gameObject;
            if (_previousPageGameObject == null)
            {
                QuickLogger.Error("Screen: PrevBTN not found.");
                return false;
            }

            var prevPageBTN = _previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.OnChangePageBy = ChangePageBy;
            prevPageBTN.AmountToChangePageBy = -1;
            prevPageBTN.STARTING_COLOR = new Color(188, 188, 188);
            prevPageBTN.HOVER_COLOR = Color.white;
            #endregion

            #region Next Page BTN
            _nextPageGameObject = _paginator.FindChild("NextBTN")?.gameObject;
            if (_nextPageGameObject == null)
            {
                QuickLogger.Error("Screen: NextBTN not found.");
                return false;
            }

            var nextPageBTN = _nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.OnChangePageBy = ChangePageBy;
            nextPageBTN.AmountToChangePageBy = 1;
            nextPageBTN.STARTING_COLOR = new Color(188, 188, 188);
            nextPageBTN.HOVER_COLOR = Color.white;
            #endregion

            #region Items Grid
            _itemsGrid = _innerFrame.transform.Find("Grid")?.gameObject;
            if (_itemsGrid == null)
            {
                QuickLogger.Error("Screen: Item Grid not found.");
                return false;
            }
            #endregion

            #region TopNumber
            _pageCounterTop = _paginator.transform.Find("TopNumber")?.gameObject;
            if (_pageCounterTop == null)
            {
                QuickLogger.Error("Screen: TopNumber not found.");
                return false;
            }
            #endregion

            #region Items Grid
            _pageCounterBottom = _paginator.transform.Find("BottomNumber")?.gameObject;
            if (_pageCounterBottom == null)
            {
                QuickLogger.Error("Screen: BottomNumber not found.");
                return false;
            }
            #endregion

            _componentsFound = true;
            return true;
        }

        internal void Setup(AIMarineMonitorController mono)
        {
            if (mono.IsBeingDeleted) return;
            _mono = mono;

            if (FindAllComponents() == false)
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                ShutDownDisplay();
                return;
            }

            UpdateLanguage();

            StartCoroutine(CompleteSetup());
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

            QuickLogger.Debug("About to get Controller Turbines");

            _container = _mono.Turbines;

            QuickLogger.Debug("Get Controller Turbines");

            if (endingPosition > _container.Count)
            {
                endingPosition = _container.Count;
            }

            QuickLogger.Debug("Get End Position");


            ClearPage();

            QuickLogger.Debug("Clear Page");

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var turbine = _container.ElementAt(i);


                QuickLogger.Debug($"Turbine: {turbine.Value.name}");

                LoadDisplay(turbine.Value);
            }


            QuickLogger.Debug($"List Count: {_mono.Turbines.Count}");
            UpdatePaginator();
        }

        public override void ChangePageBy(int amount)
        {
            DrawPage(CurrentPage + amount);
        }

        public override void OnButtonClick(string btnName, object tag)
        {

            if (tag == null)
            {
                QuickLogger.Error("The tag has returned null and is needed for this button to work");
                return;
            }

            var turbineItem = (TurbineItem)tag;

            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PowerBTN":
                    turbineItem.Turbine.PowerManager.TogglePower();
                    break;

                case "PingBTN":
                    if (turbineItem.Turbine.BeaconManager.IsBeingPinged)
                    {
                        turbineItem.Turbine.BeaconManager.HideBeacon();
                    }
                    else
                    {
                        turbineItem.Turbine.BeaconManager.ShowBeacon();
                    }
                    break;

            }
        }

        public override void ItemModified(TechType techType, int newAmount = 0)
        {
            QuickLogger.Debug("In ItemModified");
            DrawPage(1);
        }


        #endregion

        #region IEnumerators

        public override IEnumerator CompleteSetup()
        {
            QuickLogger.Debug("InComplete Setup");

            Animator.enabled = true;
            QuickLogger.Debug($"Animator Enabled : {Animator.enabled}");

            yield return new WaitForEndOfFrame();
            if (_mono.IsBeingDeleted) yield break;

            QuickLogger.Debug($"Starting Home Screen");

            Animator.SetBool("Boot", true);

            if (_mono.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (_mono.IsBeingDeleted) yield break;

            ResetAnimation();

            DrawPage(1);
        }

        public override IEnumerator ShutDown()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (_mono.IsBeingDeleted) yield break;


            QuickLogger.Debug($"Shutting Down");
            Animator.SetBool("Reboot", true);

            if (_mono.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (_mono.IsBeingDeleted) yield break;

            ResetAnimation();
        }

        public override IEnumerator PowerOff()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ITEMS_PER_PAGE = 6;
            InvokeRepeating("UpdateMenu", 0, 1);
        }

        private void Update()
        {

        }

        public override void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                var item = _itemsGrid.transform.GetChild(i).gameObject;
                //QuickLogger.Debug($"Removed Item : {item.name} from screen");
                Destroy(item);
            }
        }
        #endregion
    }
}
