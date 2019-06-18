using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Components;
using FCSCommon.Converters;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechWorkBench.Mono;
using SMLHelper.V2.Handlers;
using System;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class ARSolutionsSeaBreezeFilterContainer
    {
        private readonly ItemsContainer _filterContainer = null;

        private readonly ChildObjectIdentifier _containerRoot = null;

        private readonly Func<bool> _isContstructed;
        private Filter _filter;
        private readonly ARSolutionsSeaBreezeController _mono;
        private readonly bool _longTermFilterFound;
        private readonly TechType _longTermFilterTechType;
        private readonly bool _shortTermFilterFound;
        private readonly TechType _shortTermFilterTechType;
        private bool _isFilterDriveOpen;
        private TechType _filterTechType;
        private const int ContainerWidth = 1;

        private const int ContainerHeight = 1;

        public Action OnTimerEnd { get; set; }

        public Action<string> OnTimerUpdate { get; set; }

        public Action OnPDAClosedAction { get; set; }

        public Action OnPDAOpenedAction { get; set; }

        public ARSolutionsSeaBreezeFilterContainer(ARSolutionsSeaBreezeController mono)
        {
            _isContstructed = () => { return mono.IsConstructed; };

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Filter StorageRoot");
                var storageRoot = new GameObject("FilterStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _mono = mono;
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

                _filterContainer.onAddItem += OnAddItemEvent;
                _filterContainer.onRemoveItem += OnRemoveItemEvent;
            }

            _longTermFilterFound = TechTypeHandler.TryGetModdedTechType("LongTermFilter_ARS", out TechType longTermfilter);

            if (!_longTermFilterFound)
            {
                QuickLogger.Error("LongTermFilter TechType not found");
            }
            else
            {
                _longTermFilterTechType = longTermfilter;
            }

            _shortTermFilterFound = TechTypeHandler.TryGetModdedTechType("ShortTermFilter_ARS", out TechType shortTermfilter);

            if (!_shortTermFilterFound)
            {
                QuickLogger.Error("LongTermFilter TechType not found");
            }
            else
            {
                _shortTermFilterTechType = shortTermfilter;
            }

            _mono.OnMonoUpdate += OnMonoUpdate;
        }

        private void OnMonoUpdate()
        {
            if (_filter != null)
            {
                _filter.UpdateTimer();
                _mono.UpdateDisplayTimer(_filter.RemainingTime);
            }
            else
            {
                _mono.UpdateDisplayTimer(TimeConverters.SecondsToHMS(0));
            }
        }

        private void OnRemoveItemEvent(InventoryItem item)
        {
            if (_filter == null) return;
            _filter.StopTimer();
            _filter = null;
        }

        private void OnAddItemEvent(InventoryItem item)
        {
            if (item != null)
            {
                QuickLogger.Debug("Filter Added!", true);

                GameObject go = item.item.gameObject;
                var filter = go.GetComponent<Filter>();

                if (_longTermFilterFound || _shortTermFilterFound)
                {
                    if (item.item.GetTechType() == _longTermFilterTechType)
                    {
                        var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>().Id;
                        QuickLogger.Debug($"Initializing {prefabId} in OnAddEvent", true);

                        if (filter == null)
                        {
                            _filter = go.AddComponent<LongTermFilterController>();
                            _filter.Initialize();
                        }
                        else
                        {
                            _filter = filter;
                        }

                        _filterTechType = _longTermFilterTechType;
                    }
                    else if (item.item.GetTechType() == _shortTermFilterTechType)
                    {
                        var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>().Id;
                        QuickLogger.Debug($"Initializing {prefabId} in OnAddEvent", true);

                        if (filter == null)
                        {
                            _filter = go.AddComponent<ShortTermFilterController>();
                            _filter.Initialize();
                        }
                        else
                        {
                            _filter = filter;
                        }

                        _filterTechType = _shortTermFilterTechType;
                    }
                }

                QuickLogger.Debug($"Filter Type: {_filter.FilterType} || Filter Max Time {_filter.GetMaxTime()} || Filter Time Remaining {_filter.GetRemainingTime()}", true);
            }
        }

        private void TimerEnd()
        {
            OnTimerEnd?.Invoke();
        }

        private void TimerTick(string obj)
        {
            OnTimerUpdate?.Invoke(obj);
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
                var filter = pickupable.gameObject.GetComponent<WorkBenchFilter>();
                if (filter != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("Alterra Refrigeration Short/Long Filters allowed only");
            return flag;
        }

        public void OpenStorage()
        {
            //QuickLogger.Debug($"Filter Button Clicked", true);

            if (!_isContstructed.Invoke())
                return;

            if (_filter != null)
            {
                _filter.StopTimer();
            }

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_filterContainer, false);
            pda.Open(PDATab.Inventory, null, OnPDAClose, 4f);
            OnPDAOpenedAction?.Invoke();
            _isFilterDriveOpen = true;
        }

        private void OnPDAClose(PDA pda)
        {
            if (_filter != null)
            {
                _filter.StartTimer();
            }
            OnPDAClosedAction?.Invoke();
            _isFilterDriveOpen = false;
        }

        internal FilterState GetFilterState()
        {
            if (_filter == null) return FilterState.None;
            return _filter.FilterState;
        }

        internal bool GetOpenState()
        {
            return _isFilterDriveOpen;
        }

        public FilterTypes GetFilterType()
        {
            if (_filter == null) return FilterTypes.None;
            return _filter.FilterType;
        }

        internal string GetFilterPrebID()
        {
            return _filter == null ? string.Empty : _filter.PrefabId.Id;
        }

        internal void LoadFilter(SaveData save)
        {
            QuickLogger.Debug($"Loading Filter {save.FilterType}");

            if (save.FilterType == FilterTypes.None) return;

            var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(save.FilterTechType));

            bool flag = false;

            switch (save.FilterType)
            {
                case FilterTypes.LongTermFilter:
                    _filter = go.AddComponent<LongTermFilterController>();
                    flag = true;
                    break;
                case FilterTypes.ShortTermFilter:
                    _filter = go.AddComponent<ShortTermFilterController>();
                    flag = true;
                    break;
                case FilterTypes.None:
                    _filter = null;
                    flag = false;
                    break;
            }

            if (!flag) return;
            _filter.MaxTime = save.RemaingTime;
            QuickLogger.Debug($"// ============================== FMT{_filter.MaxTime} || SMT{save.RemaingTime} ============================== //");
            _filter.FilterState = save.FilterState;
            _filter.Initialize(true);
            //_filter.SetRemainingTime();
            _filter.StartTimer();

            var newInventoryItem = new InventoryItem(_filter.GetComponent<Pickupable>().Pickup(false));
            _filterContainer.UnsafeAdd(newInventoryItem);
        }

        public float GetFilterTime()
        {
            if (_filter == null) return 0f;
            return _filter.MaxTime;
        }

        public TechType GetFilterTechType()
        {
            if (_filter == null) return TechType.None;
            return _filterTechType;
        }
    }
}
