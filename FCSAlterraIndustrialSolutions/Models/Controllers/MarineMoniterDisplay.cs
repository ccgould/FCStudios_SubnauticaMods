using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Abstract;
using FCSAlterraIndustrialSolutions.Models.Buttons;
using FCSCommon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public class MarineMoniterDisplay : AIDisplay
    {
        #region Public Properties
        /// <summary>
        /// The controller of this display
        /// </summary>
        public MarineMonitorController Controller { get; set; }
        #endregion

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
        private Dictionary<string, JetStreamT242Controller> _container;
        private GameObject _paginator;
        private GameObject _previousPageGameObject;
        private GameObject _nextPageGameObject;
        private GameObject _itemsGrid;
        private GameObject _pageCounterBottom;
        private GameObject _pageCounterTop;

        #endregion

        #region Private Methods

        private void UpdateLanguage()
        {
            _menuFrame.FindChild("Legend").GetComponent<Text>().text = $"{LoadItems.MarineMonitorModStrings.Legend}:";

            _menuFrame.FindChild("StatusOverviewLBL").GetComponent<Text>().text = LoadItems.MarineMonitorModStrings.StatusOverview;

            _menuFrame.FindChild("HealthyContainer").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.Working;

            _menuFrame.FindChild("DamagedContainer").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.Damaged;

            _menuFrame.FindChild("PoweredOffContainer").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.PoweredOff;

            _menuFrame.FindChild("Healthy_Legend").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.HealthyLegend;

            _menuFrame.FindChild("MidlyDamaged_Legend").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.MidlyDamagedLegend;

            _menuFrame.FindChild("Damaged_Legend").FindChild("Status").GetComponent<Text>().text =
                LoadItems.MarineMonitorModStrings.DamagedLegend;
        }

        private void UpdateMenu()
        {
            var working =
                Controller.Turbines.Where(x => x.Value.LiveMixin.health <= 100 && x.Value.LiveMixin.health > 20).ToList();

            var damaged =
                Controller.Turbines.Where(x => x.Value.LiveMixin.health <= 20).ToList();

            var poweredOff =
                Controller.Turbines.Where(x => x.Value.HasBreakerTripped).ToList();

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

        private void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                var item = _itemsGrid.transform.GetChild(i).gameObject;
                //Log.Info($"Removed Item : {item.name} from screen");
                Destroy(item);
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
                Log.Info($"Animator Parameter {parameter.name} was reset to false");
            }
        }
        
        private void LoadDisplay(JetStreamT242Controller turbine)
        {
            Log.Info("Load Monitor Display");

            GameObject itemDisplay = Instantiate(LoadItems.TurbineItemPrefab);
            Log.Info("TurbineItemPrefab instantiated");

            itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            Log.Info("itemDisplay parent set");

            TurbineItem turbineItem = itemDisplay.GetOrAddComponent<TurbineItem>();
            turbineItem.Turbine = turbine;
            turbineItem.Setup(this);

            Log.Info($"Display: {turbineItem.Display}");

            Log.Info("Added Turbine Item Component");
        }

        #endregion

        #region Public Methods

        public override bool FindAllComponents()
        {
            Log.Info("Find All Components");

            #region Animator

            Animator = gameObject.GetComponentInChildren<Animator>();

            if (Animator == null)
            {
                Log.Error("Animator not found.");
                return false;
            }

            #endregion

            #region Canvas

            _canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (_canvasGameObject == null)
            {
                Log.Error("Canvas not found.");
                return false;
            }

            #endregion

            #region Home

            _home = _canvasGameObject.FindChild("Home")?.gameObject;

            if (_home == null)
            {
                Log.Error("Home not found.");
                return false;
            }

            #endregion

            #region InnerFrame
            _innerFrame = _home.FindChild("InnerFrame")?.gameObject;

            if (_innerFrame == null)
            {
                Log.Error("InnerFrame not found.");
                return false;
            }
            #endregion

            #region BackFrame
            _backFrame = _home.FindChild("BackFrame")?.gameObject;

            if (_backFrame == null)
            {
                Log.Error("BackFrame not found.");
                return false;
            }
            #endregion

            #region MenuFrame
            _menuFrame = _backFrame.FindChild("MenuFrame")?.gameObject;

            if (_menuFrame == null)
            {
                Log.Error("MenuFrame not found.");
                return false;
            }
            #endregion

            #region DamagedContainer
            _damagedContainer = _menuFrame.FindChild("DamagedContainer")?.gameObject;

            if (_damagedContainer == null)
            {
                Log.Error("DamagedContainer not found.");
                return false;
            }
            #endregion

            #region PoweredOffContainer
            _poweredOffContainer = _menuFrame.FindChild("PoweredOffContainer")?.gameObject;

            if (_poweredOffContainer == null)
            {
                Log.Error("PoweredOffContainer not found.");
                return false;
            }
            #endregion

            #region PoweredOffContainer
            _healthyContainer = _menuFrame.FindChild("HealthyContainer")?.gameObject;

            if (_healthyContainer == null)
            {
                Log.Error("HealthyContainer not found.");
                return false;
            }
            #endregion

            #region Paginator
            _paginator = _home.FindChild("Paginator")?.gameObject;
            if (_paginator == null)
            {
                Log.Error("Screen: Paginator not found.");
                return false;
            }
            #endregion

            #region Prev Page BTN
            _previousPageGameObject = _paginator.FindChild("PrevBTN")?.gameObject;
            if (_previousPageGameObject == null)
            {
                Log.Error("Screen: PrevBTN not found.");
                return false;
            }

            var prevPageBTN = _previousPageGameObject.AddComponent<PaginatorButton>();
            prevPageBTN.Display = this;
            prevPageBTN.AmountToChangePageBy = -1;
            #endregion

            #region Next Page BTN
            _nextPageGameObject = _paginator.FindChild("NextBTN")?.gameObject;
            if (_nextPageGameObject == null)
            {
                Log.Error("Screen: NextBTN not found.");
                return false;
            }

            var nextPageBTN = _nextPageGameObject.AddComponent<PaginatorButton>();
            nextPageBTN.Display = this;
            nextPageBTN.AmountToChangePageBy = 1;
            #endregion

            #region Items Grid
            _itemsGrid = _innerFrame.transform.Find("Grid")?.gameObject;
            if (_itemsGrid == null)
            {
                Log.Error("Screen: Item Grid not found.");
                return false;
            }
            #endregion

            #region TopNumber
            _pageCounterTop = _paginator.transform.Find("TopNumber")?.gameObject;
            if (_pageCounterTop == null)
            {
                Log.Error("Screen: TopNumber not found.");
                return false;
            }
            #endregion

            #region Items Grid
            _pageCounterBottom = _paginator.transform.Find("BottomNumber")?.gameObject;
            if (_pageCounterBottom == null)
            {
                Log.Error("Screen: BottomNumber not found.");
                return false;
            }
            #endregion


            return true;
        }

        public void Setup(MarineMonitorController marineMoniterController)
        {
            if (marineMoniterController.IsBeingDeleted) return;
            Controller = marineMoniterController;

            if (FindAllComponents() == false)
            {
                Log.Error("// ============== Error getting all Components ============== //");
                ShutDownDisplay();
                return;
            }

            UpdateLanguage();

            StartCoroutine(CompleteSetup());
        }

        public void DrawPage(int page)
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
            _container = Controller.Turbines;

            //====================================================================//
            //Log.Info($"//=================== StartPosition | {startingPosition} ============================== //");
            //Log.Info($"//=================== EndPosition | {endingPosition} ============================== //");
            //Log.Info($"//=================== Turbines Count| {_container.Count} ============================== //");
            //====================================================================//


            Log.Info("Get Controller Turbines");

            if (endingPosition > _container.Count)
            {
                endingPosition = _container.Count;
            }

            Log.Info("Get End Position");


            ClearPage();

            Log.Info("Clear Page");

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var turbine = _container.ElementAt(i);


                Log.Info($"Turbine: {turbine.Value.name}");

                LoadDisplay(turbine.Value);
            }


            Log.Info($"List Count: {Controller.Turbines.Count}");
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
                Log.Error("The tag has returned null and is needed for this button to work");
                return;
            }

            var turbineItem = (TurbineItem)tag;

            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PowerBTN":
                    if (!turbineItem.Turbine.HasBreakerTripped)
                    {
                        turbineItem.Turbine.TriggerPowerOff();
                    }
                    else
                    {
                        turbineItem.Turbine.TriggerPowerOn();
                    }

                    break;

                case "PingBTN":
                    if (turbineItem.Turbine.IsBeingPinged)
                    {
                        turbineItem.Turbine.PingObject(false);
                    }
                    else
                    {
                        turbineItem.Turbine.PingObject(true);
                    }
                    break;

            }
        }

        public override void ItemModified<T>(T item)
        {
            DrawPage(CurrentPage);
        }


        #endregion

        #region IEnumerators

        public override IEnumerator CompleteSetup()
        {
            Log.Info("InComplete Setup");

            Animator.enabled = true;
            Log.Info($"Animator Enabled : {Animator.enabled}");

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Log.Info($"Starting Home Screen");

            Animator.SetBool("Boot", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            ResetAnimation();

            DrawPage(1);
        }

        public override IEnumerator ShutDown()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;


            Log.Info($"Shutting Down");
            Animator.SetBool("Reboot", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

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

        #endregion
    }
}
