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
    internal class StandByPageController : MonoBehaviour, IPageController
    {
        private List<AutoCrafterItem> _autocrafterToggles = new List<AutoCrafterItem>();
        private GridHelperV2 _itemGrid;
        private string _currentSearchString;
        private PaginatorController _paginatorController;
        private DSSAutoCrafterDisplay _mono;
        private List<string> _selectedCrafters = new List<string>();
        private Toggle _toggle;
        private Toggle _craftingToggle;
        private Toggle _loadShareToggle;
        private const float _maxInteraction = 1f;

        internal void Initialize(DSSAutoCrafterDisplay mono)
        {
            _mono = mono;

            foreach (Transform craftableItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var autoCrafterItem = craftableItem.gameObject.EnsureComponent<AutoCrafterItem>();
                autoCrafterItem.Initialize(this);
                autoCrafterItem.OnButtonClick += OnToggleClick;
                _autocrafterToggles.Add(autoCrafterItem);
            }

            _itemGrid = mono.gameObject.AddComponent<GridHelperV2>();
            _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
            _itemGrid.Setup(14, mono.ManualPage, Color.gray, Color.white, null);

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(mono);

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
            var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
            text.text = AlterraHub.SearchForItemsMessage();

            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            _toggle = GameObjectHelpers.FindGameObject(gameObject, "Toggle").GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener((state =>
            {
                if (_mono.GetController().CraftManager.IsRunning())
                {
                    _toggle.SetIsOnWithoutNotify(false);
                    _mono.GetController().ShowMessage("Autocrafter is currently crafting. Please wait until complete before trying to enable standby");
                    return;
                }
            }));

            _craftingToggle = GameObjectHelpers.FindGameObject(gameObject, "CraftingToggle").GetComponent<Toggle>();
            _loadShareToggle = GameObjectHelpers.FindGameObject(gameObject, "LoadShareToggle").GetComponent<Toggle>();

            var confirmBTN = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN").GetComponent<Button>();
            var confirmFBTN = confirmBTN.gameObject.AddComponent<FCSButton>();
            confirmFBTN.MaxInteractionRange = _maxInteraction;
            confirmBTN.onClick.AddListener((() =>
            {
                foreach (var selectedCrafter in _selectedCrafters)
                {
                    var crafter = (DSSAutoCrafterController)_mono.GetController().Manager.FindDeviceById(selectedCrafter);
                    crafter.AddConnectedCrafter(_mono.GetController().UnitID);
                }

                _mono.GetController().SetStandByMode(_craftingToggle.isOn ? StandByModes.Crafting : StandByModes.Load);

                _mono.GoToPage(AutoCrafterPages.Automatic);
                
                if (_toggle.isOn)
                {
                    _mono.GetController().SetStandBy();
                }
                else
                {
                    _mono.GetController().SetAutomatic();
                }

                Reset();
            }));

            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
            var backFBTN = backBTN.gameObject.AddComponent<FCSButton>();
            backFBTN.MaxInteractionRange = _maxInteraction;
            backFBTN.ShowMouseClick = true;
            backFBTN.TextLineOne = "Back";
            backBTN.onClick.AddListener((() =>
            {
                Reset();
                _mono.GoToPage(AutoCrafterPages.Automatic);
            }));
        }
        
        private void Reset()
        {
            _selectedCrafters.Clear();
        }

        private void OnToggleClick(DSSAutoCrafterController crafter, bool state)
        {
            if(crafter == null)return;

            if (state)
            {
                _selectedCrafters.Add(crafter.UnitID);
            }
            else
            {
                crafter.RemoveAutoCrafter(_mono.GetController().UnitID);
                _selectedCrafters.Remove(crafter.UnitID);
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null || _mono.GetController()?.Manager  == null || !_mono.GetController().IsConstructed || _autocrafterToggles == null) return;
                    var grouped = _mono?.GetController()?.Manager?.GetDevices(Mod.DSSAutoCrafterTabID)?.ToList();

                if (grouped == null) return;

                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p.UnitID).ToLower().Contains(_currentSearchString.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _autocrafterToggles[i]?.Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    var crafterController = (DSSAutoCrafterController)grouped[i];
                    if(crafterController.UnitID == _mono.GetController().UnitID) continue;
                    _autocrafterToggles[w++].Set(crafterController, crafterController.CheckIfConnected(_mono.GetController().UnitID));
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
            Refresh();
            gameObject.SetActive(true);
            _toggle.SetIsOnWithoutNotify(_mono.GetController().CurrentCrafterMode == AutoCrafterMode.StandBy);
            _craftingToggle.SetIsOnWithoutNotify(_mono.GetController().GetStandByMode() == StandByModes.Crafting);
            _loadShareToggle.SetIsOnWithoutNotify(_mono.GetController().GetStandByMode() == StandByModes.Load);
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            Reset();
        }

        public void SetStandByState(bool state, bool notify)
        {
            if (notify)
            {
                _toggle.isOn = state;
            }
            else
            {
                _toggle.SetIsOnWithoutNotify(state);
            }
        }
    }
}