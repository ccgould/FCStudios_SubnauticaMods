using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Mono;
using SMLHelper.V2.Handlers;
using System;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class ARSolutionsSeaBreezeFreonContainer
    {
        private readonly ItemsContainer _freonContainer = null;

        private readonly ChildObjectIdentifier _containerRoot = null;

        private readonly Func<bool> _isContstructed;
        private readonly ARSolutionsSeaBreezeController _mono;
        private readonly bool _freonFound;
        private readonly TechType _freonTechType;
        private bool _isFreonDriveOpen;
        private InventoryItem _currentItem;
        private Freon _freon;
        private const int ContainerWidth = 1;
        private const int ContainerHeight = 1;

        public Action OnTimerEnd { get; set; }

        public Action<string> OnTimerUpdate { get; set; }

        public Action OnPDAClosedAction { get; set; }

        public Action OnPDAOpenedAction { get; set; }

        public ARSolutionsSeaBreezeFreonContainer(ARSolutionsSeaBreezeController mono)
        {
            _isContstructed = () => { return mono.IsConstructed; };

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Freon StorageRoot");
                var storageRoot = new GameObject("freonStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _mono = mono;
            }

            if (_freonContainer == null)
            {
                QuickLogger.Debug("Initializing freon Container");

                _freonContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    ARSSeaBreezeFCS32Buildable.StorageLabel(), null);

                _freonContainer.isAllowedToAdd += IsAllowedToAdd;
                _freonContainer.isAllowedToRemove += IsAllowedToRemove;

                _freonContainer.onAddItem += mono.OnAddItemEvent;
                _freonContainer.onRemoveItem += mono.OnRemoveItemEvent;

                _freonContainer.onAddItem += OnAddItemEvent;
                _freonContainer.onRemoveItem += OnRemoveItemEvent;
            }

            _freonFound = TechTypeHandler.TryGetModdedTechType("Freon_ARS", out TechType freon);

            if (!_freonFound)
            {
                QuickLogger.Error("Freon TechType not found");
            }
            else
            {
                _freonTechType = freon;
            }
        }

        private void OnRemoveItemEvent(InventoryItem item)
        {

        }

        private void OnAddItemEvent(InventoryItem item)
        {
            if (item != null)
            {
                QuickLogger.Debug("Freon Added!", true);

                _currentItem = item;
                var go = item.item.gameObject;
                _freon = go.GetComponent<Freon>();

                if (_freonFound)
                {
                    if (_freon != null && _mono.CoolantIsDone)
                    {
                        UseFreon();
                    }
                    else
                    {
                        ErrorMessage.AddMessage(LanguageHelpers.GetLanguage(ARSSeaBreezeFCS32Buildable.CannotRefillMessageKey));
                    }
                }
                else
                {
                    ErrorMessage.AddMessage(LanguageHelpers.GetLanguage(ARSSeaBreezeFCS32Buildable.FcsWorkBenchErrorMessageKey));
                }
            }
        }

        private void UseFreon()
        {
            if (_currentItem == null || _freon == null) return;
            _mono.InitializeTimer(_freon.Time);
            //_freonContainer.RemoveItem(_currentItem.item);
            UnityEngine.Object.Destroy(_currentItem.item.gameObject);
            _currentItem = null;
            _freon = null;
            QuickLogger.Message(LanguageHelpers.GetLanguage(ARSSeaBreezeFCS32Buildable.AddedFreonMessageKey), true);
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
                var filter = pickupable.gameObject.GetComponent<Freon>();
                if (filter != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("Alterra Refrigeration Freon allowed only");
            return flag;
        }

        public void OpenStorage()
        {
            if (!_isContstructed.Invoke())
                return;

            _mono.StopTimer();
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_freonContainer, false);
            pda.Open(PDATab.Inventory, null, OnPDAClose, 4f);
            OnPDAOpenedAction?.Invoke();
            _isFreonDriveOpen = true;
        }

        private void OnPDAClose(PDA pda)
        {
            _mono.StartTimer();
            OnPDAClosedAction?.Invoke();
            _isFreonDriveOpen = false;
        }

        internal bool GetOpenState()
        {
            return _isFreonDriveOpen;
        }

        internal void LoadFreon(SaveData save)
        {
            QuickLogger.Debug($"Loading Freon {save.FreonCount}");

            if (save.FreonCount == 0) return;

            if (_freonFound)
            {
                QuickLogger.Debug($"Attempting to add {save.FreonCount} freon");
                var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(_freonTechType));
                var newInventoryItem = new InventoryItem(go.GetComponent<Pickupable>().Pickup(false));
                _freonContainer.UnsafeAdd(newInventoryItem);
            }
            else
            {
                ErrorMessage.AddMessage(LanguageHelpers.GetLanguage(ARSSeaBreezeFCS32Buildable.FcsWorkBenchErrorMessageKey));
            }
        }

        internal int GetFreonCount()
        {
            return _freonContainer.count;
        }

        internal bool CheckIfFreonAvailable()
        {
            if (_freon == null) return false;
            QuickLogger.Debug("Freon Available", true);
            UseFreon();
            return true;
        }
    }
}
