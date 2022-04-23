using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUIFilterPage : DeepDrillerGUIPage
    {
        private FCSButton _toggleFilterBTN;
        private FCSButton _toggleBlackListBTN;
        private GridHelper _filterGrid;
        private readonly HashSet<uGUI_FCSDisplayItem> _trackedFilterState = new();
        private Text _filterPageInformation;

        public override void Awake()
        {
            var backBtn = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            backBtn?.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Settings);
            }));

            _toggleFilterBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleFilterBTN").AddComponent<FCSButton>();
            _toggleFilterBTN.TextLineOne = "Go to moonpool configuration.";
            _toggleFilterBTN.Subscribe((amount) =>
            {
                Hud.GetSender().OreGenerator.ToggleFocus();
            });

            var prevBTN = GameObjectHelpers.FindGameObject(gameObject, "PrevBTN").GetComponent<Button>();
            prevBTN.onClick.AddListener((() =>
            {
                _filterGrid.DrawPage(_filterGrid.GetCurrentPage() + -1);
            }));

            var nextBTN = GameObjectHelpers.FindGameObject(gameObject, "NextBTN").GetComponent<Button>();
            nextBTN.onClick.AddListener((() =>
            {
                _filterGrid.DrawPage(_filterGrid.GetCurrentPage() + 1);
            }));

            _toggleBlackListBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleBlackListBTN").AddComponent<FCSButton>();
            _toggleBlackListBTN.TextLineOne = "Go to moonpool configuration.";
            _toggleBlackListBTN.Subscribe((value) =>
            {
                Hud.GetSender().OreGenerator.SetBlackListMode(value);
            });

            #region Filter Grid

            _filterGrid = gameObject.AddComponent<GridHelper>();
            _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
            _filterGrid.Setup(6, ModelPrefab.DeepDrillerOreBTNPrefab, gameObject, Color.gray, Color.white, OnButtonClick, 5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);
            
            #endregion

            _filterPageInformation = GameObjectHelpers.FindGameObject(gameObject, "FilterPageInformation")?.GetComponent<Text>();

            InvokeRepeating(nameof(UpdateInformation), 1,0.3f);

        }

        private void UpdateInformation()
        {
            if (_filterPageInformation == null) return;
            _filterPageInformation.text = AuxPatchers.FilterPageInformation(_toggleFilterBTN.IsSelected(), _toggleBlackListBTN.IsSelected(), Hud.GetSender().OreGenerator.FocusCount());
        }

        private void OnLoadFilterGrid(DisplayData data)
        {
            try
            {
                if (IsBeingDestroyed) return;

                if (_trackedFilterState.Count < 6)
                {
                    CreateOreButtons();
                }

                QuickLogger.Debug($"OnLoadFilterGrid : {data.ItemsGrid}");

                //if (_trackedFilterState.Count <= 0)
                //{
                //    //Create all filters
                //    var grouped = Hud.GetSender().OreGenerator.AllowedOres;
                    
                //}

                var allowedOres = Hud.GetSender().OreGenerator.AllowedOres;

                if (data.EndPosition > allowedOres.Count)
                {
                    data.EndPosition = allowedOres.Count;
                }

                foreach (uGUI_FCSDisplayItem toggle in _trackedFilterState)
                {
                    toggle.Hide();
                    toggle.Clear();
                    toggle.UnSubscribe(delegate (bool b) { OnFilterButtonClick(b,null); });
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    QuickLogger.Debug($"Index: {w} | Tracked Filter State {_trackedFilterState.Count}",true);
                    var button = _trackedFilterState.ElementAt(w);
                    QuickLogger.Debug($"Button: {button?.GetTechType()}", true);
                    button.Set(allowedOres[i]);
                    button.Subscribe(delegate (bool b) { OnFilterButtonClick(b,button); });
                    button.Show();
                    w++;

                    if (Hud.GetSender().OreGenerator.GetFocusedOres().Contains(button.GetTechType()))
                    {
                        button.Select();
                    }
                }

                _filterGrid.UpdaterPaginator(allowedOres.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnFilterButtonClick(bool value, uGUI_FCSDisplayItem button)
        {
            if (value)
            {
                Hud.GetSender().OreGenerator.AddFocus(button.GetTechType());
            }
            else
            {
                Hud.GetSender().OreGenerator.RemoveFocus(button.GetTechType());
            }
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            QuickLogger.Debug($"Arg:{arg1} || Object: {arg2}",true);
        }

        public override void Show()
        {
            base.Show();

            if (Hud.GetSender().OreGenerator.GetInBlackListMode())
            {
                _toggleBlackListBTN.Check();
            }
            else
            {
                _toggleBlackListBTN.UnCheck();
            }

            if (Hud.GetSender().OreGenerator.GetIsFocused())
            {
                _toggleFilterBTN.Check();
            }
            else
            {
                _toggleFilterBTN.UnCheck();
            }
            
            foreach (var displayItem in _trackedFilterState)
            {
                if (Hud.GetSender().OreGenerator.GetFocusedOres().Contains(displayItem.GetTechType()))
                {
                    displayItem.Select();
                }
                else
                {
                    displayItem.Deselect();
                }
            }

            UpdateInformation();
        }

        private void CreateOreButtons()
        {
            if (_trackedFilterState.Count >= 6) return;
            for (int i = 0; i < 6; i++)
            {
                GameObject buttonPrefab = Instantiate(ModelPrefab.DeepDrillerOreBTNPrefab);
                buttonPrefab.transform.SetParent(_filterGrid.GetGrid().transform, false);
                var itemBTN = buttonPrefab.AddComponent<uGUI_FCSDisplayItem>();
                itemBTN.Initialize(TechType.None, true);
                _trackedFilterState.Add(itemBTN);
                itemBTN.Hide();
            }
        }

        public override void Hide()
        {
            _filterGrid?.DrawPage(0);
            base.Hide();

        }
    }
}