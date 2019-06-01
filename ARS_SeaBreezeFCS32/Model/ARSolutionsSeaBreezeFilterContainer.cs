using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Utilities;
using FCSTechWorkBench.Mono;
using System;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class ARSolutionsSeaBreezeFilterContainer
    {
        private ItemsContainer _filterContainer = null;

        private ChildObjectIdentifier _containerRoot = null;

        private Func<bool> isContstructed;

        private const int ContainerWidth = 1;

        private const int ContainerHeight = 1;

        public Action OnPDAClosedAction { get; set; }

        public Action OnPDAOpenedAction { get; set; }

        public ARSolutionsSeaBreezeFilterContainer(ARSolutionsSeaBreezeController mono)
        {
            isContstructed = () => { return mono.IsConstructed; };

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Filter StorageRoot");
                var storageRoot = new GameObject("FilterStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_filterContainer == null)
            {
                QuickLogger.Debug("Initializing Filter Container");

                _filterContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    ARSSeaBreezeFCS32Buildable.StorageLabel(), null);

                _filterContainer.isAllowedToAdd += IsAllowedToAdd;
                _filterContainer.isAllowedToRemove += IsAllowedToRemove;

                _filterContainer.onAddItem += mono.OnAddItemEvent;
                _filterContainer.onRemoveItem += mono.OnRemoveItemEvent;
            }
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                if (pickupable.gameObject.GetComponent<Filter>() != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("Alterra Refrigeration Short/Long Filters allowed only");
            return flag;
        }

        public void OpenStorage()
        {
            QuickLogger.Debug($"Filter Button Clicked", true);

            if (!isContstructed.Invoke())
                return;

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_filterContainer, false);
            pda.Open(PDATab.Inventory, null, OnPDAClose, 4f);
            OnPDAOpenedAction?.Invoke();
        }

        private void OnPDAClose(PDA pda)
        {
            OnPDAClosedAction?.Invoke();
        }
    }
}
