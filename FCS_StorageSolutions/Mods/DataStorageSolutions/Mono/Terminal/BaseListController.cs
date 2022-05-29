using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class BaseListController : MonoBehaviour, IFCSDisplay
    {
        private GameObject _grid;
        private BaseManager _manager;
        private DSSTerminalDisplayManager _displayController;
        private List<DSSListItemController> _baseButtons = new List<DSSListItemController>();
        private GridHelperV2 _baseGrid;
        private PaginatorController _paginatorController;

        internal void Initialize(BaseManager manager, DSSTerminalDisplayManager displayController)
        {
            _manager = manager;
            _displayController = displayController;
            _grid = gameObject.FindChild("Grid");

            foreach (Transform child in _grid.transform)
            {
                var baseBTN = child.gameObject.EnsureComponent<DSSListItemController>();
                baseBTN.OnButtonClick += (s,o) =>
                {
                    _displayController.OnButtonClick(s, o);
                    Hide();
                };
                child.SetParent(_grid.transform, false);
                _baseButtons.Add(baseBTN);
            }
            _baseGrid = _grid.EnsureComponent<GridHelperV2>();
            _baseGrid.OnLoadDisplay += OnLoadItemsGrid;
            _baseGrid.Setup(8, gameObject, Color.gray, Color.white, null);


            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            displayController.GetController().IPCMessage += IpcMessage;
        }

        private void IpcMessage(string message)
        {
            if (message.Equals("BaseUpdate"))
            {
                UpdateList();
            }
        }

        internal void UpdateList()
        {
            if (_manager == null) return;
            _baseGrid.DrawPage();
        }


        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if(_manager == null ) return;

                var grouped = new List<BaseManager>();
                
                if (_manager.IsVisible)
                {
                    grouped = BaseManager.Managers.Where(x=>x.IsVisible && x != _manager && x.Habitat.name.Contains("(Clone)")).ToList();
                    //grouped = BaseManager.Managers.Where(x => x.IsVisible && x != _manager && !x.GetBaseName().Equals("Cyclops 0", StringComparison.OrdinalIgnoreCase)).ToList();


                }

                grouped.Insert(0,_manager);

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                QuickLogger.Debug("1");

                foreach (DSSListItemController dssListItemController in _baseButtons)
                {
                    dssListItemController.Reset();
                }

                QuickLogger.Debug("2"); 

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    QuickLogger.Debug($"Set: {i}");
                    _baseButtons[w++].Set(grouped[i], grouped[i] == BaseManager.FindManager(Player.main.currentSub), _displayController);
                }

                QuickLogger.Debug("3");

                if (_paginatorController != null && _baseGrid != null)
                {
                    _baseGrid.UpdaterPaginator(grouped.Count);
                    QuickLogger.Debug("4");
                    _paginatorController.ResetCount(_baseGrid.GetMaxPages());
                    QuickLogger.Debug("5");
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ToggleVisibility()
        {
            if (gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void OnDestroy()
        {
            _displayController.GetController().IPCMessage -= IpcMessage;
        }

        public void GoToPage(int index)
        {
            _baseGrid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {

        }
    }
}