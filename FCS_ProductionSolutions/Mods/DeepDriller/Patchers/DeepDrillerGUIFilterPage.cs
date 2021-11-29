using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Structs;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUIFilterPage : DeepDrillerGUIPage
    {
        private FCSButton _toggleFilterBTN;
        private FCSButton _toggleBlackListBTN;
        private GridHelper _filterGrid;
        private readonly Dictionary<TechType, uGUI_FCSDisplayItem> _trackedFilterState = new();

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
        }

        private void OnLoadFilterGrid(DisplayData data)
        {
            try
            {
                if (IsBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadFilterGrid : {data.ItemsGrid}");

                if (_trackedFilterState.Count <= 0)
                {
                    //Create all filters
                    var grouped = Hud.GetSender().OreGenerator.AllowedOres;
                    foreach (TechType techType in grouped)
                    {
                        GameObject buttonPrefab = Instantiate(data.ItemsPrefab);
                        buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                        var itemBTN = buttonPrefab.AddComponent<uGUI_FCSDisplayItem>();
                        itemBTN.Initialize(techType,true);
                        itemBTN.Subscribe((value =>
                        {
                            if (value)
                            {
                                Hud.GetSender().OreGenerator.AddFocus(techType);
                            }
                            else
                            {
                                Hud.GetSender().OreGenerator.RemoveFocus(techType);
                            }
                        }));
                        itemBTN .Hide();
                        _trackedFilterState.Add(techType, itemBTN);
                    }
                }

                var allowedOres = Hud.GetSender().OreGenerator.AllowedOres;

                if (data.EndPosition > allowedOres.Count)
                {
                    data.EndPosition = allowedOres.Count;
                }

                foreach (KeyValuePair<TechType, uGUI_FCSDisplayItem> toggle in _trackedFilterState)
                {
                    toggle.Value.Hide();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedFilterState.ElementAt(i).Value.Show();
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
                if (Hud.GetSender().OreGenerator.GetFocusedOres().Contains(displayItem.Value.GetTechType()))
                {
                    displayItem.Value.Select();
                }
                else
                {
                    displayItem.Value.Deselect();
                }
            }
        }
    }
}