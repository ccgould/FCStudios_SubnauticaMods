using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Interfaces;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class ManualPageController : MonoBehaviour, IPageController
    {
        private List<CraftableItem> _craftableToggles = new List<CraftableItem>();
        private GridHelperV2 _itemGrid;
        private TechType _selectedCraftable;
        private string _currentSearchString;
        private PaginatorController _paginatorController;
        private DSSAutoCrafterDisplay _mono;
        private int _amount = 1;
        private Text _craftingAmount;
        private const float _maxInteraction = 0.9f;

        internal void Initialize(DSSAutoCrafterDisplay mono)
        {
            _mono = mono;
            
            foreach (Transform craftableItem in GameObjectHelpers.FindGameObject(mono.ManualPage, "Grid").transform)
            {
                var craftableToggle = craftableItem.gameObject.EnsureComponent<CraftableItem>();
                craftableToggle.Initialize(mono.GetController());
                craftableToggle.OnButtonClick += OnToggleClick;
                _craftableToggles.Add(craftableToggle);
            }

            _itemGrid = mono.gameObject.AddComponent<GridHelperV2>();
            _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
            _itemGrid.Setup(21, mono.ManualPage, Color.gray, Color.white, null);

            _paginatorController = GameObjectHelpers.FindGameObject(mono.ManualPage, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(mono);

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
            var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
            text.text = AlterraHub.SearchForItemsMessage();

            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            _craftingAmount = InterfaceHelpers.FindGameObject(gameObject, "CraftAmount").GetComponent<Text>();

            var craftBTN = GameObjectHelpers.FindGameObject(mono.ManualPage, "CraftBTN").GetComponent<Button>();
            var craftFBTN = craftBTN.gameObject.AddComponent<FCSButton>();
            craftFBTN.MaxInteractionRange = _maxInteraction;
            craftBTN.onClick.AddListener((() =>
            {
                _mono.GetController().CraftItem(new CraftingOperation(_selectedCraftable, _amount, false));
                mono.GoToPage(AutoCrafterPages.Automatic);
            }));

            var addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN").GetComponent<Button>();
            var addFBTN = addBTN.gameObject.AddComponent<FCSButton>();
            addFBTN.MaxInteractionRange = _maxInteraction;
            addFBTN.ShowMouseClick = true;
            addFBTN.TextLineOne = "Add";
            addFBTN.TextLineTwo = "Adds to the amount to craft. Hold (Shift) to increment by 10.";
            addBTN.onClick.AddListener((() =>
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    _amount+=10;
                }
                else
                {
                    _amount++;
                }

                if (_amount >= 100)
                {
                    _amount = 100;
                }

                _craftingAmount.text = _amount.ToString();
            }));

            var subtractBTN = GameObjectHelpers.FindGameObject(gameObject, "MinusBTN").GetComponent<Button>();
            var subtractFBTN = subtractBTN.gameObject.AddComponent<FCSButton>();
            subtractFBTN.MaxInteractionRange = _maxInteraction;
            subtractFBTN.ShowMouseClick = true;
            subtractFBTN.TextLineOne = "Subtract";
            subtractFBTN.TextLineTwo = "Removes from the amount to craft. Hold (Shift) to decrement by 10.";

            subtractBTN.onClick.AddListener((() =>
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    _amount -= 10;
                }
                else
                {
                    _amount--;
                }

                if (_amount <= 1)
                {
                    _amount = 1;
                }
                _craftingAmount.text = _amount.ToString();
            }));

            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
            var backFBTN = backBTN.gameObject.AddComponent<FCSButton>();
            backFBTN.MaxInteractionRange = _maxInteraction;
            backFBTN.ShowMouseClick = true;
            backFBTN.TextLineOne = "Back";
            backBTN.onClick.AddListener((() =>
            {
                Reset();
                _mono.GoToPage(AutoCrafterPages.Home);
            }));
        }

        private void Reset()
        {
            _amount = 1;
            _craftingAmount.text = _amount.ToString();
            _selectedCraftable = TechType.None;
            foreach (CraftableItem craftableItem in _craftableToggles)
            {
                craftableItem.SetState(false);
            }
        }

        private void OnToggleClick(TechType techType, bool state)
        {

            _selectedCraftable = state ? techType : TechType.None;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                _mono.GetController().GetCraftables();
                var grouped = Mod.Craftables;

                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => TechTypeExtensions.Get(Language.main, p).ToLower().Contains(_currentSearchString.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _craftableToggles[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _craftableToggles[w++].Set(grouped[i], _selectedCraftable == grouped[i]);
                }

                _itemGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_itemGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void UpdateSearch(string newSearch)
        {
            _currentSearchString = newSearch;
            _itemGrid.DrawPage();
        }

        public void Refresh()
        {
            _itemGrid?.DrawPage();
        }

        public void GoToPage(int index)
        {
            _itemGrid.DrawPage(index);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
            _mono.GetController().SetManual();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            Reset();
        }
    }
}