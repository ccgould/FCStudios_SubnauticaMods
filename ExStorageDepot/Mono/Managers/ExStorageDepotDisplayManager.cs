using ExStorageDepot.Buildable;
using ExStorageDepot.Display;
using ExStorageDepot.Enumerators;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotDisplayManager : AIDisplay
    {
        private ExStorageDepotController _mono;
        private GameObject _storageLabels;
        private GameObject _grid;
        private Text _pageCounter;
        private Text _plier;
        private readonly Dictionary<TechType, GameObject> _trackedResourcesDisplayElements = new Dictionary<TechType, GameObject>();
        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

            ITEMS_PER_PAGE = 16;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                DrawPage(1);
            }
            else
            {
                QuickLogger.Error("Failed to find all components for the display.");
            }

            if (mono.NameController == null)
            {
                QuickLogger.Error("Name Controller is null", true);
                return;
            }

            mono.NameController.OnLabelChanged += OnLabelChanged;

            UpdateLabels();
        }

        private void OnLabelChanged(string obj)
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (_storageLabels == null) return;

            foreach (Transform label in _storageLabels.transform)
            {
                QuickLogger.Debug("Changing label.", true);
                var text = label.GetComponent<Text>();
                text.text = _mono.NameController.GetCurrentName();
                QuickLogger.Debug($"Label {label.name} changed.", true);
            }
        }

        public override void ClearPage()
        {
            for (int i = 0; i < _grid.transform.childCount; i++)
            {
                Destroy(_grid.transform.GetChild(i).gameObject);
            }

            _trackedResourcesDisplayElements?.Clear();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "DumpBTN":
                    QuickLogger.Debug($"In {btnName} switch", true);
                    _mono.Storage.OpenStorage();
                    break;

                case "RenameBTN":
                    QuickLogger.Debug($"In {btnName} switch", true);
                    _mono.NameController.Show();
                    break;
                case "LButton":
                    QuickLogger.Debug($"Current Multiplier = {_mono.BulkMultiplier}");

                    switch (_mono.BulkMultiplier)
                    {
                        case BulkMultipliers.TimesTen:
                            _mono.BulkMultiplier = BulkMultipliers.TimesEight;
                            break;
                        case BulkMultipliers.TimesEight:
                            _mono.BulkMultiplier = BulkMultipliers.TimesSix;
                            break;
                        case BulkMultipliers.TimesSix:
                            _mono.BulkMultiplier = BulkMultipliers.TimesFour;
                            break;
                        case BulkMultipliers.TimesFour:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTwo;
                            break;
                        case BulkMultipliers.TimesTwo:
                            _mono.BulkMultiplier = BulkMultipliers.TimesOne;
                            break;
                    }

                    UpdateMultiplier();
                    break;

                case "RButton":
                    QuickLogger.Debug($"Current Multiplier = {_mono.BulkMultiplier}");

                    switch (_mono.BulkMultiplier)
                    {
                        case BulkMultipliers.TimesEight:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTen;
                            break;
                        case BulkMultipliers.TimesSix:
                            _mono.BulkMultiplier = BulkMultipliers.TimesEight;
                            break;
                        case BulkMultipliers.TimesFour:
                            _mono.BulkMultiplier = BulkMultipliers.TimesSix;
                            break;
                        case BulkMultipliers.TimesTwo:
                            _mono.BulkMultiplier = BulkMultipliers.TimesFour;
                            break;
                        case BulkMultipliers.TimesOne:
                            _mono.BulkMultiplier = BulkMultipliers.TimesTwo;
                            break;
                    }
                    UpdateMultiplier();
                    break;
            }
        }

        private void UpdateMultiplier()
        {
            switch (_mono.BulkMultiplier)
            {
                case BulkMultipliers.TimesOne:
                    _plier.text = $"x1";
                    _mono.Storage.SetMultiplier(1);
                    break;
                case BulkMultipliers.TimesTwo:
                    _plier.text = $"x2";
                    _mono.Storage.SetMultiplier(2);
                    break;
                case BulkMultipliers.TimesFour:
                    _plier.text = $"x4";
                    _mono.Storage.SetMultiplier(4);
                    break;
                case BulkMultipliers.TimesSix:
                    _plier.text = $"x6";
                    _mono.Storage.SetMultiplier(6);
                    break;
                case BulkMultipliers.TimesEight:
                    _plier.text = $"x8";
                    _mono.Storage.SetMultiplier(8);
                    break;
                case BulkMultipliers.TimesTen:
                    _plier.text = $"x10";
                    _mono.Storage.SetMultiplier(10);
                    break;
            }
        }

        public override void ItemModified(TechType type, int newAmount = 0)
        {
            if (newAmount > 0 && _trackedResourcesDisplayElements.ContainsKey(type))
            {
                _trackedResourcesDisplayElements[type].GetComponentInChildren<Text>().text = newAmount.ToString();
                return;
            }

            DrawPage(CurrentPage);
        }

        public override bool FindAllComponents()
        {
            var canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (canvas == null)
            {
                QuickLogger.Error("Canvas could not be found!");
                return false;
            }

            var home = canvas.FindChild("Home")?.gameObject;

            if (home == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Home");
                return false;
            }

            _storageLabels = home.FindChild("Storage_Labels")?.gameObject;

            if (_storageLabels == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Storage_Labels");
                return false;
            }

            _grid = home.FindChild("Grid")?.gameObject;

            if (_grid == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Grid");
                return false;
            }

            var dumpBTN = home.FindChild("Dump_Button")?.gameObject;

            if (dumpBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Dump_Button");
                return false;
            }

            var dumpBtn = dumpBTN.AddComponent<InterfaceButton>();
            dumpBtn.BtnName = "DumpBTN";
            dumpBtn.OnButtonClick = OnButtonClick;
            dumpBtn.ButtonMode = InterfaceButtonMode.Background;
            dumpBtn.HOVER_COLOR = Color.gray;


            var renameBTN = home.FindChild("Rename_Button")?.gameObject;

            if (renameBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Rename_Button");
                return false;
            }

            var renameBtn = renameBTN.AddComponent<InterfaceButton>();
            renameBtn.BtnName = "RenameBTN";
            renameBtn.OnButtonClick = OnButtonClick;
            renameBtn.ButtonMode = InterfaceButtonMode.Background;
            renameBtn.HOVER_COLOR = Color.gray;


            var paginator = home.FindChild("Paginator")?.gameObject;

            if (paginator == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Paginator");
                return false;
            }

            var pprevBTN = paginator.FindChild("Prev_BTN")?.gameObject;

            if (pprevBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Prev_BTN");
                return false;
            }

            var ppBtn = pprevBTN.AddComponent<PaginatorButton>();
            ppBtn.AmountToChangePageBy = -1;
            ppBtn.HoverTextLineTwo = ExStorageDepotBuildable.PrevPage();
            ppBtn.OnChangePageBy = ChangePageBy;

            var pnextBTN = paginator.FindChild("Next_BTN")?.gameObject;

            if (pnextBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Next_BTN");
                return false;
            }

            var pnBtn = pnextBTN.AddComponent<PaginatorButton>();
            pnBtn.AmountToChangePageBy = 1;
            pnBtn.HoverTextLineTwo = ExStorageDepotBuildable.NextPage();
            pnBtn.OnChangePageBy = ChangePageBy;


            _pageCounter = paginator.FindChild("PageCounter").GetComponent<Text>();

            if (_pageCounter == null)
            {
                QuickLogger.Error("Couldn't find Text in the PageCounter");
                return false;
            }


            var multiplier = home.FindChild("Multiplier")?.gameObject;

            if (multiplier == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Multiplier");
                return false;
            }

            var mprevBTN = multiplier.FindChild("Prev_BTN")?.gameObject;

            if (mprevBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Prev_BTN in Multiplier");
                return false;
            }

            var mpBtn = mprevBTN.AddComponent<InterfaceButton>();
            mpBtn.BtnName = "LButton";
            mpBtn.OnButtonClick = OnButtonClick;
            mpBtn.ButtonMode = InterfaceButtonMode.Background;

            var mnextBTN = multiplier.FindChild("Next_BTN")?.gameObject;

            if (mnextBTN == null)
            {
                QuickLogger.Error("Couldn't find the gameobject Next_BTN in Multiplier");
                return false;
            }

            var mnBtn = mnextBTN.AddComponent<InterfaceButton>();
            mnBtn.BtnName = "RButton";
            mnBtn.OnButtonClick = OnButtonClick;
            mnBtn.ButtonMode = InterfaceButtonMode.Background;


            _plier = multiplier.FindChild("plier").GetComponent<Text>();

            if (_pageCounter == null)
            {
                QuickLogger.Error("Couldn't find the Text on PageCounter in Multiplier");
                return false;
            }





            return true;
        }

        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.ToggleScreenState();

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

            //QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);
            //QuickLogger.Debug($"Number Of Items: {_mono.Storage.GetItemsCount()}");
            var container = _mono.Storage.TrackedItems;


            if (endingPosition > container.Count)
            {
                endingPosition = container.Count;
            }

            //QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);

            ClearPage();

            //QuickLogger.Debug($"startingPosition: {startingPosition} || endingPosition : {endingPosition}", true);

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var element = container.ElementAt(i);
                var techType = element.Key;

                QuickLogger.Debug($"Element: {element} || TechType : {techType}");

                LoadDisplay(element.Key, element.Value);

                //Tracked items was here
            }

            UpdatePaginator();
        }

        private void LoadDisplay(TechType elementTechType, int techTypeCount)
        {
            QuickLogger.Debug("Load Fridge Display");

            GameObject itemDisplay = Instantiate(ExStorageDepotBuildable.ItemPrefab);

            itemDisplay.transform.SetParent(_grid.transform, false);
            itemDisplay.GetComponentInChildren<Text>().text = techTypeCount.ToString();

            ItemButton itemButton = itemDisplay.AddComponent<ItemButton>();
            itemButton.Type = elementTechType;
            itemButton.Amount = techTypeCount;
            itemButton.OnButtonClick = _mono.Storage.AttemptToTakeItem;

            uGUI_Icon icon = itemDisplay.transform.Find("Image").gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(elementTechType);

            _trackedResourcesDisplayElements.Add(elementTechType, itemDisplay);
        }

        public override void UpdatePaginator()
        {
            base.UpdatePaginator();

            CalculateNewMaxPages();
            //_pageCounter.SetActive(_mono.NumberOfItems != 0);
            _pageCounter.text = $"{CurrentPage} / {MaxPage}";
            //_previousPageGameObject.SetActive(true); //CurrentPage != 1
            //_nextPageGameObject.SetActive(true); //CurrentPage != MaxPage
        }

        private void CalculateNewMaxPages()
        {
            QuickLogger.Debug($"Seabreeze TrackedItems {_mono.Storage.TrackedItems.Count}, Items Per Page {ITEMS_PER_PAGE}, Max Page {MaxPage}", true);

            MaxPage = Mathf.CeilToInt((_mono.Storage.TrackedItems.Count - 1) / ITEMS_PER_PAGE) + 1;


            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
            }
        }
    }
}
