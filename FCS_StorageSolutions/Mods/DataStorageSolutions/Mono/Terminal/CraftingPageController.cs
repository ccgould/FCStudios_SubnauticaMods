using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class CraftingPageController : MonoBehaviour, ITerminalPage, IFCSDisplay
    {
        private DSSTerminalDisplayManager _mono;
        private GridHelperV2 _grid;
        private readonly List<CrafterListItem> _trackedCrafterListItem = new List<CrafterListItem>();
        private PaginatorController _paginatorController;

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;

            var craftersSideBarGrid = GameObjectHelpers.FindGameObject(gameObject, "CraftersSideBarGrid");

            foreach (Transform child in craftersSideBarGrid.transform)
            {
                _trackedCrafterListItem.Add(child.gameObject.EnsureComponent<CrafterListItem>());
            }

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);


            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
            backBTN.onClick.AddListener((() =>
            {
                Hide();
                _mono.GoToTerminalPage(TerminalPages.Configuration);
            }));

            _grid = craftersSideBarGrid.EnsureComponent<GridHelperV2>();
            _grid.OnLoadDisplay += OnLoadCraftersGrid;
            _grid.Setup(8, gameObject, Color.gray, Color.white, null, "CraftersSideBarGrid");
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _grid.DrawPage();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            _grid.DrawPage();
        }

        private void OnLoadCraftersGrid(DisplayData data)
        {
            try
            {
                if (_mono == null) return;

                var grouped = _mono.GetController().Manager.GetDevices("ACU").ToList();
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _trackedCrafterListItem[i].Reset();
                }
                
                int w = 0;
                
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedCrafterListItem[w++].Set(grouped.ElementAt(i));
                }

                _grid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_grid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
        
        private class CrafterListItem : MonoBehaviour
        {
            private bool _isInitialized;
            private Text _title;
            private FcsDevice _crafter;

            private void Initialize()
            {
                if (_isInitialized) return;
                _title = gameObject.GetComponentInChildren<Text>();
                _isInitialized = true;
                InvokeRepeating(nameof(UpdateStatus),1f,1f);
            }

            private void UpdateStatus()
            {
                if (_title != null && _crafter != null)
                {
                    var info = _crafter.GetDeviceInformation();
                    _title.text = $"{info.UnitID} - {info.Status}";
                }
            }
            
            internal void Set(FcsDevice device)
            {
                Initialize();
                _crafter = device;
                gameObject.SetActive(true);
            }

            internal void Reset()
            {
                gameObject.SetActive(false);
            }
        }

        public void GoToPage(int index)
        {
            _grid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            _grid.DrawPage(index);
        }
    }
}