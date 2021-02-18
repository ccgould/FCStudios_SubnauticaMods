using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class BaseListController : MonoBehaviour
    {
        private GameObject _grid;
        private BaseManager _manager;
        private DSSTerminalDisplayManager _displayController;
        private List<DSSListItemController> _baseButtons = new List<DSSListItemController>();
        private GridHelperV2 _baseGrid;

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
                if(_manager == null)return;

                var grouped = new List<BaseManager>();
                
                if (_manager.IsVisible)
                {
                    grouped = BaseManager.Managers.Where(x=>x.IsVisible && x != _manager && !x.GetBaseName().Equals("Cyclops 0", StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                grouped.Insert(0,_manager);

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _baseButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _baseButtons[i].Set(grouped[i], grouped[i] == BaseManager.FindManager(Player.main.currentSub), _displayController);
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
    }
}