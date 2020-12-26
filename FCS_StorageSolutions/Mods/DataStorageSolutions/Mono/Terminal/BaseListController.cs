using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class BaseListController : MonoBehaviour
    {
        private GameObject _grid;
        private readonly List<DSSListItemController> _baseListItems = new List<DSSListItemController>();
        private BaseManager _manager;
        private DSSTerminalDisplayManager _displayController;

        internal void Initialize(BaseManager manager, DSSTerminalDisplayManager displayController)
        {
            _manager = manager;
            _displayController = displayController;
            _grid = gameObject.FindChild("Grid");
            InvokeRepeating(nameof(UpdateList),1f,1f);
        }

        private void UpdateList()
        {
            if (_manager == null) return;
            foreach (BaseManager baseManager in BaseManager.Managers)
            {
                if (baseManager.IsVisible)
                {
                    if (_baseListItems.Any(x => x.GetBaseManager() == baseManager)) continue;
                    CreateNewEntry(baseManager, _manager == baseManager);
                }
            }
        }

        private void CreateNewEntry(BaseManager baseManager, bool isCurrent)
        {
            var item = GameObject.Instantiate(ModelPrefab.DSSAvaliableVehiclesItemPrefab);
            var itemBTN = item.EnsureComponent<DSSListItemController>();
            itemBTN.Initialize(baseManager);
            itemBTN.ChangeIcon(isCurrent ? DssListItemIcon.Current : DssListItemIcon.HUB);
            itemBTN.OnButtonClick += _displayController.OnButtonClick;
            item.transform.SetParent(_grid.transform, false);
            _baseListItems.Add(itemBTN);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void DestroyEntry(BaseManager baseManager)
        {
            foreach (DSSListItemController controller in _baseListItems)
            {
                if (controller.GetBaseManager() == baseManager)
                {
                    controller.Purge();
                    _baseListItems.Remove(controller);
                }

            }
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
    }
}